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
using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace CaeloWorks.NINA.Benchmark.Core {

    /// <summary>Persists the benchmark history as JSON under %localappdata%\NINA\BenchmarkPlugin.</summary>
    public class BenchmarkResultStore {
        public const int MaxEntries = 25;

        private readonly string filePath;
        private static readonly JsonSerializerOptions JsonOptions = new() {
            WriteIndented = true,
            // Keep accented text (CPU/GPU/power-plan names) readable instead of \uXXXX-escaped.
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        public BenchmarkResultStore() {
            var dir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "NINA", "BenchmarkPlugin");
            filePath = Path.Combine(dir, "history.json");
        }

        public List<BenchmarkResult> Load() {
            try {
                if (!File.Exists(filePath)) { return new List<BenchmarkResult>(); }
                var json = File.ReadAllText(filePath);
                return JsonSerializer.Deserialize<List<BenchmarkResult>>(json) ?? new List<BenchmarkResult>();
            } catch {
                return new List<BenchmarkResult>();
            }
        }

        public void Save(IEnumerable<BenchmarkResult> results) {
            try {
                Directory.CreateDirectory(Path.GetDirectoryName(filePath));
                var json = JsonSerializer.Serialize(results, JsonOptions);
                File.WriteAllText(filePath, json);
            } catch { /* best effort; never crash a benchmark over persistence */ }
        }
    }
}
