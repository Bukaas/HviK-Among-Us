@echo off
setlocal EnableExtensions EnableDelayedExpansion
title NikoCat233 Custom Server Installer

echo ==================================================
echo     NikoCat233 Custom Server Installer
echo ==================================================
echo.

echo Checking for curl...
where curl >nul 2>nul
if errorlevel 1 (
    echo.
    echo curl is not installed. This script cannot continue.
    echo Please follow the manual install guide:
    echo https://au.niko233.top
    echo.
    pause
    exit /b 1
)

echo curl detected successfully.
echo.
echo Downloading region info from au.niko233.top...
echo.

set "TEMP_JSON=%TEMP%\niko233-regioninfo.json"
curl -f -sSL --ssl-no-revoke -A "Mozilla/5.0 (Windows NT 10.0; Win64; x64)" -H "Accept: application/json" -o "%TEMP_JSON%" "https://au.niko233.top/regioninfo.json"

if errorlevel 1 (
    echo.
    echo Download failed. Please check your internet connection and try again.
    echo You can also use the manual install guide:
    echo https://au.niko233.top
    echo.
    if exist "%TEMP_JSON%" del "%TEMP_JSON%" >nul 2>nul
    pause
    exit /b 1
)

if not exist "%TEMP_JSON%" (
    echo.
    echo Download failed: file was not created.
    echo.
    pause
    exit /b 1
)

findstr /c:"CurrentRegionIdx" "%TEMP_JSON%" >nul 2>nul
if errorlevel 1 (
    echo.
    echo Download failed: regioninfo.json does not look valid.
    echo Please use the manual install guide:
    echo https://au.niko233.top
    echo.
    del "%TEMP_JSON%" >nul 2>nul
    pause
    exit /b 1
)

echo Download completed.
echo.

set "STEAM_DIR=%APPDATA%\..\LocalLow\Innersloth\Among Us"
set "WGS_BASE=%LOCALAPPDATA%\Packages\Innersloth.AmongUs_fw5x688tam7rm\SystemAppData\wgs"

set "STEAM_OK=0"
set "MS_OK=0"
set "ANY_INSTALLED=0"

if exist "%STEAM_DIR%\" (
    set "STEAM_OK=1"
    set "ANY_INSTALLED=1"
    echo [Default / Steam] Installation detected.
)

if exist "%WGS_BASE%\" (
    set "MS_OK=1"
    set "ANY_INSTALLED=1"
    echo [Microsoft Store] Installation detected.
)

if "%ANY_INSTALLED%"=="0" (
    echo.
    echo No Among Us installation was found.
    echo Please launch the game at least once, then run this script again.
    echo.
    echo Manual install guide:
    echo https://au.niko233.top
    echo.
    del "%TEMP_JSON%" >nul 2>nul
    pause
    exit /b 1
)

echo.

if "%STEAM_OK%"=="1" (
    echo [Default / Steam] Installing region info...
    copy /y "%TEMP_JSON%" "%STEAM_DIR%\regionInfo.json" >nul
    if !ERRORLEVEL! EQU 0 (
        echo [Default / Steam] OK - Installation completed.
    ) else (
        echo [Default / Steam] ERROR - Failed to copy regionInfo.json.
    )
    echo.
)

if "%MS_OK%"=="1" (
    echo [Microsoft Store] Searching save files...
    set "MS_FOUND=0"

    for /d %%A in ("%WGS_BASE%\*") do (
        for /d %%B in ("%%A\*") do (
            for %%F in ("%%B\*") do (
                findstr /m "CurrentRegionIdx" "%%F" >nul 2>nul
                if !ERRORLEVEL! EQU 0 (
                    echo [Microsoft Store] Found file: %%F
                    copy /y "%TEMP_JSON%" "%%F" >nul
                    if !ERRORLEVEL! EQU 0 (
                        echo [Microsoft Store] OK - Installation completed.
                    ) else (
                        echo [Microsoft Store] ERROR - Failed to copy the file.
                    )
                    set "MS_FOUND=1"
                )
            )
        )
    )

    if "!MS_FOUND!"=="0" (
        echo [Microsoft Store] No matching save file was found.
        echo Launch Among Us once, then try again.
        echo If it still fails, use the manual Microsoft Store method on the website.
    )
    echo.
)

del "%TEMP_JSON%" >nul 2>nul

echo ==================================================
echo Installation completed. Restart Among Us.
echo https://au.niko233.top
echo ==================================================
echo.
pause
