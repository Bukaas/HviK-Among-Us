# Richtet eine Test-Kopie von Among Us unter .game/ ein (die Steam-Installation bleibt unveraendert).
# Nach einem Among-Us-Update einfach erneut ausfuehren.
. "$PSScriptRoot\common.ps1"

if (-not (Test-Path (Join-Path $SteamGame 'Among Us.exe'))) {
    throw "Among Us nicht gefunden unter: $SteamGame"
}

Write-Host "Kopiere Among Us nach $DevGame ..."
if (Test-Path $DevGame) { Remove-Item $DevGame -Recurse -Force }
Copy-Item $SteamGame $DevGame -Recurse

# Verhindert, dass Steam die Original-Installation statt der Kopie startet.
Set-Content (Join-Path $DevGame 'steam_appid.txt') '945360' -Encoding ascii -NoNewline

Install-Loader $DevGame 'x86'
Copy-Item (Build-Mod) (Join-Path $DevGame 'BepInEx\plugins') -Force

Write-Host 'Fertig. Starten mit: scripts\run-dev.ps1'
