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
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace CaeloWorks.NINA.Benchmark.Core {

    /// <summary>One frame in the test-set manifest served by the site (<c>/api/testset</c>).</summary>
    public class TestSetFileInfo {
        public string Name { get; set; }
        public long Bytes { get; set; }
        public string Sha256 { get; set; }
        public string Url { get; set; }
    }

    /// <summary>The full test-set manifest: which frames to download and how to verify them.</summary>
    public class TestSetManifest {
        public string Version { get; set; }
        public long TotalBytes { get; set; }
        public List<TestSetFileInfo> Files { get; set; } = new List<TestSetFileInfo>();
    }

    /// <summary>Progress update emitted while the test set is downloading.</summary>
    public class TestSetDownloadProgress {
        public long DoneBytes { get; set; }
        public long TotalBytes { get; set; }
        public double BytesPerSecond { get; set; }
        public string CurrentFile { get; set; }
    }

    /// <summary>
    /// Owns the locally-cached benchmark frames. The frames are no longer shipped with the
    /// plugin: they are downloaded once from the sharing site into
    /// <c>%localappdata%\NINA\BenchmarkPlugin\TestImages</c> and re-verified (sha256) before
    /// every run, so a corrupted or stale set is caught and re-downloaded.
    /// </summary>
    public class TestSetManager {
        private static readonly string[] FrameExtensions = { ".fits", ".fit", ".fts", ".xisf" };

        // Long-lived client: large downloads, so no overall timeout (cancellation drives stop).
        private static readonly HttpClient Http = CreateClient();

        private static HttpClient CreateClient() {
            var http = new HttpClient { Timeout = Timeout.InfiniteTimeSpan };
            http.DefaultRequestHeaders.UserAgent.ParseAdd(BenchmarkEndpoints.UserAgent);
            return http;
        }
        private static readonly JsonSerializerOptions JsonRead = new() { PropertyNameCaseInsensitive = true };
        private static readonly JsonSerializerOptions JsonWrite = new() { WriteIndented = true };

        private readonly string manifestPath;

        public string LocalFolder { get; }

        public TestSetManager() {
            var root = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "NINA", "BenchmarkPlugin");
            LocalFolder = Path.Combine(root, "TestImages");
            manifestPath = Path.Combine(root, "testset.json");
        }

        /// <summary>The frame files currently present in the local cache (any supported extension).</summary>
        public IReadOnlyList<string> LocalFrames() {
            if (!Directory.Exists(LocalFolder)) { return Array.Empty<string>(); }
            return Directory.EnumerateFiles(LocalFolder, "*.*", SearchOption.TopDirectoryOnly)
                .Where(f => FrameExtensions.Contains(Path.GetExtension(f).ToLowerInvariant()))
                .OrderBy(f => f)
                .ToList();
        }

        /// <summary>Version id of the currently cached set (from the stored manifest), or "" if none.</summary>
        public string Version => StoredManifest()?.Version ?? "";

        /// <summary>Total size of the cached set, for a stored-manifest-free "looks downloaded" hint.</summary>
        public TestSetManifest StoredManifest() {
            try {
                if (!File.Exists(manifestPath)) { return null; }
                return JsonSerializer.Deserialize<TestSetManifest>(File.ReadAllText(manifestPath), JsonRead);
            } catch {
                return null;
            }
        }

        /// <summary>
        /// Cheap, offline check used at startup: a stored manifest exists and every listed frame is
        /// present with the right size. (Content is verified separately via <see cref="VerifyAsync"/>.)
        /// </summary>
        public bool IsComplete() {
            var manifest = StoredManifest();
            if (manifest == null || manifest.Files.Count == 0) { return false; }
            foreach (var f in manifest.Files) {
                var path = Path.Combine(LocalFolder, f.Name);
                if (!File.Exists(path)) { return false; }
                if (new FileInfo(path).Length != f.Bytes) { return false; }
            }
            return true;
        }

        /// <summary>
        /// Full integrity check against the stored manifest: every frame present, right size, and
        /// matching sha256. Returns false if anything is missing, the wrong size, or corrupted.
        /// </summary>
        public async Task<bool> VerifyAsync(IProgress<double> progress, CancellationToken ct) {
            var manifest = StoredManifest();
            if (manifest == null || manifest.Files.Count == 0) { return false; }

            var total = manifest.Files.Sum(f => f.Bytes);
            long done = 0;
            foreach (var f in manifest.Files) {
                var path = Path.Combine(LocalFolder, f.Name);
                if (!File.Exists(path) || new FileInfo(path).Length != f.Bytes) { return false; }

                var actual = await HashFileAsync(path, total, done, progress, ct);
                if (!HexEquals(actual, f.Sha256)) { return false; }
                done += f.Bytes;
            }
            progress?.Report(1.0);
            return true;
        }

        /// <summary>
        /// Downloads the current test set from the site, verifying each frame's sha256 as it lands,
        /// then persists the manifest locally. Throws on network error or checksum mismatch.
        /// </summary>
        public async Task DownloadAsync(IProgress<TestSetDownloadProgress> progress, CancellationToken ct) {
            var manifest = await FetchManifestAsync(ct);
            if (manifest.Files.Count == 0) {
                throw new InvalidOperationException("The site reports no test frames available.");
            }

            Directory.CreateDirectory(LocalFolder);
            var total = manifest.TotalBytes > 0 ? manifest.TotalBytes : manifest.Files.Sum(f => f.Bytes);
            long done = 0;

            // Smoothed throughput: exponential moving average over the sampled instantaneous rate.
            var clock = Stopwatch.StartNew();
            var lastTicks = 0L;
            var lastDone = 0L;
            double ema = 0;

            void Report(string file) => progress?.Report(new TestSetDownloadProgress {
                DoneBytes = done, TotalBytes = total, BytesPerSecond = ema, CurrentFile = file
            });

            foreach (var f in manifest.Files) {
                ct.ThrowIfCancellationRequested();
                Report(f.Name);

                var finalPath = Path.Combine(LocalFolder, f.Name);
                var partPath = finalPath + ".part";

                using (var resp = await Http.GetAsync(f.Url, HttpCompletionOption.ResponseHeadersRead, ct))
                using (var sha = SHA256.Create()) {
                    resp.EnsureSuccessStatusCode();
                    using (var net = await resp.Content.ReadAsStreamAsync(ct))
                    using (var file = new FileStream(partPath, FileMode.Create, FileAccess.Write, FileShare.None, 1 << 20, true)) {
                        var buffer = new byte[1 << 20];
                        int read;
                        while ((read = await net.ReadAsync(buffer, 0, buffer.Length, ct)) > 0) {
                            await file.WriteAsync(buffer, 0, read, ct);
                            sha.TransformBlock(buffer, 0, read, null, 0);
                            done += read;

                            // Sample throughput roughly every 200 ms and refresh the UI.
                            var nowTicks = clock.ElapsedMilliseconds;
                            if (nowTicks - lastTicks >= 200) {
                                var dt = (nowTicks - lastTicks) / 1000.0;
                                if (dt > 0) {
                                    var inst = (done - lastDone) / dt;
                                    ema = ema <= 0 ? inst : ema * 0.7 + inst * 0.3;
                                }
                                lastTicks = nowTicks;
                                lastDone = done;
                                Report(f.Name);
                            }
                        }
                    }
                    sha.TransformFinalBlock(Array.Empty<byte>(), 0, 0);

                    var hash = ToHex(sha.Hash);
                    if (!HexEquals(hash, f.Sha256)) {
                        TryDelete(partPath);
                        throw new InvalidOperationException(
                            $"Checksum mismatch on {f.Name}. The download may be corrupted — please retry.");
                    }
                }

                if (File.Exists(finalPath)) { File.Delete(finalPath); }
                File.Move(partPath, finalPath);
                Report(f.Name);
            }

            // Drop any stale frames no longer in the set so they aren't benchmarked.
            var keep = new HashSet<string>(manifest.Files.Select(f => f.Name), StringComparer.OrdinalIgnoreCase);
            foreach (var path in LocalFrames()) {
                if (!keep.Contains(Path.GetFileName(path))) { TryDelete(path); }
            }

            Directory.CreateDirectory(Path.GetDirectoryName(manifestPath));
            File.WriteAllText(manifestPath, JsonSerializer.Serialize(manifest, JsonWrite));
        }

        private static async Task<TestSetManifest> FetchManifestAsync(CancellationToken ct) {
            using var resp = await Http.GetAsync(BenchmarkEndpoints.TestSet, ct);
            resp.EnsureSuccessStatusCode();
            var json = await resp.Content.ReadAsStringAsync(ct);
            return JsonSerializer.Deserialize<TestSetManifest>(json, JsonRead) ?? new TestSetManifest();
        }

        private static async Task<string> HashFileAsync(string path, long total, long baseDone,
            IProgress<double> progress, CancellationToken ct) {
            using var sha = SHA256.Create();
            using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 1 << 20, true);
            var buffer = new byte[1 << 20];
            int read;
            long fileDone = 0;
            while ((read = await stream.ReadAsync(buffer, 0, buffer.Length, ct)) > 0) {
                sha.TransformBlock(buffer, 0, read, null, 0);
                fileDone += read;
                if (total > 0) { progress?.Report((double)(baseDone + fileDone) / total); }
            }
            sha.TransformFinalBlock(Array.Empty<byte>(), 0, 0);
            return ToHex(sha.Hash);
        }

        private static string ToHex(byte[] bytes) {
            var c = new char[bytes.Length * 2];
            for (var i = 0; i < bytes.Length; i++) {
                var b = bytes[i];
                c[i * 2] = HexDigit(b >> 4);
                c[i * 2 + 1] = HexDigit(b & 0xF);
            }
            return new string(c);
        }

        private static char HexDigit(int v) => (char)(v < 10 ? '0' + v : 'a' + (v - 10));

        private static bool HexEquals(string a, string b) =>
            !string.IsNullOrEmpty(a) && !string.IsNullOrEmpty(b) &&
            string.Equals(a, b, StringComparison.OrdinalIgnoreCase);

        private static void TryDelete(string path) {
            try { if (File.Exists(path)) { File.Delete(path); } } catch { /* best effort */ }
        }
    }
}
