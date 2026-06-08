# N.I.N.A. Benchmark plugin

A plugin for [N.I.N.A.](https://nighttime-imaging.eu/) that benchmarks your machine by timing the
**genuine N.I.N.A. / Accord image-analysis primitives** that make up the post-capture pipeline, over
a set of test frames. Each function is invoked exactly the way N.I.N.A. invokes it (same
input pixel formats, same constructor arguments, same chain order):

| Function | Role |
|----------|------|
| `BayerFilter16bpp` | debayer (OSC frames) |
| `ColorRemappingGeneral` | auto-stretch pixel remap |
| `FastGaussianBlur` | noise reduction |
| `ResizeBicubic` | downscale to detection size |
| `CannyEdgeDetector` / `NoBlurCannyEdgeDetector` | edge detection |
| `SISThreshold` | thresholding |
| `BinaryDilation3x3` | dilation |
| `Convolution` | Laplacian-of-Gaussian kernel pass |
| `BlobCounter` | structure / blob detection |
| `StarDetection` | full detector (HFR + star count), shown but excluded from the score as a superset |

Each function is timed (one warm-up pass, then the mean of N runs) and aggregated into a per-function
breakdown plus an overall score (`100000 / sum-of-primitive-ms`, higher is faster), so different
machines (mini-PCs, NUCs, laptops) can be compared on the real acquisition workload.

The plugin surfaces two blocks, **system information** (CPU, Windows edition, frequency, power plan,
GPU, RAM) and the **benchmark results** (per-function breakdown of the latest run, best score, run
history, plus *Run benchmark* and *Clear all*), both on the plugin page (Plugins tab) and as
**dockables on the Imaging view**. A single shared `BenchmarkEngine` backs all three views.

## Installation

The plugin isn't in N.I.N.A.'s built-in plugin manager yet, so for now install it manually from a
release:

1. Close N.I.N.A.
2. Download `CaeloWorks.NINA.Benchmark.dll` from the
   [latest release](https://github.com/caelo-works/nina.plugin.benchmark/releases/latest).
3. Copy it into `%localappdata%\NINA\Plugins\3.0.0\` (create the folder if it doesn't exist).
4. Start N.I.N.A. The plugin shows up under **Plugins**, and as dockables on the Imaging view.

On first use, open the **Benchmark results** panel and click **Download test set** to fetch the test
frames (about 190 MB, cached and reused afterwards).

## Test frames

The test frames are **not** bundled with the plugin. On first use you click **Download test set** in
the *Benchmark results* panel; the frames (~190 MB) are fetched once from the sharing site's
`/api/testset` manifest into `%localappdata%\NINA\BenchmarkPlugin\TestImages` and cached. Their
sha256 is re-verified before every run; if a frame is missing or corrupted the plugin shows a
warning and the download button again. Run history is persisted as JSON under
`%localappdata%\NINA\BenchmarkPlugin\history.json`.

## License

[MPL-2.0](LICENSE.txt), same as N.I.N.A.
