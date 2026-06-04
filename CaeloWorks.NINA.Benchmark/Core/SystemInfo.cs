#region "copyright"
/*
    Copyright © 2026 CaeloWorks

    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org/MPL/2.0/.
*/
#endregion "copyright"
using System.Text;

namespace CaeloWorks.NINA.Benchmark.Core {

    /// <summary>Snapshot of the host machine, captured next to every benchmark run.</summary>
    public class SystemInfo {
        public string Cpu { get; set; }
        public int PhysicalCores { get; set; }
        public int LogicalCores { get; set; }
        public double MaxClockMhz { get; set; }
        public string Gpu { get; set; }
        public double TotalRamGb { get; set; }
        public double AvailableRamGb { get; set; }
        public string PowerPlan { get; set; }
        public string Os { get; set; }
        public string DotNet { get; set; }
        public string HostVersion { get; set; }

        /// <summary>Multi-line human readable summary used by the monospace info block.</summary>
        public string Summary {
            get {
                var sb = new StringBuilder();
                sb.AppendLine($"CPU         : {Cpu}");
                sb.AppendLine($"Cores       : {PhysicalCores} physical / {LogicalCores} logical");
                if (MaxClockMhz > 0) {
                    sb.AppendLine($"Max clock   : {MaxClockMhz / 1000.0:0.00} GHz");
                }
                sb.AppendLine($"GPU         : {Gpu}");
                sb.AppendLine($"RAM         : {AvailableRamGb:0.0} GB free / {TotalRamGb:0.0} GB total");
                sb.AppendLine($"Power plan  : {PowerPlan}");
                sb.AppendLine($"OS          : {Os}");
                sb.AppendLine($".NET        : {DotNet}");
                sb.Append($"N.I.N.A.    : {HostVersion}");
                return sb.ToString();
            }
        }
    }
}
