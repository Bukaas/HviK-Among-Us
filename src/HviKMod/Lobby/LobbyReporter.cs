using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using HviKMod.Localization;
using InnerNet;
using Reactor.Utilities;
using UnityEngine;

namespace HviKMod.Lobby;

/// <summary>
/// Meldet die Lobby des Hosts an hvik.org (Code, Status, Spieler mit Friend Codes).
/// Der Discord-Bot erstellt daraus den Kanal #among-us-code, die Website zeigt einen Live-Ticker.
/// Siehe docs/lobby-api.md. Nur der Host sendet, nur in Online-Spielen, abschaltbar im Tab "HviK".
/// </summary>
public static class LobbyReporter
{
    private const string Endpoint = "https://hvik.org/api/among-us/lobby";
    private const float HeartbeatSeconds = 30f;

    private static readonly HttpClient Http = new() { Timeout = TimeSpan.FromSeconds(10) };
    internal static readonly string? ApiKey = LoadApiKey();
    private static readonly string ModVersion = HviKPlugin.Version.Split('+')[0];

    private static string? _lastCode;
    private static string? _lastFingerprint;
    private static float _lastSentAt;
    private static bool _warnedMissingKey;

    public static void Start() => Coroutines.Start(Loop());

    private static IEnumerator Loop()
    {
        while (true)
        {
            try
            {
                FriendCodeSync.Tick();
                Tick();
            }
            catch (Exception e)
            {
                HviKPlugin.Instance.Log.LogWarning($"LobbyReporter: {e.Message}");
            }

            yield return new WaitForSecondsRealtime(1f);
        }
    }

    private static void Tick()
    {
        var snapshot = HviKLocalSettings.SendLobbyCode ? TakeSnapshot() : null;

        // Lobby verlassen (oder Senden ausgeschaltet) -> einmal "closed" schicken
        if (snapshot == null)
        {
            if (_lastCode != null)
            {
                Send(ClosedReport(_lastCode));
                _lastCode = null;
                _lastFingerprint = null;
            }

            return;
        }

        if (_lastCode != null && _lastCode != snapshot.code)
        {
            Send(ClosedReport(_lastCode));
        }

        var fingerprint = snapshot.Fingerprint();
        var changed = fingerprint != _lastFingerprint;
        var heartbeatDue = Time.realtimeSinceStartup - _lastSentAt >= HeartbeatSeconds;
        if (!changed && !heartbeatDue) return;

        _lastCode = snapshot.code;
        _lastFingerprint = fingerprint;
        _lastSentAt = Time.realtimeSinceStartup;
        Send(snapshot);
    }

    /// <summary>
    /// "Lobby zu" - mit dem eigenen Friend Code, damit der Server erkennt, wenn nur der Host
    /// gewechselt hat (dann meldet sich die Mod des neuen Hosts und die Lobby laeuft weiter).
    /// </summary>
    private static LobbyReport ClosedReport(string code) => new()
    {
        code = code,
        state = "closed",
        hostFriendCode = EOSManager.Instance ? EOSManager.Instance.FriendCode : null,
        modVersion = ModVersion,
    };

    private static LobbyReport? TakeSnapshot()
    {
        var client = AmongUsClient.Instance;
        if (!client || !client.AmHost || client.NetworkMode != NetworkModes.OnlineGame) return null;
        if (client.GameState is not (InnerNetClient.GameStates.Joined or InnerNetClient.GameStates.Started)) return null;
        if (!GameData.Instance || !PlayerControl.LocalPlayer) return null;

        var players = GameData.Instance.AllPlayers.ToArray()
            .Where(p => p && !p.Disconnected && !string.IsNullOrEmpty(p.PlayerName))
            .Select(p => new LobbyPlayer { name = p.PlayerName, friendCode = FriendCodeSync.For(p) })
            .ToArray();

        return new LobbyReport
        {
            code = GameCode.IntToGameName(client.GameId),
            state = client.GameState == InnerNetClient.GameStates.Started ? "ingame" : "lobby",
            region = ServerManager.Instance ? ServerManager.Instance.CurrentRegion?.Name : null,
            host = PlayerControl.LocalPlayer.Data.PlayerName,
            hostFriendCode = FriendCodeSync.For(PlayerControl.LocalPlayer.Data),
            players = players,
            maxPlayers = GameOptionsManager.Instance.CurrentGameOptions.MaxPlayers,
            modVersion = ModVersion,
        };
    }

    private static void Send(LobbyReport report)
    {
        if (ApiKey == null)
        {
            if (!_warnedMissingKey) HviKPlugin.Instance.Log.LogWarning("LobbyReporter: kein API-Schluessel eingebaut - Lobby-Codes werden nicht gesendet.");
            _warnedMissingKey = true;
            return;
        }

        var json = JsonSerializer.Serialize(report);
        // Netzwerk im Hintergrund, damit das Spiel nie haengt, wenn hvik.org langsam/offline ist.
        Task.Run(async () =>
        {
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Post, Endpoint);
                request.Headers.Add("X-HviK-Key", ApiKey);
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");
                using var response = await Http.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    HviKPlugin.Instance.Log.LogWarning($"LobbyReporter: hvik.org antwortet {(int)response.StatusCode}");
                }
            }
            catch (Exception e)
            {
                HviKPlugin.Instance.Log.LogWarning($"LobbyReporter: senden fehlgeschlagen ({e.GetType().Name})");
            }
        });
    }

    /// <summary>Schluessel wird beim Bauen aus src/HviKMod/Secrets/ApiKey.txt eingebettet (nicht im Git).</summary>
    private static string? LoadApiKey()
    {
        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("HviKMod.Secrets.ApiKey.txt");
        if (stream == null) return null;
        using var reader = new StreamReader(stream);
        var key = reader.ReadToEnd().Trim();
        return key.Length > 0 ? key : null;
    }

    // Feldnamen = JSON-Namen (siehe docs/lobby-api.md)
    private sealed class LobbyReport
    {
        public string code { get; set; } = "";
        public string state { get; set; } = "lobby";
        public string? region { get; set; }
        public string? host { get; set; }
        public string? hostFriendCode { get; set; }
        public LobbyPlayer[] players { get; set; } = [];
        public int? maxPlayers { get; set; }
        public string modVersion { get; set; } = "";

        public string Fingerprint() =>
            $"{code}|{state}|{maxPlayers}|{string.Join(",", players.Select(p => p.name + "/" + p.friendCode))}";
    }

    private sealed class LobbyPlayer
    {
        public string name { get; set; } = "";
        public string? friendCode { get; set; }
    }
}
