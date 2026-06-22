<div align="center">

# Benchmark

### Benchmark your machine on N.I.N.A.'s real image-analysis pipeline.

[![Version](https://img.shields.io/github/v/release/caelo-works/nina.plugin.benchmark?style=for-the-badge&labelColor=0f172a&color=22d3ee&label=version)](https://github.com/caelo-works/nina.plugin.benchmark/releases/latest)
[![N.I.N.A.](https://img.shields.io/badge/N.I.N.A.-%E2%89%A5%203.2-67e8f9?style=for-the-badge&labelColor=0f172a)](https://nighttime-imaging.eu/)
[![Status](https://img.shields.io/badge/status-active-34d399?style=for-the-badge&labelColor=0f172a)](https://nina-plugins.caelo.works/en/plugins/benchmark)
[![License](https://img.shields.io/badge/license-MPL--2.0-94a3b8?style=for-the-badge&labelColor=0f172a)](LICENSE.txt)
[![Website](https://img.shields.io/badge/%E2%86%92%20see%20all%20plugins-nina--plugins.caelo.works-0f172a?style=for-the-badge&labelColor=22d3ee)](https://nina-plugins.caelo.works/en)

<a href="https://nina-plugins.caelo.works/en"><img src="https://nina-plugins.caelo.works/assets/readme-banner.png" alt="CaeloWorks · N.I.N.A. Plugins" width="75%"></a>

</div>

---

## Overview

Benchmark times the genuine N.I.N.A. and Accord image-analysis routines that make up the
post-capture pipeline (debayer, stretch remap, resize, blur, Canny, SIS threshold, dilation,
blob counter, star detection) and turns them into a single comparable score. Each function is
invoked exactly the way N.I.N.A. invokes it (same pixel formats, same constructor arguments, same
chain order) over a set of test frames, so any machine (mini-PC, NUC, laptop) can be compared on
the real acquisition workload. Submit your result and see where it lands.

> 📖 **Full details, screenshots & docs:** **[nina-plugins.caelo.works/en/plugins/benchmark](https://nina-plugins.caelo.works/en/plugins/benchmark)**

## Features

| | |
|---|---|
| ⚙️ **Real N.I.N.A. primitives** | Times the actual debayer, stretch, resize, blur, edge-detection, threshold, dilation, blob-counter and star-detection routines, called exactly as N.I.N.A. calls them. No synthetic proxy. |
| 📊 **Per-function score** | One warm-up pass, then the mean of N runs per function, aggregated into a per-primitive breakdown plus an overall score (higher is faster). |
| 🖥️ **System snapshot** | Captures CPU, Windows edition, frequency, power plan, GPU and RAM alongside every run, so differences are easy to explain. |
| 🏆 **Share & compare** | Submit a run to the online leaderboard and see how your rig ranks against others, by score and by CPU. |

Test frames are not bundled with the plugin: they are downloaded once from the sharing site
(about 190 MB), cached locally, and re-verified (sha256) before every run.

## Installation

### From N.I.N.A.'s plugin manager (recommended)

1. In N.I.N.A., go to **Plugins → Available**.
2. Find **Benchmark** (CaeloWorks) in the list and click **Install**.
3. **Restart N.I.N.A.** The plugin appears under **Plugins** and as dockables on the Imaging view.

### Manual install

Download the latest `CaeloWorks.NINA.Benchmark.dll` from the
**[Releases](https://github.com/caelo-works/nina.plugin.benchmark/releases/latest)** and drop it
into your N.I.N.A. plugins folder (`%LOCALAPPDATA%\NINA\Plugins\<NINA version>\`), then restart
N.I.N.A.

> **Requires N.I.N.A. 3.2 or newer.**

## Getting started

1. Open the **Benchmark** page in the Plugins tab, or the Benchmark dockables on the Imaging view.
2. In the **Benchmark results** panel, click **Download test set** to fetch the test frames once
   (about 190 MB); they are cached and re-verified (sha256) before every run.
3. Click **Run benchmark**, review the per-function breakdown and your score, then hit **Share** to
   post it to the leaderboard.

## Links

- 🌐 **Plugin page:** [nina-plugins.caelo.works/en/plugins/benchmark](https://nina-plugins.caelo.works/en/plugins/benchmark)
- 📦 **Releases:** [github.com/caelo-works/nina.plugin.benchmark/releases](https://github.com/caelo-works/nina.plugin.benchmark/releases)
- 🏆 **Leaderboard:** [nina-benchmark-plugin.com/leaderboard](https://nina-benchmark-plugin.com/leaderboard)

## Screenshots

<div align="center">

![Benchmark options page in N.I.N.A.](https://nina-plugins.caelo.works/assets/plugins/benchmark-1-options.webp)

![System info and benchmark dockables on the Imaging view](https://nina-plugins.caelo.works/assets/plugins/benchmark-2-dockables.webp)

![Sharing a benchmark run](https://nina-plugins.caelo.works/assets/plugins/benchmark-3-share.webp)

![The online leaderboard](https://nina-plugins.caelo.works/assets/plugins/benchmark-4-leaderboard.webp)

![Top scores podium](https://nina-plugins.caelo.works/assets/plugins/benchmark-5-podium.webp)

</div>

---

<div align="center">

### 🌌 More N.I.N.A. plugins by CaeloWorks

**[Explore the full catalogue → nina-plugins.caelo.works](https://nina-plugins.caelo.works/en)**

<sub>Made by <a href="https://caelo.works">CaeloWorks</a> · astrophotography software, firmware & hardware · MPL-2.0 License</sub>

</div>
