using HviKMod.Localization;
using System.Linq;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Keybinds;
using MiraAPI.Utilities.Assets;
using UnityEngine;

namespace HviKMod.Roles.Transporter;

public class TransporterButton : CustomActionButton
{
    private static TransporterOptions Options => OptionGroupSingleton<TransporterOptions>.Instance;

    public override string Name => Loc.T("Tauschen", "Swap");
    public override float Cooldown => Options.Cooldown;
    public override LoadableAsset<Sprite> Sprite => HviKAssets.TransporterButton;
    public override Color TextOutlineColor => HviKColors.Transporter;
    public override BaseKeybind? Keybind => MiraGlobalKeybinds.PrimaryAbility;

    // Ladungen: 0 bedeutet hier wirklich 0 (nicht unendlich) - aufgeladen wird durch Aufgaben.
    public override bool ZeroIsInfinite { get; set; } = false;
    public override int MaxUses => (int)Options.StartCharges;

    public override bool Enabled(RoleBehaviour? role) => role is TransporterRole;

    public override bool CanUse() => base.CanUse() && TransporterAbility.GetTransportablePlayers().Length >= 2;

    protected override void OnClick()
    {
        var players = TransporterAbility.GetTransportablePlayers().OrderBy(_ => Random.value).Take(2).ToArray();
        if (players.Length < 2) return;

        TransporterAbility.RpcTransport(PlayerControl.LocalPlayer, players[0], players[1]);
    }
}
