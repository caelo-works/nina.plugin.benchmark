#region "copyright"
/*
    Copyright © 2026 CaeloWorks

    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org/MPL/2.0/.
*/
#endregion "copyright"
using System;
using System.IO;
using System.Text.Json;

namespace CaeloWorks.NINA.Benchmark.Core {

    /// <summary>Submission settings for the sharing site (persisted as JSON next to the history).</summary>
    public class BenchmarkSettings {
        // The only user-configurable submission field. Endpoint is compile-time; nickname comes from
        // the NINA Observer option; if this is empty the active profile name is used as the machine label.
        public string MachineName { get; set; } = "";
    }

    public class BenchmarkSettingsStore {
        private readonly string filePath;
        private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

        public BenchmarkSettingsStore() {
            var dir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "NINA", "BenchmarkPlugin");
            filePath = Path.Combine(dir, "settings.json");
        }

        public BenchmarkSettings Load() {
            try {
                if (!File.Exists(filePath)) { return new BenchmarkSettings(); }
                return JsonSerializer.Deserialize<BenchmarkSettings>(File.ReadAllText(filePath)) ?? new BenchmarkSettings();
            } catch {
                return new BenchmarkSettings();
            }
        }

        public void Save(BenchmarkSettings settings) {
            try {
                Directory.CreateDirectory(Path.GetDirectoryName(filePath));
                File.WriteAllText(filePath, JsonSerializer.Serialize(settings, JsonOptions));
            } catch { /* best effort */ }
        }
    }
}
