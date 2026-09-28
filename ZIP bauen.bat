@echo off
rem Doppelklick: ZIPs fuer Steam und Epic nach dist\ bauen (ohne sie zu veroeffentlichen).
cd /d "%~dp0"
powershell -NoProfile -ExecutionPolicy Bypass -File "scripts\build-release.ps1"
echo.
echo Fertig. Die ZIPs liegen im Ordner "dist".
pause
