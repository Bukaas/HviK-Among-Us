using System.Collections.Generic;
using HviKMod.Localization;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Usables;
using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Hud;
using MiraAPI.Keybinds;
using MiraAPI.Networking;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using Reactor.Networking.Attributes;
using UnityEngine;

namespace HviKMod.Roles.Bastion;

public class BastionRole : CrewmateRole, ICustomRole
{
    public string RoleName => "Bastion";
    public string RoleDescription => Loc.T("Vermine die Lüftungsschächte", "Rig the vents");
    public string RoleMedDescription => Loc.T(
        "Vermine Vents. Wer in einen verminten Vent steigt, stirbt.",
        "Rig vents with bombs. Whoever enters a rigged vent dies.");
    public string RoleLongDescription => Loc.T(
        "Stell dich an einen Vent und vermine ihn. Steigt jemand hinein, explodiert die Bombe und er stirbt.",
        "Stand next to a vent and rig it. If someone enters it, the bomb explodes and they die.");

    public Color RoleColor => HviKColors.Bastion;
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;
    public CustomRoleConfiguration Configuration => new(this) { MaxRoleCount = 2 };
}

public class BastionOptions : AbstractRoleOptionGroup<BastionRole>
{
    public override string GroupName => "Bastion";

    [ModdedNumberOption("HviK.Option.Cooldown", 5, 60, 2.5f, MiraNumberSuffixes.Seconds)]
    public float Cooldown { get; set; } = 20f;

    [ModdedNumberOption("HviK.Bastion.Bombs", 1, 10)]
    public float Bombs { get; set; } = 3f;
}

public class BastionBombButton : CustomActionButton<Vent>
{
    public override string Name => Loc.T("Verminen", "Rig vent");
    public override float Cooldown => OptionGroupSingleton<BastionOptions>.Instance.Cooldown;
    public override int MaxUses => (int)OptionGroupSingleton<BastionOptions>.Instance.Bombs;
    public override LoadableAsset<Sprite> Sprite => HviKAssets.BastionButton;
    public override Color TextOutlineColor => HviKColors.Bastion;
    public override BaseKeybind? Keybind => MiraGlobalKeybinds.PrimaryAbility;
    public override ButtonLocation Location { get; set; } = ButtonLocation.BottomRight;

    public override bool Enabled(RoleBehaviour? role) => role is BastionRole;

    public override Vent? GetTarget()
    {
        if (!ShipStatus.Instance) return null;
        Vector2 me = PlayerControl.LocalPlayer.GetTruePosition();
        Vent? best = null;
        var bestDistance = 1.2f;
        foreach (var vent in ShipStatus.Instance.AllVents)
        {
            var distance = Vector2.Distance(me, vent.transform.position);
            if (distance < bestDistance && !BastionBombs.IsBombed(vent.Id))
            {
                best = vent;
                bestDistance = distance;
            }
        }

        return best;
    }

    public override void SetOutline(bool active)
    {
        Target?.SetOutline(active, active);
    }

    protected override void OnClick()
    {
        if (Target != null) BastionBombs.RpcBombVent(PlayerControl.LocalPlayer, Target.Id);
    }
}

/// <summary>Verminte Vents (auf allen Clients gleich). Ausgeloest wird auf dem Client, der hineinsteigt.</summary>
public static class BastionBombs
{
    /// <summary>Vent-ID -> Bastion (Spieler-ID)</summary>
    private static readonly Dictionary<int, byte> Bombs = new();

    public static bool IsBombed(int ventId) => Bombs.ContainsKey(ventId);

    [MethodRpc((uint)HviKRpc.BastionBombVent)]
    public static void RpcBombVent(PlayerControl bastion, int ventId)
    {
        if (bastion.Data.Role is BastionRole) Bombs[ventId] = bastion.PlayerId;
    }

    [MethodRpc((uint)HviKRpc.BastionRemoveBomb)]
    public static void RpcRemoveBomb(PlayerControl player, int ventId) => Bombs.Remove(ventId);

    [RegisterEvent]
    public static void OnEnterVent(EnterVentEvent @event)
    {
        var vent = @event.Vent;
        if (vent == null || !Bombs.TryGetValue(vent.Id, out var bastionId)) return;

        var player = @event.Player;
        var bastion = RoleHelpers.PlayerById(bastionId) ?? player;
        @event.Cancel(); // nicht in den Vent - Bombe explodiert davor
        RpcRemoveBomb(player, vent.Id);
        bastion.RpcCustomMurder(player, MeetingCheck.OutsideMeeting, resetKillTimer: false, teleportMurderer: false);
    }

    [HarmonyLib.HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.CoBegin))]
    public static class ResetPatch
    {
        public static void Prefix() => Bombs.Clear();
    }
}
