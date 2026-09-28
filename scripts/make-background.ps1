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

# --- Button-Icons (128x128, transparent) ---
$btnDir = Join-Path $res 'Buttons'
New-Item -ItemType Directory -Force $btnDir | Out-Null

function New-Icon([string]$name, [scriptblock]$draw) {
    $bmp, $g = New-Canvas 128 128
    & $draw $g
    $bmp.Save((Join-Path $btnDir "$name.png"), [System.Drawing.Imaging.ImageFormat]::Png)
    $g.Dispose(); $bmp.Dispose()
}
function Brush($r, $gr, $b) { New-Object System.Drawing.SolidBrush ([System.Drawing.Color]::FromArgb(255, $r, $gr, $b)) }
function Outline($w) { New-Object System.Drawing.Pen ([System.Drawing.Color]::FromArgb(255, 20, 20, 20)), $w }
function Pt($x, $y) { New-Object System.Drawing.PointF $x, $y }

# Sheriff: goldener Stern
New-Icon 'Sheriff' {
    param($g)
    $pts = for ($i = 0; $i -lt 10; $i++) {
        $r = if ($i % 2 -eq 0) { 58 } else { 24 }
        $a = [Math]::PI / 5 * $i - [Math]::PI / 2
        Pt (64 + $r * [Math]::Cos($a)) (66 + $r * [Math]::Sin($a))
    }
    $g.FillPolygon((Brush 255 204 0), [System.Drawing.PointF[]]$pts)
    $g.DrawPolygon((Outline 6), [System.Drawing.PointF[]]$pts)
    $g.FillEllipse((Brush 200 150 0), 50, 52, 28, 28)
}

# Transporter: zwei Pfeile in entgegengesetzte Richtungen
New-Icon 'Transporter' {
    param($g)
    $cyan = Brush 0 238 255
    foreach ($arrow in @(@{ Y = 38; Dir = 1 }, @{ Y = 90; Dir = -1 })) {
        $y = $arrow.Y; $d = $arrow.Dir
        $x0 = if ($d -eq 1) { 14 } else { 114 }
        $x1 = if ($d -eq 1) { 84 } else { 44 }
        $tip = if ($d -eq 1) { 118 } else { 10 }
        $pts = [System.Drawing.PointF[]]@((Pt $x0 ($y - 9)), (Pt $x1 ($y - 9)), (Pt $x1 ($y - 24)), (Pt $tip $y), (Pt $x1 ($y + 24)), (Pt $x1 ($y + 9)), (Pt $x0 ($y + 9)))
        $g.FillPolygon($cyan, $pts)
        $g.DrawPolygon((Outline 5), $pts)
    }
}

# Puppeteer: Marionettenkreuz mit Faeden
New-Icon 'Puppeteer' {
    param($g)
    $pink = Brush 180 40 120
    $g.FillRectangle($pink, 14, 14, 100, 16); $g.DrawRectangle((Outline 4), 14, 14, 100, 16)
    $g.FillRectangle($pink, 56, 4, 16, 44); $g.DrawRectangle((Outline 4), 56, 4, 16, 44)
    $string = New-Object System.Drawing.Pen ([System.Drawing.Color]::White), 3
    foreach ($x in 20, 64, 108) { $g.DrawLine($string, $x, 30, $x, 96) }
    $g.FillEllipse((Brush 235 40 50), 34, 70, 60, 50); $g.DrawEllipse((Outline 5), 34, 70, 60, 50)
    $g.FillEllipse((Brush 170 220 240), 58, 80, 30, 18)
}

# Penguin: Pinguin
New-Icon 'Penguin' {
    param($g)
    $g.FillEllipse((Brush 30 30 45), 24, 10, 80, 112); $g.DrawEllipse((Outline 4), 24, 10, 80, 112)
    $g.FillEllipse((Brush 245 245 245), 40, 42, 48, 74)
    $g.FillEllipse((Brush 255 255 255), 44, 26, 16, 18); $g.FillEllipse((Brush 255 255 255), 68, 26, 16, 18)
    $g.FillEllipse((Brush 0 0 0), 50, 31, 8, 10); $g.FillEllipse((Brush 0 0 0), 70, 31, 8, 10)
    $g.FillPolygon((Brush 255 150 0), [System.Drawing.PointF[]]@((Pt 54 46), (Pt 74 46), (Pt 64 60)))
}

Write-Host "Grafiken erstellt in: $res"
