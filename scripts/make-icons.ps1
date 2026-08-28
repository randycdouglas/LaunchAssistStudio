# Renders the Launch Assist logo mark to PNG icons, using the same path
# geometry as wwwroot/favicon.svg.
Add-Type -AssemblyName System.Drawing

$OutDir = $args[0]
$Master = 512

$bmp = New-Object System.Drawing.Bitmap($Master, $Master)
$g = [System.Drawing.Graphics]::FromImage($bmp)
$g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
$g.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
$g.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::HighQuality
$g.Clear([System.Drawing.Color]::Transparent)

# Navy rounded-square background (rx 24 at 128 => 96 at 512)
$navy = [System.Drawing.ColorTranslator]::FromHtml("#0D1B2A")
$r = 96
$bg = New-Object System.Drawing.Drawing2D.GraphicsPath
$bg.AddArc(0, 0, 2*$r, 2*$r, 180, 90)
$bg.AddArc($Master-2*$r, 0, 2*$r, 2*$r, 270, 90)
$bg.AddArc($Master-2*$r, $Master-2*$r, 2*$r, 2*$r, 0, 90)
$bg.AddArc(0, $Master-2*$r, 2*$r, 2*$r, 90, 90)
$bg.CloseFigure()
$g.FillPath((New-Object System.Drawing.SolidBrush($navy)), $bg)

# Mark coordinate space is 128 units; SVG applies translate(8.56 5.92) scale(0.88).
$k = $Master / 128.0
$g.TranslateTransform(8.56 * $k, 5.92 * $k)
$g.ScaleTransform(0.88 * $k, 0.88 * $k)

function P([double]$x, [double]$y) { New-Object System.Drawing.PointF($x, $y) }

# 1) Blue gradient chevron
$chevron = New-Object System.Drawing.Drawing2D.GraphicsPath
$chevron.AddPolygon([System.Drawing.PointF[]]@((P 28 102), (P 70 16), (P 112 102), (P 94 88), (P 70 39), (P 45 89)))
$grad = New-Object System.Drawing.Drawing2D.LinearGradientBrush(
    (New-Object System.Drawing.PointF(14, 16)),
    (New-Object System.Drawing.PointF(112, 116)),
    [System.Drawing.Color]::White, [System.Drawing.Color]::White)
$blend = New-Object System.Drawing.Drawing2D.ColorBlend(3)
$blend.Colors = @(
    [System.Drawing.ColorTranslator]::FromHtml("#5CC2FF"),
    [System.Drawing.ColorTranslator]::FromHtml("#2563EB"),
    [System.Drawing.ColorTranslator]::FromHtml("#1646B7"))
$blend.Positions = @(0.0, 0.55, 1.0)
$grad.InterpolationColors = $blend
$g.FillPath($grad, $chevron)

# 2) White swoosh (three cubic beziers)
$swoosh = New-Object System.Drawing.Drawing2D.GraphicsPath
$swoosh.AddBezier((P 14 110), (P 39 99),  (P 61 91),  (P 92 89))
$swoosh.AddBezier((P 92 89),  (P 77 96),  (P 66 103), (P 56 115))
$swoosh.AddBezier((P 56 115), (P 39 112), (P 27 113), (P 14 110))
$swoosh.CloseFigure()
$g.FillPath((New-Object System.Drawing.SolidBrush([System.Drawing.ColorTranslator]::FromHtml("#F8FAFC"))), $swoosh)

# 3) Light-blue highlight (90% opacity)
$hi = New-Object System.Drawing.Drawing2D.GraphicsPath
$hi.AddPolygon([System.Drawing.PointF[]]@((P 70 16), (P 97 70), (P 77 58)))
$hiColor = [System.Drawing.ColorTranslator]::FromHtml("#75CEFF")
$g.FillPath((New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(230, $hiColor))), $hi)

# 4) Navy notch
$notch = New-Object System.Drawing.Drawing2D.GraphicsPath
$notch.AddPolygon([System.Drawing.PointF[]]@((P 70 55), (P 84 84), (P 55 84)))
$g.FillPath((New-Object System.Drawing.SolidBrush($navy)), $notch)

$g.Dispose()

function Save-Resized([int]$size, [string]$path) {
    $out = New-Object System.Drawing.Bitmap($size, $size)
    $og = [System.Drawing.Graphics]::FromImage($out)
    $og.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
    $og.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::HighQuality
    $og.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::HighQuality
    $og.CompositingQuality = [System.Drawing.Drawing2D.CompositingQuality]::HighQuality
    $og.DrawImage($bmp, (New-Object System.Drawing.Rectangle(0, 0, $size, $size)))
    $og.Dispose()
    $out.Save($path, [System.Drawing.Imaging.ImageFormat]::Png)
    $out.Dispose()
    "wrote $path ($size x $size)"
}

Save-Resized 32  (Join-Path $OutDir "favicon-32x32.png")
Save-Resized 180 (Join-Path $OutDir "apple-touch-icon.png")
Save-Resized 512 (Join-Path $OutDir "icon-512.png")
$bmp.Dispose()
