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
| `StarDetection` | full detector (HFR + star count) — shown but excluded from the score as a superset |

Each function is timed (one warm-up pass, then the mean of N runs) and aggregated into a per-function
breakdown plus an overall score (`100000 / sum-of-primitive-ms`, higher is faster), so different
machines (mini-PCs, NUCs, laptops) can be compared on the real acquisition workload.

The plugin surfaces two blocks — **system information** (CPU, Windows edition, frequency, power plan,
GPU, RAM) and the **benchmark results** (per-function breakdown of the latest run, best score, run
history, plus *Run benchmark* and *Clear all*) — both on the plugin page (Plugins tab) and as
**dockables on the Imaging view**. A single shared `BenchmarkEngine` backs all three views.

## Test frames

The test frames are **not** bundled with the plugin. On first use you click **Download test set** in
the *Benchmark results* panel; the frames (~190 MB) are fetched once from the sharing site's
`/api/testset` manifest into `%localappdata%\NINA\BenchmarkPlugin\TestImages` and cached. Their
sha256 is re-verified before every run — if a frame is missing or corrupted the plugin shows a
warning and the download button again. Run history is persisted as JSON under
`%localappdata%\NINA\BenchmarkPlugin\history.json`.

## Building

> The plugin targets `net8.0-windows` (WPF) and therefore builds on **Windows** only.

```powershell
dotnet build CaeloWorks.NINA.Benchmark/CaeloWorks.NINA.Benchmark.csproj -c Release
```

Deploy to your local N.I.N.A. plugin folder during development:

```powershell
dotnet build CaeloWorks.NINA.Benchmark/CaeloWorks.NINA.Benchmark.csproj -c Release -p:NinaDeploy=true
```

CI (GitHub Actions, `windows-latest`) compiles the plugin on every push and uploads the built DLL as
an artifact.

## License

[MPL-2.0](LICENSE.txt) — same as N.I.N.A.
