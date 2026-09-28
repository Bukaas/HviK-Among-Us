using HviKMod.Localization;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Keybinds;
using MiraAPI.Networking;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using UnityEngine;

namespace HviKMod.Roles.Puppeteer;

/// <summary>
/// Markiert eine Puppe. Solange der Effekt laeuft, prueft der Client des Puppeteers,
/// ob die Puppe jemandem nahe kommt - dann wird der Kill der Puppe zugeschrieben.
/// </summary>
public class PuppeteerControlButton : CustomActionButton<PlayerControl>
{
    private static PuppeteerOptions Options => OptionGroupSingleton<PuppeteerOptions>.Instance;

    private PlayerControl? _puppet;

    public override string Name => Loc.T("Kontrollieren", "Control");
    public override float Cooldown => Options.Cooldown;
    public override float EffectDuration => Options.ControlDuration;
    public override LoadableAsset<Sprite> Sprite => HviKAssets.PuppeteerButton;
    public override Color TextOutlineColor => HviKColors.Puppeteer;
    public override BaseKeybind? Keybind => MiraGlobalKeybinds.PrimaryAbility;
    public override ButtonLocation Location { get; set; } = ButtonLocation.BottomRight;

    public override bool Enabled(RoleBehaviour? role) => role is PuppeteerRole;

    public override PlayerControl? GetTarget() => PlayerControl.LocalPlayer.GetClosestPlayer(false, Distance);

    public override void SetOutline(bool active)
    {
        Target?.cosmetics.SetOutline(active, new Il2CppSystem.Nullable<Color>(HviKColors.Puppeteer));
    }

    protected override void OnClick()
    {
        _puppet = Target;
    }

    public override void OnEffectEnd()
    {
        _puppet = null;
    }

    protected override void FixedUpdate(PlayerControl puppeteer)
    {
        base.FixedUpdate(puppeteer);
        if (!EffectActive || _puppet == null) return;

        if (MeetingHud.Instance || puppeteer.Data.IsDead || !RoleHelpers.IsFreeToMove(_puppet))
        {
            ResetCooldownAndOrEffect();
            return;
        }

        var victim = _puppet.GetClosestPlayer(
            Options.PuppetCanKillImpostors,
            GetKillDistance(),
            predicate: p => p.PlayerId != puppeteer.PlayerId && RoleHelpers.IsFreeToMove(p));
        if (victim == null) return;

        // Nicht RpcFramedCustomMurder: das animiert/teleportiert in MiraAPI 0.5.0 den Puppeteer statt der
        // Puppe, und bei allen anderen bleibt die Leiche unsichtbar. Stattdessen ist die Puppe selbst der
        // Taeter - normale Kill-Animation, Leiche fuer alle sichtbar, Puppe steht daneben.
        _puppet.RpcCustomMurder(victim, MeetingCheck.OutsideMeeting, resetKillTimer: false);
        ResetCooldownAndOrEffect();
    }

    private static float GetKillDistance()
    {
        var options = GameOptionsManager.Instance.currentNormalGameOptions;
        var distances = options.GetFloatArray(AmongUs.GameOptions.FloatArrayOptionNames.KillDistances);
        return distances[Mathf.Clamp(options.KillDistance, 0, distances.Length - 1)];
    }
}
