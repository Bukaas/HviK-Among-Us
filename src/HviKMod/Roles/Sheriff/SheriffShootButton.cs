using HviKMod.Localization;
using HviKMod.Roles.Jester;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Keybinds;
using MiraAPI.Networking;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using UnityEngine;

namespace HviKMod.Roles.Sheriff;

public class SheriffShootButton : CustomActionButton<PlayerControl>
{
    private static SheriffOptions Options => OptionGroupSingleton<SheriffOptions>.Instance;

    public override string Name => Loc.T("Schießen", "Shoot");
    public override float Cooldown => Options.Cooldown;
    public override int MaxUses => (int)Options.Shots;
    public override LoadableAsset<Sprite> Sprite => HviKAssets.SheriffButton;
    public override Color TextOutlineColor => HviKColors.Sheriff;
    public override BaseKeybind? Keybind => MiraGlobalKeybinds.PrimaryAbility;
    public override ButtonLocation Location { get; set; } = ButtonLocation.BottomRight;

    public override bool Enabled(RoleBehaviour? role) => role is SheriffRole;

    public override PlayerControl? GetTarget() => PlayerControl.LocalPlayer.GetClosestPlayer(true, Distance);

    public override void SetOutline(bool active)
    {
        Target?.cosmetics.SetOutline(active, new Il2CppSystem.Nullable<Color>(HviKColors.Sheriff));
    }

    protected override void OnClick()
    {
        if (Target == null) return;

        var sheriff = PlayerControl.LocalPlayer;
        var role = Target.Data.Role;
        var isValidTarget = role.IsImpostor || (role is JesterRole && Options.CanShootJester);

        if (isValidTarget)
        {
            sheriff.RpcCustomMurder(Target, MeetingCheck.OutsideMeeting);
            return;
        }

        // Fehlschuss
        if (Options.MisfireKillsTarget)
        {
            sheriff.RpcCustomMurder(Target, MeetingCheck.OutsideMeeting);
        }

        sheriff.RpcCustomMurder(sheriff, MeetingCheck.OutsideMeeting, teleportMurderer: false);
    }
}
