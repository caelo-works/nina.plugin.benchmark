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
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace CaeloWorks.NINA.Benchmark.Core {

    /// <summary>Timing of a single benchmarked function, summed over the frames it applied to.</summary>
    public class FunctionResult {
        public string Name { get; set; }
        /// <summary>Mean time of one application, summed across all frames the function ran on (ms).</summary>
        public double Ms { get; set; }
        public int Runs { get; set; }
        public bool Applicable { get; set; } = true;
        /// <summary>Whether this function counts toward the overall score (the full StarDetection does not — it is a superset of the primitives).</summary>
        public bool IncludeInTotal { get; set; } = true;

        public string Display => Applicable ? $"{Ms:N2} ms" : "n/a";
    }

    /// <summary>
    /// Aggregated outcome of a single benchmark run: one <see cref="FunctionResult"/> per measured
    /// NINA/Accord primitive, plus an overall score (higher is faster).
    /// </summary>
    public class BenchmarkResult : INotifyPropertyChanged {
        public DateTime TimestampUtc { get; set; }
        public int ImageCount { get; set; }
        public int Runs { get; set; }

        public List<FunctionResult> Functions { get; set; } = new List<FunctionResult>();

        public double TotalMs { get; set; }
        public int Score { get; set; }
        public int TotalStarsDetected { get; set; }

        public SystemInfo System { get; set; }

        public string DisplayTimestamp => TimestampUtc.ToLocalTime().ToString("yyyy-MM-dd HH:mm");
        public string CpuShort => System?.Cpu;

        /// <summary>UI-only: whether the row details (system snapshot + per-function timings) are expanded.</summary>
        private bool isExpanded;

        [JsonIgnore]
        public bool IsExpanded {
            get => isExpanded;
            set {
                if (isExpanded != value) {
                    isExpanded = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsExpanded)));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
