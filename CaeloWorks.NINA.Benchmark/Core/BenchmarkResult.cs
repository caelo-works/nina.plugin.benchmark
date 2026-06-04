#region "copyright"
/*
    Copyright © 2026 CaeloWorks

    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org/MPL/2.0/.
*/
#endregion "copyright"
using System;

namespace CaeloWorks.NINA.Benchmark.Core {

    /// <summary>
    /// Aggregated outcome of a single benchmark run. Times are the mean per-iteration totals (ms)
    /// across all processed frames; the score is derived so that higher is faster.
    /// </summary>
    public class BenchmarkResult {
        public DateTime TimestampUtc { get; set; }
        public int ImageCount { get; set; }
        public int Iterations { get; set; }

        public double LoadMs { get; set; }
        public double DebayerMs { get; set; }
        public double StatisticsMs { get; set; }
        public double StretchMs { get; set; }
        public double StarDetectionMs { get; set; }
        public double TotalMs { get; set; }

        public int TotalStarsDetected { get; set; }
        public int Score { get; set; }

        public SystemInfo System { get; set; }

        public string DisplayTimestamp => TimestampUtc.ToLocalTime().ToString("yyyy-MM-dd HH:mm");
    }
}
