using System.Collections.Generic;
using InnerNet;
using Reactor.Networking.Attributes;
using UnityEngine;

namespace HviKMod.Lobby;

/// <summary>
/// Modded-Server geben die Friend Codes der Spieler nicht weiter (NetworkedPlayerInfo.FriendCode ist leer).
/// Deshalb schickt jeder Spieler mit HviK-Mod seinen eigenen Friend Code regelmaessig an alle -
/// so kennt der Host alle Codes und kann sie an hvik.org melden (Discord-Zuordnung).
/// </summary>
public static class FriendCodeSync
{
    private const float AnnounceEverySeconds = 20f;

    private static readonly Dictionary<byte, string> Codes = new();
    private static int _gameId = -1;
    private static float _lastAnnounce = -AnnounceEverySeconds;

    /// <summary>Friend Code eines Spielers: vom Server, sonst vom Spieler selbst gemeldet.</summary>
    public static string? For(NetworkedPlayerInfo player)
    {
        if (!string.IsNullOrEmpty(player.FriendCode)) return player.FriendCode;
        return Codes.TryGetValue(player.PlayerId, out var code) ? code : null;
    }

    /// <summary>Wird jede Sekunde vom LobbyReporter aufgerufen (auf allen Clients).</summary>
    public static void Tick()
    {
        var client = AmongUsClient.Instance;
        if (!client || client.GameState is not (InnerNetClient.GameStates.Joined or InnerNetClient.GameStates.Started))
        {
            _gameId = -1;
            return;
        }

        if (client.GameId != _gameId)
        {
            _gameId = client.GameId;
            Codes.Clear();
            _lastAnnounce = -AnnounceEverySeconds;
        }

        var ownCode = EOSManager.Instance ? EOSManager.Instance.FriendCode : null;
        var me = PlayerControl.LocalPlayer;
        if (string.IsNullOrEmpty(ownCode) || !me) return;

        // Regelmaessig wiederholen: so bekommen auch spaeter beigetretene Spieler/ein neuer Host alles mit.
        if (Time.realtimeSinceStartup - _lastAnnounce < AnnounceEverySeconds) return;
        _lastAnnounce = Time.realtimeSinceStartup;
        RpcAnnounce(me, ownCode);
    }

    [MethodRpc((uint)HviKRpc.AnnounceFriendCode)]
    public static void RpcAnnounce(PlayerControl player, string friendCode)
    {
        if (player && friendCode.Length <= 40) Codes[player.PlayerId] = friendCode;
    }
}
