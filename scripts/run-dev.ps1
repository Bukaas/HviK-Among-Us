# Baut die Mod und startet die Test-Installation. Steam muss laufen.
. "$PSScriptRoot\common.ps1"

if (-not (Test-Path (Join-Path $DevGame 'BepInEx'))) {
    throw 'Test-Installation fehlt - zuerst scripts\setup-dev.ps1 ausfuehren.'
}

Copy-Item (Build-Mod) (Join-Path $DevGame 'BepInEx\plugins') -Force
Start-Process (Join-Path $DevGame 'Among Us.exe') -WorkingDirectory $DevGame
Write-Host "Spiel gestartet. Log: $DevGame\BepInEx\LogOutput.log"
