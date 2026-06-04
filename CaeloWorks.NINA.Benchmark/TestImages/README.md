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
| File | Camera | Size | Mode |
|------|--------|------|------|
| `OSC-16Mp.fits`  | ToupTek ATR585C (OSC)     | 3840×2160, 16-bit | bayered (RGGB) → debayered |
| `MONO-16Mp.fits` | ZWO ASI585MM Pro (mono)   | 3840×2160, 16-bit | mono → no debayer |
