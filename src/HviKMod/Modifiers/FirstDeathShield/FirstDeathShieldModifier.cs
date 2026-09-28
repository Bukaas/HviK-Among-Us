using HviKMod.Localization;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Modifiers.Types;
using MiraAPI.Utilities.Assets;
using UnityEngine;

namespace HviKMod.Modifiers.FirstDeathShield;

/// <summary>
/// Wer im letzten Spiel als Erstes gestorben ist, bekommt in diesem Spiel einen Schild bis zum
/// ersten Meeting: Kills gegen ihn schlagen fehl. Ueber dem Namen steht "Died Round 1".
/// </summary>
public class FirstDeathShieldModifier : GameModifier
{
    public const string Label = "Died Round 1";

    public override string ModifierName => Loc.T("Schild (Died Round 1)", "Shield (Died Round 1)");

    public override string GetDescription() => Loc.T(
        "Du bist letztes Spiel als Erstes gestorben: Bis zum ersten Meeting kann dich niemand töten.",
        "You died first last game: nobody can kill you until the first meeting.");

    public override LoadableAsset<Sprite>? ModifierIcon => HviKAssets.GuessButton;

    // Wird nur dem gemerkten Spieler gegeben (siehe FirstDeathTracker).
    public override int GetAmountPerGame() => Enabled && FirstDeathTracker.LastFirstDeadName != null ? 1 : 0;

    public override int GetAssignmentChance() => 100;

    public override bool IsModifierValidOn(RoleBehaviour role) =>
        Enabled && role.Player && role.Player.Data.PlayerName == FirstDeathTracker.LastFirstDeadName;

    private static bool Enabled => OptionGroupSingleton<FirstDeathShieldOptions>.Instance.Enabled;

    public override void OnMeetingStart()
    {
        // Schild gilt nur bis zum ersten Meeting
        RestoreName();
        Player.RemoveModifier(this);
    }

    public override void OnDeath(DeathReason reason)
    {
        RestoreName();
        Player.RemoveModifier(this);
    }

    public override void Update()
    {
        var nameText = Player ? Player.cosmetics.nameText : null;
        if (nameText == null) return;

        var wanted = $"{Player.Data.PlayerName}\n<size=60%><color=#64DC64>🛡 {Label}</color></size>";
        if (nameText.text != wanted) nameText.text = wanted;
    }

    private void RestoreName()
    {
        if (Player && Player.cosmetics.nameText) Player.cosmetics.nameText.text = Player.Data.PlayerName;
    }
}
