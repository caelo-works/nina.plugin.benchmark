# Test frames

Drop the benchmark's reference light frames in this folder. They are copied next to the plugin DLL
at build time and loaded at runtime through N.I.N.A.'s real file loaders.

## Accepted formats
`.fits`, `.fit`, `.fts`, `.xisf`

## Bayer / mono classification
A frame is treated as **OSC (bayered)** — and therefore debayered during the benchmark — when its
path or file name contains one of: `osc`, `color`, `colour`, `bayer`, `rggb` (case-insensitive).
Anything else is treated as **mono** (no debayer step).

Recommended layout:

```
TestImages/
├─ osc/    <- one or more OSC light frames (RGGB)   -> exercises the debayer path
└─ mono/   <- one or more mono light frames          -> skips debayer
```

Use real light frames with actual stars so star detection (HFR + star count) is meaningful. Keep the
total size reasonable; if frames are large, enable Git LFS for `*.fits`/`*.xisf` before committing.

## Shipped frames
Tracked via **Git LFS** (see `.gitattributes`). A range of resolutions is included so the
benchmark exercises the pipeline at different image sizes.

| File | Camera | Dimensions | Mode |
|------|--------|-----------|------|
| `osc-16.fits`  | ToupTek ATR585C (OSC)   | 3840×2160 (8 MP)   | bayered (RGGB) → debayered |
| `mono-16.fits` | ZWO ASI585MM Pro (mono) | 3840×2160 (8 MP)   | mono → no debayer |
| `mono-32.fits` | ASI camera (mono)       | 4656×3520 (16 MP)  | mono → no debayer |
| `mono-120.fits`| ASI camera (mono)       | 9576×6388 (61 MP)  | mono → no debayer |

All 16-bit. To work with the repo you need Git LFS installed (`git lfs install`).
