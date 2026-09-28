# Lobby-API: Mod → hvik.org

Schnittstelle zwischen der Among-Us-Mod (sendet) und Homepage/Discord-Bot (empfängt).
Die Mod-Seite wird in diesem Repo gebaut, die Server-Seite im Bot-Projekt auf der VM.

## Anfrage

```
POST https://hvik.org/api/among-us/lobby
Header:  X-HviK-Key: <AMONG_US_API_KEY aus der .env des Bots>
Content-Type: application/json
```

```json
{
  "code": "ABCDEF",
  "state": "lobby",
  "region": "Niko EU",
  "host": "Bukaas",
  "players": 7,
  "maxPlayers": 15,
  "modVersion": "0.3.0"
}
```

| Feld | Bedeutung |
|---|---|
| `code` | Lobby-Code, nur Großbuchstaben (4 oder 6 Zeichen) |
| `state` | `lobby` (Lobby offen), `ingame` (Runde läuft), `closed` (Lobby geschlossen) |
| `region` | Name des Servers/der Region |
| `host` | Spielername des Hosts |
| `players` / `maxPlayers` | aktuelle / maximale Spielerzahl |
| `modVersion` | Version der HviK-Mod |

**Wann sendet die Mod?** Nur der Host, nur in Online-Spielen:
- sobald eine Lobby erstellt wird,
- bei jeder Änderung von `state` oder `players`,
- sonst alle 30 Sekunden als Lebenszeichen,
- `closed`, wenn der Host die Lobby verlässt.

## Antworten

- `200 {"ok": true}`: angenommen
- `401`: falscher oder fehlender Schlüssel
- `422`: ungültige Daten

## Verhalten auf dem Server

1. **Speichern:** Die Lobby wird anhand von `code` gespeichert bzw. aktualisiert.
2. **Timeout:** Kommt 2 Minuten lang kein Lebenszeichen, gilt die Lobby als geschlossen, z.B. weil das Spiel des Hosts abgestürzt ist.
3. **Discord:** Der Bot postet **eine** Nachricht in den Text-Channel des laufenden Among-Us-Events und **bearbeitet** sie danach nur noch (Lobby offen → Runde läuft → beendet).
   - Als laufendes Event gilt: `status = upcoming`, `game` enthält „among us“, `text_channel_id` ist gesetzt und nicht archiviert, Start liegt maximal 12 Stunden entfernt. Bei mehreren Treffern wird das Event genommen, dessen Start am nächsten liegt.
   - Gibt es kein passendes Event, wird nichts gepostet.
4. **Homepage:** Solange eine Lobby offen ist, zeigt die Homepage für eingeloggte Nutzer einen Live-Ticker:
   `🔴 Aktuelles Event läuft – Spiel: Among Us – Code: ABCDEF`
   - Der Ticker bekommt einen Kopieren-Button für den Code.
   - Er aktualisiert sich über `GET /api/among-us/live`.
