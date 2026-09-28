using System.Collections.Generic;
using HarmonyLib;
using Reactor.Networking.Attributes;
using UnityEngine;

namespace HviKMod.Roles.Penguin;

/// <summary>
/// Wer wird gerade von welchem Penguin gezogen? Wird per RPC auf allen Clients gleich gehalten.
/// Bewegt wird das Opfer von seinem eigenen Client (nur der darf die eigene Figur bewegen).
/// </summary>
public static class PenguinAbility
{
    /// <summary>Opfer-ID -> Penguin-ID</summary>
    private static readonly Dictionary<byte, byte> Grabbed = new();

    private static bool _localFrozen;

    public static bool IsGrabbed(byte playerId) => Grabbed.ContainsKey(playerId);

    [MethodRpc((uint)HviKRpc.PenguinGrab)]
    public static void RpcGrab(PlayerControl penguin, PlayerControl victim)
    {
        if (penguin.Data.Role is not PenguinRole || victim.Data.IsDead) return;
        Grabbed[victim.PlayerId] = penguin.PlayerId;
    }

    [MethodRpc((uint)HviKRpc.PenguinRelease)]
    public static void RpcRelease(PlayerControl penguin, PlayerControl victim)
    {
        Release(victim.PlayerId);
    }

    private static void Release(byte victimId)
    {
        Grabbed.Remove(victimId);
        if (_localFrozen && PlayerControl.LocalPlayer && PlayerControl.LocalPlayer.PlayerId == victimId)
        {
            _localFrozen = false;
            PlayerControl.LocalPlayer.moveable = true;
        }
    }

    public static void ReleaseAll()
    {
        foreach (var id in new List<byte>(Grabbed.Keys)) Release(id);
        Grabbed.Clear();
    }

    /// <summary>Laeuft jeden Frame beim gezogenen Spieler: an den Penguin heften.</summary>
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.FixedUpdate))]
    public static class FollowPatch
    {
        public static void Postfix(PlayerControl __instance)
        {
            if (!__instance.AmOwner || !Grabbed.TryGetValue(__instance.PlayerId, out var penguinId)) return;

            var penguin = RoleHelpers.PlayerById(penguinId);
            if (penguin == null || penguin.Data.IsDead || penguin.Data.Disconnected || __instance.Data.IsDead)
            {
                Release(__instance.PlayerId);
                return;
            }

            _localFrozen = true;
            __instance.moveable = false;
            __instance.MyPhysics.body.velocity = Vector2.zero;
            __instance.NetTransform.SnapTo(penguin.transform.position);
        }
    }

    /// <summary>Meetings und Spielstart geben alle frei.</summary>
    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Start))]
    public static class MeetingPatch
    {
        public static void Prefix() => ReleaseAll();
    }

    [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.CoBegin))]
    public static class ResetPatch
    {
        public static void Prefix() => ReleaseAll();
    }
}
