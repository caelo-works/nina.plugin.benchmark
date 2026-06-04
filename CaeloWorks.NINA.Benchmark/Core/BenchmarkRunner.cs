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
using System.Threading;
using System.Threading.Tasks;
using NINA.Core.Enum;
using NINA.Core.Model;
using NINA.Image.ImageAnalysis;
using NINA.Image.Interfaces;
using NINA.Profile.Interfaces;

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
    /// Runs N.I.N.A.'s genuine post-capture image pipeline over a set of test frames and times each
    /// step. Nothing is re-implemented here: the exact same factory, debayer, statistics, stretch and
    /// star-detection code paths used during a real exposure are exercised.
    /// </summary>
    public class BenchmarkRunner {
        private readonly IProfileService profileService;
        private readonly IImageDataFactory imageDataFactory;
        private readonly IStarDetection starDetection;

        public BenchmarkRunner(IProfileService profileService, IImageDataFactory imageDataFactory, IStarDetection starDetection) {
            this.profileService = profileService;
            this.imageDataFactory = imageDataFactory;
            this.starDetection = starDetection;
        }

        public async Task<BenchmarkResult> RunAsync(IReadOnlyList<TestFrame> frames, int iterations,
            IProgress<BenchmarkProgress> progress, CancellationToken token) {

            if (frames == null || frames.Count == 0) {
                throw new InvalidOperationException("No test frames found. Add FITS/XISF frames to the plugin's TestImages folder.");
            }
            if (iterations < 1) { iterations = 1; }

            var imageSettings = profileService.ActiveProfile.ImageSettings;
            var detectionProgress = new Progress<ApplicationStatus>();

            // The first pass warms caches/JIT and is excluded from the measurement when possible.
            var measuredIterations = iterations > 1 ? iterations - 1 : 1;
            double sumLoad = 0, sumDebayer = 0, sumStats = 0, sumStretch = 0, sumDetect = 0;
            int lastStarCount = 0;

            var totalUnits = (double)iterations * frames.Count;
            var unit = 0;

            for (var i = 0; i < iterations; i++) {
                var measure = iterations == 1 || i > 0;
                lastStarCount = 0;

                foreach (var frame in frames) {
                    token.ThrowIfCancellationRequested();
                    progress?.Report(new BenchmarkProgress {
                        Fraction = unit / totalUnits,
                        Status = $"Iteration {i + 1}/{iterations} — {frame.Name}"
                    });

                    var sw = Stopwatch.StartNew();
                    var imageData = await imageDataFactory.CreateFromFile(frame.Path, 16, frame.IsBayered, RawConverterEnum.FREEIMAGE, token);
                    var loadMs = sw.Elapsed.TotalMilliseconds;

                    // Render to a bitmap source (the form star detection / stretch consume).
                    IRenderedImage rendered = imageData.RenderImage();

                    double debayerMs = 0;
                    if (frame.IsBayered) {
                        sw.Restart();
                        rendered = rendered.Debayer(saveColorChannels: false, saveLumChannel: false, bayerPattern: frame.BayerPattern);
                        debayerMs = sw.Elapsed.TotalMilliseconds;
                    }

                    sw.Restart();
                    await imageData.Statistics;
                    var statsMs = sw.Elapsed.TotalMilliseconds;

                    sw.Restart();
                    var stretched = await rendered.Stretch(imageSettings.AutoStretchFactor, imageSettings.BlackClipping, unlinked: frame.IsBayered);
                    var stretchMs = sw.Elapsed.TotalMilliseconds;

                    sw.Restart();
                    var p = new StarDetectionParams {
                        Sensitivity = imageSettings.StarSensitivity,
                        NoiseReduction = imageSettings.NoiseReduction
                    };
                    StarDetectionResult detection = await starDetection.Detect(stretched, stretched.Image.Format, p, detectionProgress, token);
                    var detectMs = sw.Elapsed.TotalMilliseconds;
                    lastStarCount += detection?.DetectedStars ?? 0;

                    if (measure) {
                        sumLoad += loadMs;
                        sumDebayer += debayerMs;
                        sumStats += statsMs;
                        sumStretch += stretchMs;
                        sumDetect += detectMs;
                    }
                    unit++;
                }
            }

            var load = sumLoad / measuredIterations;
            var debayer = sumDebayer / measuredIterations;
            var stats = sumStats / measuredIterations;
            var stretch = sumStretch / measuredIterations;
            var detect = sumDetect / measuredIterations;
            var total = load + debayer + stats + stretch + detect;

            progress?.Report(new BenchmarkProgress { Fraction = 1.0, Status = "Done" });

            return new BenchmarkResult {
                TimestampUtc = DateTime.UtcNow,
                ImageCount = frames.Count,
                Iterations = iterations,
                LoadMs = load,
                DebayerMs = debayer,
                StatisticsMs = stats,
                StretchMs = stretch,
                StarDetectionMs = detect,
                TotalMs = total,
                TotalStarsDetected = lastStarCount,
                Score = total > 0 ? (int)Math.Round(100000.0 / total) : 0
            };
        }
    }
}
