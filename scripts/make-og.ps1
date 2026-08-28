# Renders the 1200x630 Open Graph / Twitter card image.
Add-Type -AssemblyName System.Drawing

$OutPath = $args[0]
$W = 1200; $H = 630

$bmp = New-Object System.Drawing.Bitmap($W, $H)
$g = [System.Drawing.Graphics]::FromImage($bmp)
$g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
$g.TextRenderingHint = [System.Drawing.Text.TextRenderingHint]::AntiAliasGridFit
$g.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::HighQuality

$navy = [System.Drawing.ColorTranslator]::FromHtml("#0D1B2A")
$g.Clear($navy)

# Subtle blue radial-ish glow on the right, approximated with a path gradient
$glow = New-Object System.Drawing.Drawing2D.GraphicsPath
$glow.AddEllipse(700, 60, 620, 520)
$pgb = New-Object System.Drawing.Drawing2D.PathGradientBrush($glow)
$pgb.CenterColor = [System.Drawing.Color]::FromArgb(70, 37, 99, 235)
$pgb.SurroundColors = @([System.Drawing.Color]::FromArgb(0, 13, 27, 42))
$g.FillPath($pgb, $glow)

function P([double]$x, [double]$y) { New-Object System.Drawing.PointF($x, $y) }

# Logo mark, scaled into the left margin
$state = $g.Save()
$g.TranslateTransform(96, 150)
$g.ScaleTransform(1.15, 1.15)

$chevron = New-Object System.Drawing.Drawing2D.GraphicsPath
$chevron.AddPolygon([System.Drawing.PointF[]]@((P 28 102), (P 70 16), (P 112 102), (P 94 88), (P 70 39), (P 45 89)))
$grad = New-Object System.Drawing.Drawing2D.LinearGradientBrush(
    (New-Object System.Drawing.PointF(14, 16)), (New-Object System.Drawing.PointF(112, 116)),
    [System.Drawing.Color]::White, [System.Drawing.Color]::White)
$blend = New-Object System.Drawing.Drawing2D.ColorBlend(3)
$blend.Colors = @(
    [System.Drawing.ColorTranslator]::FromHtml("#5CC2FF"),
    [System.Drawing.ColorTranslator]::FromHtml("#2563EB"),
    [System.Drawing.ColorTranslator]::FromHtml("#1646B7"))
$blend.Positions = @(0.0, 0.55, 1.0)
$grad.InterpolationColors = $blend
$g.FillPath($grad, $chevron)

$swoosh = New-Object System.Drawing.Drawing2D.GraphicsPath
$swoosh.AddBezier((P 14 110), (P 39 99),  (P 61 91),  (P 92 89))
$swoosh.AddBezier((P 92 89),  (P 77 96),  (P 66 103), (P 56 115))
$swoosh.AddBezier((P 56 115), (P 39 112), (P 27 113), (P 14 110))
$swoosh.CloseFigure()
$g.FillPath((New-Object System.Drawing.SolidBrush([System.Drawing.ColorTranslator]::FromHtml("#F8FAFC"))), $swoosh)

$hi = New-Object System.Drawing.Drawing2D.GraphicsPath
$hi.AddPolygon([System.Drawing.PointF[]]@((P 70 16), (P 97 70), (P 77 58)))
$g.FillPath((New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(230, [System.Drawing.ColorTranslator]::FromHtml("#75CEFF")))), $hi)

$notch = New-Object System.Drawing.Drawing2D.GraphicsPath
$notch.AddPolygon([System.Drawing.PointF[]]@((P 70 55), (P 84 84), (P 55 84)))
$g.FillPath((New-Object System.Drawing.SolidBrush($navy)), $notch)
$g.Restore($state)

$white = New-Object System.Drawing.SolidBrush([System.Drawing.ColorTranslator]::FromHtml("#F8FAFC"))
$blue  = New-Object System.Drawing.SolidBrush([System.Drawing.ColorTranslator]::FromHtml("#3A9EFF"))
$light = New-Object System.Drawing.SolidBrush([System.Drawing.ColorTranslator]::FromHtml("#CBD5E1"))

function Font([string]$family, [single]$size, [int]$style) {
    try { New-Object System.Drawing.Font($family, $size, [System.Drawing.FontStyle]$style, [System.Drawing.GraphicsUnit]::Pixel) }
    catch { New-Object System.Drawing.Font("Segoe UI", $size, [System.Drawing.FontStyle]$style, [System.Drawing.GraphicsUnit]::Pixel) }
}

# Wordmark
$g.DrawString("LAUNCH ASSIST", (Font "Poppins" 40 ([System.Drawing.FontStyle]::Bold)), $white, 260, 158)
$g.DrawString("S T U D I O",   (Font "Segoe UI" 22 ([System.Drawing.FontStyle]::Regular)), $blue,  263, 210)

# Kicker + headline
$g.DrawString("BUILD. LAUNCH. GROW.", (Font "Segoe UI" 22 ([System.Drawing.FontStyle]::Bold)), $blue, 96, 300)
$hFont = Font "Poppins" 58 ([System.Drawing.FontStyle]::Bold)
$g.DrawString("Websites & Custom Software", $hFont, $white, 92, 340)
$g.DrawString("That Drive Real Results.",   $hFont, $white, 92, 406)

# Supporting copy
$g.DrawString("Professional design. Clean code. Microsoft technologies.",
    (Font "Segoe UI" 25 ([System.Drawing.FontStyle]::Regular)), $light, 96, 495)
$g.DrawString(".NET  •  C#  •  ASP.NET Core  •  Blazor  •  SQL Server",
    (Font "Segoe UI" 22 ([System.Drawing.FontStyle]::Bold)), $blue, 96, 540)

$g.Dispose()
$bmp.Save($OutPath, [System.Drawing.Imaging.ImageFormat]::Png)
$bmp.Dispose()
"wrote $OutPath"
