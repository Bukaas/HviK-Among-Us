@echo off
rem Doppelklick: Mod bauen und Test-Among-Us starten (Steam muss laufen).
rem Beim allerersten Mal wird automatisch die Test-Installation angelegt.
cd /d "%~dp0"
if not exist ".game\BepInEx" (
    powershell -NoProfile -ExecutionPolicy Bypass -File "scripts\setup-dev.ps1" || goto :error
)
powershell -NoProfile -ExecutionPolicy Bypass -File "scripts\run-dev.ps1" || goto :error
ping -n 5 127.0.0.1 >nul
exit /b 0

:error
echo.
echo Es ist ein Fehler aufgetreten - siehe oben.
pause
