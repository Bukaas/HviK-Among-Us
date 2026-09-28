# Gemeinsame Pfade und Hilfsfunktionen fuer alle Skripte.
$ErrorActionPreference = 'Stop'

$Root      = Split-Path $PSScriptRoot -Parent
$Vendor    = Join-Path $Root 'vendor'
$DevGame   = Join-Path $Root '.game'
$Dist      = Join-Path $Root 'dist'
$Project   = Join-Path $Root 'src\HviKMod\HviKMod.csproj'
$SteamGame = 'C:\Program Files (x86)\Steam\steamapps\common\Among Us'

# Mod-Abhaengigkeiten - bei einem Among-Us-Update hier die Versionen anheben.
# Steam/Itch ist 32-Bit (x86), Epic/MS Store ist 64-Bit (x64) -> unterschiedliches BepInEx.
$BepInExUrls = @{
    x86 = 'https://builds.bepinex.dev/projects/bepinex_be/752/BepInEx-Unity.IL2CPP-win-x86-6.0.0-be.752%2Bdd0655f.zip'
    x64 = 'https://builds.bepinex.dev/projects/bepinex_be/752/BepInEx-Unity.IL2CPP-win-x64-6.0.0-be.752%2Bdd0655f.zip'
}
$PluginUrls = @(
    'https://nuget.reactor.gg/v3/package/reactor/2.5.1-ci.384/reactor.2.5.1-ci.384.nupkg'
    'https://api.nuget.org/v3-flatcontainer/allofus.miraapi/0.5.0/allofus.miraapi.0.5.0.nupkg'
)

$Dotnet = 'C:\Program Files\dotnet\dotnet.exe'
if (-not (Test-Path $Dotnet)) { $Dotnet = 'dotnet' }

function Get-ModVersion {
    ([xml](Get-Content $Project)).Project.PropertyGroup.Version | Where-Object { $_ } | Select-Object -First 1
}

# Laedt eine Datei nach vendor/ (Dateiname = letzter URL-Teil, also pro Version eindeutig).
function Get-VendorFile([string]$Url) {
    New-Item -ItemType Directory -Force $Vendor | Out-Null
    $file = Join-Path $Vendor ([uri]::UnescapeDataString(($Url -split '/')[-1]))
    if (-not (Test-Path $file)) {
        Write-Host "Lade $(Split-Path $file -Leaf)..."
        Invoke-WebRequest $Url -OutFile "$file.part" -UseBasicParsing
        Move-Item "$file.part" $file
    }
    $file
}

# Installiert BepInEx + Reactor + MiraAPI in einen Among-Us-Ordner (ohne unsere Mod).
function Install-Loader([string]$GameDir, [string]$Arch = 'x86') {
    Add-Type -AssemblyName System.IO.Compression.FileSystem
    Expand-Archive (Get-VendorFile $BepInExUrls[$Arch]) -DestinationPath $GameDir -Force
    $plugins = Join-Path $GameDir 'BepInEx\plugins'
    New-Item -ItemType Directory -Force $plugins | Out-Null

    # Aus den NuGet-Paketen nur die Plugin-DLL entnehmen.
    foreach ($url in $PluginUrls) {
        $zip = [System.IO.Compression.ZipFile]::OpenRead((Get-VendorFile $url))
        try {
            $entry = $zip.Entries | Where-Object { $_.FullName -like 'lib/net6.0/*.dll' } | Select-Object -First 1
            [System.IO.Compression.ZipFileExtensions]::ExtractToFile($entry, (Join-Path $plugins $entry.Name), $true)
        } finally { $zip.Dispose() }
    }
}

function Build-Mod {
    & $Dotnet build $Project -c Release --nologo -v quiet | Out-Host
    if ($LASTEXITCODE -ne 0) { throw 'Build fehlgeschlagen.' }
    Join-Path $Root 'src\HviKMod\bin\Release\net6.0\HviKMod.dll'
}
