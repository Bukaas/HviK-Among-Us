# Baut die ZIPs und veroeffentlicht sie als GitHub-Release (Download-Seite fuer die Mitspieler).
# Voraussetzung: einmalig "gh auth login" ausgefuehrt, Code ist committet und gepusht.
. "$PSScriptRoot\common.ps1"

Set-Location $Root
$version = Get-ModVersion
$tag = "v$version"

if (git -C $Root status --porcelain) { throw 'Es gibt noch nicht committete Aenderungen - erst committen.' }
if (git -C $Root tag --list $tag) { throw "Release $tag existiert schon - erst <Version> in HviKMod.csproj hochzaehlen." }

& "$PSScriptRoot\build-release.ps1"

$notes = @"
## Installation
- **Steam:** ``HviKMod_${tag}_Steam.zip`` herunterladen
- **Epic Games:** ``HviKMod_${tag}_Epic.zip`` herunterladen

Den Inhalt der ZIP in den Among-Us-Ordner entpacken (``winhttp.dll`` muss neben ``Among Us.exe`` liegen) und das Spiel starten.
Der erste Start dauert 1-3 Minuten. Alle Mitspieler brauchen dieselbe Version.

**Deinstallieren:** ``winhttp.dll`` im Among-Us-Ordner loeschen.
"@

git -C $Root push
& $Gh release create $tag (Join-Path $Dist "HviKMod_${tag}_Steam.zip") (Join-Path $Dist "HviKMod_${tag}_Epic.zip") `
    --title "HviK Mod $tag" --notes $notes
if ($LASTEXITCODE -ne 0) { throw 'Release konnte nicht erstellt werden.' }
Write-Host "Release $tag veroeffentlicht."
