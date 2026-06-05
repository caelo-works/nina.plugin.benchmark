#region "copyright"
/*
    Copyright © 2026 CaeloWorks

    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org/MPL/2.0/.
*/
#endregion "copyright"
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Accord.Imaging;
using Accord.Imaging.Filters;
using NINA.Core.Enum;
using NINA.Core.Model;
using NINA.Image.ImageAnalysis;
using NINA.Image.Interfaces;
using NINA.Profile.Interfaces;
using DrawingPixelFormat = System.Drawing.Imaging.PixelFormat;

namespace CaeloWorks.NINA.Benchmark.Core {

    /// <summary>A bundled test frame and how it should be processed.</summary>
    public class TestFrame {
        public string Path { get; set; }
        public bool IsBayered { get; set; }
        public SensorType BayerPattern { get; set; } = SensorType.RGGB;
        public string Name { get; set; }
    }

    /// <summary>Progress update emitted while a benchmark is running.</summary>
    public class BenchmarkProgress {
        public double Fraction { get; set; }
        public string Status { get; set; }
    }

    /// <summary>
    /// Times the individual NINA / Accord image primitives that make up the post-capture pipeline,
    /// invoking them exactly as <c>StarDetection</c>/<c>ImageUtility</c> do (same input pixel formats,
    /// same constructor arguments, same chain order). Nothing is re-implemented.
    /// </summary>
    public class BenchmarkRunner {
        // Canonical detection downscale width, matching StarDetection._maxWidth.
        private const int MaxWidth = 1552;

        // Function labels (kept stable for the history JSON).
        private const string FnBayer = "BayerFilter16bpp (debayer)";
        private const string FnColorRemap = "ColorRemappingGeneral (stretch)";
        private const string FnFastBlur = "FastGaussianBlur";
        private const string FnResize = "ResizeBicubic";
        private const string FnCanny = "CannyEdgeDetector";
        private const string FnNoBlurCanny = "NoBlurCannyEdgeDetector";
        private const string FnSis = "SISThreshold";
        private const string FnDilation = "BinaryDilation3x3";
        private const string FnConvolution = "Convolution (LoG 5x5)";
        private const string FnBlob = "BlobCounter";
        private const string FnStarDetection = "StarDetection (full)";

        // Representative 5x5 Laplacian-of-Gaussian kernel for the Convolution micro-benchmark
        // (NINA defines a LoG builder but no longer feeds Accord's Convolution itself).
        private static readonly int[,] LogKernel5 = {
            {  0,  0, -1,  0,  0 },
            {  0, -1, -2, -1,  0 },
            { -1, -2, 16, -2, -1 },
            {  0, -1, -2, -1,  0 },
            {  0,  0, -1,  0,  0 }
        };

        private readonly IProfileService profileService;
        private readonly IImageDataFactory imageDataFactory;
        private readonly IStarDetection starDetection;

        public BenchmarkRunner(IProfileService profileService, IImageDataFactory imageDataFactory, IStarDetection starDetection) {
            this.profileService = profileService;
            this.imageDataFactory = imageDataFactory;
            this.starDetection = starDetection;
        }

        public async Task<BenchmarkResult> RunAsync(IReadOnlyList<TestFrame> frames, int runs,
            IProgress<BenchmarkProgress> progress, CancellationToken token) {

            if (frames == null || frames.Count == 0) {
                throw new InvalidOperationException("No test frames found. Download the test set first.");
            }
            if (runs < 1) { runs = 1; }

            var acc = new FunctionAccumulator();
            var lastStars = 0;

            for (var i = 0; i < frames.Count; i++) {
                token.ThrowIfCancellationRequested();
                var frame = frames[i];
                progress?.Report(new BenchmarkProgress {
                    Fraction = (double)i / frames.Count,
                    Status = $"Processing {frame.Name} ({i + 1}/{frames.Count})…"
                });

                lastStars += await RunFrameAsync(frame, runs, acc, progress, i, frames.Count, token);
            }

            var functions = acc.Build();
            var total = functions.Where(f => f.IncludeInTotal && f.Applicable).Sum(f => f.Ms);

            progress?.Report(new BenchmarkProgress { Fraction = 1.0, Status = "Done" });

            return new BenchmarkResult {
                TimestampUtc = DateTime.UtcNow,
                ImageCount = frames.Count,
                Runs = runs,
                Functions = functions,
                TotalMs = total,
                TotalStarsDetected = lastStars,
                // One decimal of precision so close machines don't tie on a whole number.
                Score = total > 0 ? Math.Round(100000.0 / total, 1, MidpointRounding.AwayFromZero) : 0
            };
        }

        private async Task<int> RunFrameAsync(TestFrame frame, int runs, FunctionAccumulator acc,
            IProgress<BenchmarkProgress> progress, int frameIndex, int frameCount, CancellationToken token) {

            var imageData = await imageDataFactory.CreateFromFile(frame.Path, 16, frame.IsBayered, RawConverterEnum.FREEIMAGE, token);

            var src16 = imageData.RenderBitmapSource();                                        // Gray16 BitmapSource
            Bitmap bmp16 = null, bmp8full = null, bmp8small = null, cannyBase = null, sisBase = null, prepared = null;
            var starCount = 0;
            try {
                // 16bpp gray GDI+ bitmap (input for debayer + stretch remap).
                bmp16 = ImageUtility.BitmapFromSource(src16, DrawingPixelFormat.Format16bppGrayScale);
                // 8bpp indexed grayscale (input for the whole structure-detection chain).
                bmp8full = ImageUtility.Convert16BppTo8Bpp(src16);

                void Step(string s) => progress?.Report(new BenchmarkProgress {
                    Fraction = (double)frameIndex / frameCount,
                    Status = $"{frame.Name}: {s}"
                });

                // --- Debayer (OSC frames only) ---
                if (frame.IsBayered) {
                    Step("BayerFilter16bpp");
                    var pattern = BayerPatternFor(frame.BayerPattern);
                    acc.Add(FnBayer, TimeNew(() => {
                        var f = new BayerFilter16bpp { SaveColorChannels = false, SaveLumChannel = false, BayerPattern = pattern };
                        return f.Apply(bmp16);
                    }, runs), runs);
                }

                // --- Stretch color remapping (16bpp gray, in place) ---
                Step("ColorRemappingGeneral");
                var map = IdentityMap16();
                acc.Add(FnColorRemap, TimeInPlace(bmp16, b => new ColorRemappingGeneral(map).ApplyInPlace(b), runs), runs);

                // --- Noise reduction on full-res 8bpp (returns a new bitmap) ---
                Step("FastGaussianBlur");
                acc.Add(FnFastBlur, TimeNew(() => new FastGaussianBlur(bmp8full).Process(1), runs), runs);

                // --- Downscale to detection size ---
                var factor = bmp8full.Width > MaxWidth ? (double)MaxWidth / bmp8full.Width : 1.0;
                var tw = Math.Max(1, (int)Math.Floor(bmp8full.Width * factor));
                var th = Math.Max(1, (int)Math.Floor(bmp8full.Height * factor));
                Step("ResizeBicubic");
                acc.Add(FnResize, TimeNew(() => new ResizeBicubic(tw, th).Apply(bmp8full), runs), runs);

                // Resized base for the structure-detection chain (built once, untimed).
                bmp8small = new ResizeBicubic(tw, th).Apply(bmp8full);

                // --- Canny edge detection (in place) ---
                Step("CannyEdgeDetector");
                acc.Add(FnCanny, TimeInPlace(bmp8small, b => new CannyEdgeDetector(10, 80).ApplyInPlace(b), runs), runs);
                Step("NoBlurCannyEdgeDetector");
                acc.Add(FnNoBlurCanny, TimeInPlace(bmp8small, b => new NoBlurCannyEdgeDetector(10, 80).ApplyInPlace(b), runs), runs);

                // --- Convolution (LoG), in place ---
                Step("Convolution");
                acc.Add(FnConvolution, TimeInPlace(bmp8small, b => new Convolution(LogKernel5, 1).ApplyInPlace(b), runs), runs);

                // SISThreshold runs on a Canny output; dilation on a thresholded image; blob on the dilated one.
                cannyBase = (Bitmap)bmp8small.Clone();
                new CannyEdgeDetector(10, 80).ApplyInPlace(cannyBase);
                Step("SISThreshold");
                acc.Add(FnSis, TimeInPlace(cannyBase, b => new SISThreshold().ApplyInPlace(b), runs), runs);

                sisBase = (Bitmap)cannyBase.Clone();
                new SISThreshold().ApplyInPlace(sisBase);
                Step("BinaryDilation3x3");
                acc.Add(FnDilation, TimeInPlace(sisBase, b => new BinaryDilation3x3().ApplyInPlace(b), runs), runs);

                prepared = (Bitmap)sisBase.Clone();
                new BinaryDilation3x3().ApplyInPlace(prepared);
                Step("BlobCounter");
                acc.Add(FnBlob, TimeRead(() => {
                    var bc = new BlobCounter();
                    bc.ProcessImage(prepared);
                    starCount = bc.GetObjectsInformation().Length;
                }, runs), runs);

                // --- Full star detection (superset; excluded from the score total) ---
                // Run it on a representative image the way N.I.N.A. does after a capture: debayer (OSC)
                // then auto-stretch, then detect. Detecting on the raw, unstretched render would bury
                // faint stars in the noise and report a far lower count.
                Step("StarDetection");
                var imageSettings = profileService.ActiveProfile.ImageSettings;
                IRenderedImage forDetect = imageData.RenderImage();
                if (frame.IsBayered) {
                    forDetect = forDetect.Debayer(saveColorChannels: false, saveLumChannel: false, bayerPattern: frame.BayerPattern);
                }
                forDetect = await forDetect.Stretch(imageSettings.AutoStretchFactor, imageSettings.BlackClipping, unlinked: frame.IsBayered);
                var p = new StarDetectionParams {
                    Sensitivity = imageSettings.StarSensitivity,
                    NoiseReduction = imageSettings.NoiseReduction
                };
                var noProgress = new Progress<ApplicationStatus>();
                var detectMs = await TimeAsync(async () => {
                    var r = await starDetection.Detect(forDetect, forDetect.Image.Format, p, noProgress, token);
                    return r?.DetectedStars ?? 0;
                }, runs, v => starCount = v);
                acc.Add(FnStarDetection, detectMs, runs, includeInTotal: false);

                return starCount;
            } finally {
                prepared?.Dispose();
                sisBase?.Dispose();
                cannyBase?.Dispose();
                bmp8small?.Dispose();
                bmp8full?.Dispose();
                bmp16?.Dispose();
            }
        }

        // ---- timing helpers (1 warmup pass excluded, then mean of `runs`) ----

        private static double TimeNew(Func<Bitmap> produce, int runs) {
            using (var w = produce()) { }
            var sw = new Stopwatch();
            var total = 0d;
            for (var i = 0; i < runs; i++) {
                sw.Restart();
                var r = produce();
                sw.Stop();
                total += sw.Elapsed.TotalMilliseconds;
                r.Dispose();
            }
            return total / runs;
        }

        private static double TimeInPlace(Bitmap baseBmp, Action<Bitmap> apply, int runs) {
            using (var w = (Bitmap)baseBmp.Clone()) { apply(w); }
            var sw = new Stopwatch();
            var total = 0d;
            for (var i = 0; i < runs; i++) {
                var c = (Bitmap)baseBmp.Clone();
                sw.Restart();
                apply(c);
                sw.Stop();
                total += sw.Elapsed.TotalMilliseconds;
                c.Dispose();
            }
            return total / runs;
        }

        private static double TimeRead(Action act, int runs) {
            act();
            var sw = new Stopwatch();
            var total = 0d;
            for (var i = 0; i < runs; i++) {
                sw.Restart();
                act();
                sw.Stop();
                total += sw.Elapsed.TotalMilliseconds;
            }
            return total / runs;
        }

        private static async Task<double> TimeAsync(Func<Task<int>> act, int runs, Action<int> capture) {
            capture(await act());
            var sw = new Stopwatch();
            var total = 0d;
            for (var i = 0; i < runs; i++) {
                sw.Restart();
                capture(await act());
                sw.Stop();
                total += sw.Elapsed.TotalMilliseconds;
            }
            return total / runs;
        }

        // ---- input construction helpers ----

        private static ushort[] IdentityMap16() {
            var map = new ushort[ushort.MaxValue + 1];
            for (var i = 0; i < map.Length; i++) { map[i] = (ushort)i; }
            return map;
        }

        // Accord uses BGR ordering: RGB.R=2, RGB.G=1, RGB.B=0 (see ImageUtility.Debayer).
        private static int[,] BayerPatternFor(SensorType type) {
            switch (type) {
                case SensorType.GRBG: return new int[,] { { RGB.G, RGB.B }, { RGB.R, RGB.G } };
                case SensorType.GBRG: return new int[,] { { RGB.G, RGB.R }, { RGB.B, RGB.G } };
                case SensorType.BGGR: return new int[,] { { RGB.R, RGB.G }, { RGB.G, RGB.B } };
                case SensorType.RGGB:
                default: return new int[,] { { RGB.B, RGB.G }, { RGB.G, RGB.R } };
            }
        }

        /// <summary>Accumulates per-function timings in a stable, canonical order across frames.</summary>
        private class FunctionAccumulator {
            private readonly List<FunctionResult> order = new List<FunctionResult>();
            private readonly Dictionary<string, FunctionResult> map = new Dictionary<string, FunctionResult>();

            public void Add(string name, double ms, int runs, bool includeInTotal = true) {
                if (!map.TryGetValue(name, out var fr)) {
                    fr = new FunctionResult { Name = name, Runs = runs, IncludeInTotal = includeInTotal, Ms = 0, Applicable = true };
                    map[name] = fr;
                    order.Add(fr);
                }
                fr.Ms += ms;   // sum across frames
            }

            public List<FunctionResult> Build() => order;
        }
    }
}
