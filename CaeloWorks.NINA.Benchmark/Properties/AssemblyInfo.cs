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
[assembly: AssemblyVersion("0.2.4.0")]
[assembly: AssemblyFileVersion("0.2.4.0")]

// [MANDATORY] The minimum N.I.N.A. version this plugin is compatible with.
[assembly: AssemblyMetadata("MinimumApplicationVersion", "3.0.0.0")]

// [MANDATORY] Author / company.
[assembly: AssemblyCompany("CaeloWorks")]
[assembly: AssemblyProduct("CaeloWorks N.I.N.A. Benchmark")]
[assembly: AssemblyCopyright("Copyright © 2026 CaeloWorks")]

// [MANDATORY] A short one-line summary shown in the plugin list.
[assembly: AssemblyMetadata("ShortDescription", "Benchmarks your machine by timing N.I.N.A.'s real image-analysis primitives (debayer, stretch remap, resize, blur, Canny, SIS threshold, dilation, blob counter, star detection) over bundled test frames.")]

// [OPTIONAL] A longer description (markdown supported by N.I.N.A.).
[assembly: AssemblyMetadata("LongDescription", @"This plugin measures how fast your machine runs the genuine N.I.N.A. / Accord image-analysis routines that make up the post-capture pipeline. Each function is invoked exactly the way N.I.N.A. invokes it (same input pixel formats, same constructor arguments, same chain order) over the bundled test frames.

Measured per frame:
- BayerFilter16bpp — debayering (OSC frames),
- ColorRemappingGeneral — the auto-stretch pixel remap,
- FastGaussianBlur — noise reduction,
- ResizeBicubic — downscale to detection size,
- CannyEdgeDetector and NoBlurCannyEdgeDetector — edge detection,
- SISThreshold — thresholding,
- BinaryDilation3x3 — dilation,
- Convolution — a Laplacian-of-Gaussian kernel pass,
- BlobCounter — structure/blob detection,
- StarDetection — the full detector (HFR + star count), shown but excluded from the score as a superset of the primitives.

Each function is timed (one warm-up pass, then the mean of N runs) and aggregated into a per-function breakdown plus an overall score, so you can compare mini-PCs, NUCs and laptops on the real acquisition workload. A system-information panel (CPU, frequency, power plan, GPU, RAM) and the history of the latest runs (with best score and a clear-all action) are available both on the plugin page and as dockables on the Imaging view.")]

// [OPTIONAL] Metadata.
[assembly: AssemblyMetadata("Author", "CaeloWorks")]
[assembly: AssemblyMetadata("License", "MPL-2.0")]
[assembly: AssemblyMetadata("LicenseURL", "https://www.mozilla.org/en-US/MPL/2.0/")]
[assembly: AssemblyMetadata("Repository", "https://github.com/CaeloWorks/nina.plugin.benchmark")]
[assembly: AssemblyMetadata("Homepage", "https://github.com/CaeloWorks/nina.plugin.benchmark")]
[assembly: AssemblyMetadata("Tags", "benchmark,performance,diagnostics,imaging")]

// Plugin icon shown in the plugin list and detail page (replaces the default puzzle piece).
// Embedded as a WPF resource and referenced via a pack URI so it works offline / from a private repo.
[assembly: AssemblyMetadata("FeaturedImageURL", "pack://application:,,,/CaeloWorks.NINA.Benchmark;component/Resources/cpu.png")]

[assembly: ComVisible(false)]
