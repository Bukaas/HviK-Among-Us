# Baut die fertigen ZIPs zum Verschicken:
#   dist/HviKMod_v<Version>_Steam.zip  (Steam / Itch, 32-Bit)
#   dist/HviKMod_v<Version>_Epic.zip   (Epic Games / MS Store, 64-Bit)
. "$PSScriptRoot\common.ps1"

$version = Get-ModVersion
$modDll = Build-Mod
New-Item -ItemType Directory -Force $Dist | Out-Null

$readme = @"
HviK Community Mod v$version ({0})
=============================

INSTALLATION
1. Among Us-Ordner oeffnen:
   Steam: Rechtsklick auf Among Us > Verwalten > Lokale Dateien durchsuchen
   Epic:  Bibliothek > ... bei Among Us > Verwalten > Ordner-Symbol
2. Den kompletten Inhalt dieser ZIP in diesen Ordner entpacken
   (winhttp.dll muss direkt neben "Among Us.exe" liegen).
3. Among Us starten. Der erste Start dauert 1-3 Minuten - das ist normal.
   Im Hauptmenue steht dann "HviK Community" statt "AMONG US".

WICHTIG: Alle Mitspieler brauchen dieselbe Mod-Version.

DEINSTALLIEREN
winhttp.dll im Among-Us-Ordner loeschen (oder umbenennen).

UPDATE
Neue ZIP einfach drueber entpacken und Dateien ersetzen.
"@

foreach ($target in @(@{ Name = 'Steam'; Arch = 'x86' }, @{ Name = 'Epic'; Arch = 'x64' })) {
    $staging = Join-Path $Dist "staging-$($target.Name)"
    if (Test-Path $staging) { Remove-Item $staging -Recurse -Force }
    New-Item -ItemType Directory -Force $staging | Out-Null

    Install-Loader $staging $target.Arch
    Copy-Item $modDll (Join-Path $staging 'BepInEx\plugins') -Force
    Remove-Item (Join-Path $staging 'changelog.txt') -ErrorAction SilentlyContinue
    ($readme -f $target.Name) | Set-Content (Join-Path $staging 'ANLEITUNG.txt') -Encoding utf8

    $zip = Join-Path $Dist "HviKMod_v${version}_$($target.Name).zip"
    if (Test-Path $zip) { Remove-Item $zip }
    # ZIP von Hand bauen: Compress-Archive/CreateFromDirectory schreiben in PS 5.1
    # "\" statt "/" in die Pfade, was manche Entpack-Programme falsch verarbeiten.
    $archive = [System.IO.Compression.ZipFile]::Open($zip, 'Create')
    try {
        foreach ($file in Get-ChildItem $staging -Recurse -File -Force) {
            $name = $file.FullName.Substring($staging.Length + 1).Replace('\', '/')
            [System.IO.Compression.ZipFileExtensions]::CreateEntryFromFile($archive, $file.FullName, $name, 'Optimal') | Out-Null
        }
    } finally { $archive.Dispose() }
    Remove-Item $staging -Recurse -Force
    Write-Host "ZIP erstellt: $zip"
}
