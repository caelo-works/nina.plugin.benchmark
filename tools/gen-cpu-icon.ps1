# Generates the plugin's CPU featured image (Resources/cpu.png) used in N.I.N.A.'s plugin
# list and detail page. Same chip silhouette as the dockable ImageGeometry.
Add-Type -AssemblyName System.Drawing

$size  = 256
$scale = 200.0 / 24.0   # 24-unit viewBox -> 200px glyph
$off   = 28.0           # centered in 256 with margin

$bmp = New-Object System.Drawing.Bitmap $size, $size
$g   = [System.Drawing.Graphics]::FromImage($bmp)
$g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
$g.Clear([System.Drawing.Color]::Transparent)

function Rect($x, $y, $w, $h) {
    New-Object System.Drawing.RectangleF (($x * $scale + $off), ($y * $scale + $off), ($w * $scale), ($h * $scale))
}

$path = New-Object System.Drawing.Drawing2D.GraphicsPath
$path.FillMode = [System.Drawing.Drawing2D.FillMode]::Alternate
$path.AddRectangle((Rect 5 5 14 14))            # body (outer)
$path.AddRectangle((Rect 8 8 8 8))              # hole -> frame
$path.AddRectangle((Rect 10.5 10.5 3 3))        # central die
$path.AddRectangle((Rect 8.5 3 1.5 2)); $path.AddRectangle((Rect 14 3 1.5 2))     # top pins
$path.AddRectangle((Rect 8.5 19 1.5 2)); $path.AddRectangle((Rect 14 19 1.5 2))   # bottom pins
$path.AddRectangle((Rect 3 8.5 2 1.5)); $path.AddRectangle((Rect 3 14 2 1.5))     # left pins
$path.AddRectangle((Rect 19 8.5 2 1.5)); $path.AddRectangle((Rect 19 14 2 1.5))   # right pins

$brush = New-Object System.Drawing.SolidBrush ([System.Drawing.Color]::FromArgb(255, 46, 155, 230))
$g.FillPath($brush, $path)
$g.Dispose()

# GDI+ cannot write to a PowerShell provider-qualified path, so save to a local temp first.
$tmp = Join-Path $env:TEMP 'cpu.png'
$bmp.Save($tmp, [System.Drawing.Imaging.ImageFormat]::Png)
$bmp.Dispose()

$dir = Join-Path $PWD.ProviderPath 'CaeloWorks.NINA.Benchmark\Resources'
New-Item -ItemType Directory -Force -Path $dir | Out-Null
$out = Join-Path $dir 'cpu.png'
Copy-Item $tmp $out -Force
Write-Output "saved $out"
