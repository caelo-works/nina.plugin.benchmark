#region "copyright"
/*
    Copyright © 2026 CaeloWorks

    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org/MPL/2.0/.
*/
#endregion "copyright"
using System.Reflection;
using System.Runtime.InteropServices;

// [MANDATORY] The name that will be displayed for the plugin. Also used as the prefix
// for the plugin options DataTemplate key ("Benchmark_Options").
[assembly: AssemblyTitle("Benchmark")]

// [MANDATORY] A unique, immutable identifier for this plugin. NEVER change this once published.
[assembly: Guid("5ebd0d69-a343-472f-b4b6-487a63249448")]

// [MANDATORY] The plugin version, "Major.Minor.Patch.Build".
[assembly: AssemblyVersion("0.1.0.0")]
[assembly: AssemblyFileVersion("0.1.0.0")]

// [MANDATORY] The minimum N.I.N.A. version this plugin is compatible with.
[assembly: AssemblyMetadata("MinimumApplicationVersion", "3.0.0.0")]

// [MANDATORY] Author / company.
[assembly: AssemblyCompany("CaeloWorks")]
[assembly: AssemblyProduct("CaeloWorks N.I.N.A. Benchmark")]
[assembly: AssemblyCopyright("Copyright © 2026 CaeloWorks")]

// [MANDATORY] A short one-line summary shown in the plugin list.
[assembly: AssemblyMetadata("ShortDescription", "Benchmarks your machine by running N.I.N.A.'s real image-processing pipeline (load, debayer, statistics, stretch, star detection) over bundled test frames.")]

// [OPTIONAL] A longer description (markdown supported by N.I.N.A.).
[assembly: AssemblyMetadata("LongDescription", @"This plugin measures how fast your machine processes images the same way N.I.N.A. does after every exposure.

For each bundled test frame it runs the genuine N.I.N.A. internals:
- file load/decode,
- bayer debayering (for OSC frames),
- image statistics (mean/median/stddev/MAD/histogram),
- auto-stretch,
- star detection (HFR + star count).

Each step is timed and aggregated into per-step results and an overall score, so you can compare mini-PCs, NUCs and laptops on the real acquisition workload. A system-information panel (CPU, frequency, power plan, GPU, RAM) and the history of the latest runs are available both on the plugin page and as dockables on the Imaging view.")]

// [OPTIONAL] Metadata.
[assembly: AssemblyMetadata("Author", "CaeloWorks")]
[assembly: AssemblyMetadata("License", "MPL-2.0")]
[assembly: AssemblyMetadata("LicenseURL", "https://www.mozilla.org/en-US/MPL/2.0/")]
[assembly: AssemblyMetadata("Repository", "https://github.com/CaeloWorks/nina.plugin.benchmark")]
[assembly: AssemblyMetadata("Homepage", "https://github.com/CaeloWorks/nina.plugin.benchmark")]
[assembly: AssemblyMetadata("Tags", "benchmark,performance,diagnostics,imaging")]

[assembly: ComVisible(false)]
