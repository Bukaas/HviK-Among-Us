using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Keybinds;
using MiraAPI.Networking;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using UnityEngine;

namespace HviKMod.Roles.Penguin;

public class PenguinGrabButton : CustomActionButton<PlayerControl>
{
    private static PenguinOptions Options => OptionGroupSingleton<PenguinOptions>.Instance;

    private PlayerControl? _victim;

    public override string Name => "Packen";
    public override float Cooldown => Options.Cooldown;
    public override float EffectDuration => Options.DragDuration;
    public override LoadableAsset<Sprite> Sprite => HviKAssets.PenguinButton;
    public override Color TextOutlineColor => HviKColors.Penguin;
    public override BaseKeybind? Keybind => MiraGlobalKeybinds.PrimaryAbility;

    // Nochmal druecken = sofort toeten
    public override bool IsEffectCancellable() => true;

    public override bool Enabled(RoleBehaviour? role) => role is PenguinRole;

    public override PlayerControl? GetTarget() =>
        PlayerControl.LocalPlayer.GetClosestPlayer(false, Distance, predicate: p => RoleHelpers.IsFreeToMove(p));

    public override void SetOutline(bool active)
    {
        Target?.cosmetics.SetOutline(active, new Il2CppSystem.Nullable<Color>(HviKColors.Penguin));
    }

    public override bool CanUse() => EffectActive ? PlayerControl.LocalPlayer.moveable : base.CanUse();

    protected override void OnClick()
    {
        if (Target == null) return;
        _victim = Target;
        PenguinAbility.RpcGrab(PlayerControl.LocalPlayer, _victim);
        OverrideName("Töten");
    }

    public override void OnEffectEnd()
    {
        OverrideName("Packen");
        var victim = _victim;
        _victim = null;
        if (victim == null) return;

        var penguin = PlayerControl.LocalPlayer;
        PenguinAbility.RpcRelease(penguin, victim);

        // Bei Meeting-Start ruft Mira das auch auf - dann kommt das Opfer frei.
        var inMeeting = MeetingHud.Instance || ExileController.Instance;
        if (!inMeeting && !penguin.Data.IsDead && !victim.Data.IsDead)
        {
            penguin.RpcCustomMurder(victim, MeetingCheck.OutsideMeeting, resetKillTimer: false, teleportMurderer: false);
        }
    }

    protected override void FixedUpdate(PlayerControl penguin)
    {
        base.FixedUpdate(penguin);
        if (EffectActive && _victim != null && (penguin.Data.IsDead || _victim.Data.IsDead))
        {
            ResetCooldownAndOrEffect();
        }
    }
}
