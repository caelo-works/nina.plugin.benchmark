#region "copyright"
/*
    Copyright © 2026 CaeloWorks

    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org/MPL/2.0/.
*/
#endregion "copyright"
using System;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace CaeloWorks.NINA.Benchmark.Core {

    /// <summary>
    /// Collects host hardware/OS information via WMI and the .NET runtime. WMI queries are slow,
    /// so collection is performed off the UI thread.
    /// </summary>
    public class SystemInfoProvider {

        public Task<SystemInfo> GetAsync() => Task.Run(Collect);

        private SystemInfo Collect() {
            var info = new SystemInfo {
                LogicalCores = Environment.ProcessorCount,
                Os = RuntimeInformation.OSDescription,
                DotNet = RuntimeInformation.FrameworkDescription,
                HostVersion = HostVersion(),
                Cpu = "Unknown",
                Gpu = "Unknown",
                PowerPlan = "Unknown"
            };

            TryQuery("SELECT Name, NumberOfCores, MaxClockSpeed FROM Win32_Processor", mo => {
                info.Cpu = (mo["Name"]?.ToString() ?? "Unknown").Trim();
                info.PhysicalCores = ToInt(mo["NumberOfCores"]);
                info.MaxClockMhz = ToDouble(mo["MaxClockSpeed"]);
            });

            TryQuery("SELECT Name, AdapterRAM FROM Win32_VideoController", mo => {
                var name = mo["Name"]?.ToString();
                var ram = ToDouble(mo["AdapterRAM"]);
                if (!string.IsNullOrWhiteSpace(name)) {
                    info.Gpu = ram > 0 ? $"{name.Trim()} ({ram / 1024 / 1024 / 1024:0.0} GB)" : name.Trim();
                }
            }, firstOnly: true);

            TryQuery("SELECT TotalVisibleMemorySize, FreePhysicalMemory FROM Win32_OperatingSystem", mo => {
                info.TotalRamGb = ToDouble(mo["TotalVisibleMemorySize"]) / 1024 / 1024;   // KB -> GB
                info.AvailableRamGb = ToDouble(mo["FreePhysicalMemory"]) / 1024 / 1024;   // KB -> GB
            });

            info.PowerPlan = GetActivePowerPlan();
            return info;
        }

        private static string HostVersion() {
            try {
                var module = Process.GetCurrentProcess().MainModule;
                var v = module != null ? FileVersionInfo.GetVersionInfo(module.FileName).ProductVersion : null;
                return string.IsNullOrWhiteSpace(v) ? "Unknown" : v;
            } catch {
                return "Unknown";
            }
        }

        private static string GetActivePowerPlan() {
            try {
                var psi = new ProcessStartInfo("powercfg.exe", "/getactivescheme") {
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                using var p = Process.Start(psi);
                var output = p.StandardOutput.ReadToEnd();
                p.WaitForExit(3000);
                // Format: "Power Scheme GUID: <guid>  (Balanced)"
                var open = output.LastIndexOf('(');
                var close = output.LastIndexOf(')');
                if (open >= 0 && close > open) {
                    return output.Substring(open + 1, close - open - 1).Trim();
                }
            } catch { /* best effort */ }
            return "Unknown";
        }

        private static void TryQuery(string query, Action<ManagementBaseObject> map, bool firstOnly = false) {
            try {
                using var searcher = new ManagementObjectSearcher(query);
                foreach (var mo in searcher.Get().Cast<ManagementBaseObject>()) {
                    map(mo);
                    if (firstOnly) { break; }
                }
            } catch { /* WMI not available / access denied — keep defaults */ }
        }

        private static int ToInt(object o) => o != null && int.TryParse(o.ToString(), out var v) ? v : 0;

        private static double ToDouble(object o) => o != null && double.TryParse(o.ToString(), out var v) ? v : 0d;
    }
}
