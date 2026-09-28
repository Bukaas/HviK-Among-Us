# Erzeugt die Hauptmenue-Grafiken in src/HviKMod/Resources/:
#   MenuBackground.png - Sternenhimmel-Hintergrund
#   MenuLogo.png       - "HviK Community"-Schriftzug (ersetzt das AMONG US-Logo)
# Beide koennen auch durch eigene PNGs mit gleichem Namen ersetzt werden.
Add-Type -AssemblyName System.Drawing

$root = Split-Path $PSScriptRoot -Parent
$res = Join-Path $root "src\HviKMod\Resources"
New-Item -ItemType Directory -Force $res | Out-Null

function New-Canvas($w, $h) {
    $bmp = New-Object System.Drawing.Bitmap $w, $h
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    $g.SmoothingMode = 'AntiAlias'
    $g.TextRenderingHint = 'AntiAliasGridFit'
    return $bmp, $g
}

# --- Hintergrund ---
$w = 1920; $h = 1080
$bmp, $g = New-Canvas $w $h
$rect = New-Object System.Drawing.Rectangle 0, 0, $w, $h
$grad = New-Object System.Drawing.Drawing2D.LinearGradientBrush $rect, ([System.Drawing.Color]::FromArgb(255, 18, 8, 48)), ([System.Drawing.Color]::FromArgb(255, 4, 20, 40)), 90
$g.FillRectangle($grad, $rect)

# Sterne (fester Seed, damit das Bild reproduzierbar ist)
$rng = New-Object System.Random 42
for ($i = 0; $i -lt 450; $i++) {
    $s = $rng.Next(1, 4)
    $brush = New-Object System.Drawing.SolidBrush ([System.Drawing.Color]::FromArgb($rng.Next(90, 255), 255, 255, 255))
    $g.FillEllipse($brush, $rng.Next(0, $w), $rng.Next(0, $h), $s, $s)
    $brush.Dispose()
}
$bmp.Save((Join-Path $res 'MenuBackground.png'), [System.Drawing.Imaging.ImageFormat]::Png)
$g.Dispose(); $bmp.Dispose()

# --- Logo (transparent, gleiches Seitenverhaeltnis wie das Original-Logo ~3.8:1) ---
$w = 1536; $h = 400
$bmp, $g = New-Canvas $w $h
$fmt = New-Object System.Drawing.StringFormat
$fmt.Alignment = 'Center'; $fmt.LineAlignment = 'Center'

$titleFont = New-Object System.Drawing.Font "Segoe UI Black", 230, ([System.Drawing.FontStyle]::Bold), ([System.Drawing.GraphicsUnit]::Pixel)
$subFont = New-Object System.Drawing.Font "Segoe UI", 96, ([System.Drawing.FontStyle]::Bold), ([System.Drawing.GraphicsUnit]::Pixel)
$shadow = New-Object System.Drawing.SolidBrush ([System.Drawing.Color]::FromArgb(200, 0, 0, 0))
$red = New-Object System.Drawing.SolidBrush ([System.Drawing.Color]::FromArgb(255, 235, 40, 50))
$white = New-Object System.Drawing.SolidBrush ([System.Drawing.Color]::White)

$g.DrawString("HviK", $titleFont, $shadow, (New-Object System.Drawing.RectangleF 8, 8, $w, 280), $fmt)
$g.DrawString("HviK", $titleFont, $red, (New-Object System.Drawing.RectangleF 0, 0, $w, 280), $fmt)
$g.DrawString("COMMUNITY", $subFont, $shadow, (New-Object System.Drawing.RectangleF 5, 275, $w, 120), $fmt)
$g.DrawString("COMMUNITY", $subFont, $white, (New-Object System.Drawing.RectangleF 0, 270, $w, 120), $fmt)

$bmp.Save((Join-Path $res 'MenuLogo.png'), [System.Drawing.Imaging.ImageFormat]::Png)
$g.Dispose(); $bmp.Dispose()

Write-Host "Grafiken erstellt in: $res"
