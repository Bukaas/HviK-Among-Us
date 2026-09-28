# HviK Community Mod

Eigene Among-Us-Mod der HviK Community, gebaut auf [MiraAPI](https://github.com/All-Of-Us-Mods/MiraAPI) + [Reactor](https://github.com/NuclearPowered/Reactor) + BepInEx.

**Getestet mit:** Among Us v18s (2026.8.18), Steam

## Projektstruktur

```
src/HviKMod/            Code der Mod
  HviKPlugin.cs         Einstiegspunkt
  Patches/              Aenderungen am Spiel (Harmony-Patches)
  Resources/            Bilder (werden in die DLL eingebettet)
scripts/
  setup-dev.ps1         Test-Kopie von Among Us unter .game/ anlegen
  run-dev.ps1           Mod bauen + Test-Kopie starten
  build-release.ps1     ZIPs fuer Steam und Epic nach dist/ bauen
  make-background.ps1   Menue-Grafiken neu erzeugen
  common.ps1            Versionen von BepInEx/Reactor/MiraAPI
```

## Befehle (PowerShell im Projektordner)

```powershell
powershell -ExecutionPolicy Bypass -File scripts\setup-dev.ps1      # einmalig / nach Among-Us-Update
powershell -ExecutionPolicy Bypass -File scripts\run-dev.ps1        # testen
powershell -ExecutionPolicy Bypass -File scripts\build-release.ps1  # ZIPs zum Verschicken
```

Log der Test-Installation: `.game/BepInEx/LogOutput.log`

## Neue Version rausgeben

1. `<Version>` in `src/HviKMod/HviKMod.csproj` hochzaehlen
2. `build-release.ps1` ausfuehren
3. `dist/HviKMod_v<Version>_Steam.zip` bzw. `_Epic.zip` verschicken

## Nach einem Among-Us-Update

1. Neue Versionen von MiraAPI / Reactor / BepInEx pruefen (Town of Us Mira als Referenz: welche Versionen liefert deren aktuelles Release mit?)
2. Versionen in `scripts/common.ps1` und `src/HviKMod/HviKMod.csproj` anpassen (inkl. `AmongUs.GameLibs.Steam`)
3. `setup-dev.ps1` erneut ausfuehren und testen
