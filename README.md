# N.I.N.A. Benchmark plugin

A plugin for [N.I.N.A.](https://nighttime-imaging.eu/) that benchmarks your machine by running
**N.I.N.A.'s genuine post-capture image pipeline** over a set of bundled test frames — the same
internal code paths executed after every exposure:

1. **File load / decode** (`IImageDataFactory.CreateFromFile`)
2. **Debayer** for OSC frames (`IRenderedImage.Debayer`)
3. **Image statistics** — mean/median/stddev/MAD/histogram (`IImageData.Statistics`)
4. **Auto-stretch** (`IRenderedImage.Stretch`)
5. **Star detection** — HFR + star count (`IStarDetection.Detect`)

Each step is timed and aggregated into per-step results and an overall score, so different machines
(mini-PCs, NUCs, laptops) can be compared on the real acquisition workload.

The plugin surfaces two blocks — **system information** (CPU, frequency, power plan, GPU, RAM) and
the **history of the latest runs** with a *Run benchmark* button — both on the plugin page (Plugins
tab) and as **dockables on the Imaging view**. A single shared `BenchmarkEngine` backs all three
views.

## Test frames

The plugin ships test frames in `CaeloWorks.NINA.Benchmark/TestImages/` (copied next to the DLL at
build time and read at runtime). See that folder's `README.md` for the expected format and the
OSC/mono layout convention. Run history is persisted as JSON under
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
