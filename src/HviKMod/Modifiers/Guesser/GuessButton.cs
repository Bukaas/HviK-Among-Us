using System.Linq;
using HviKMod.Localization;
using MiraAPI.MeetingAbilities;
using MiraAPI.Modifiers;
using MiraAPI.Networking;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using UnityEngine;

namespace HviKMod.Modifiers.Guesser;

/// <summary>
/// "Raten"-Button neben jedem Spieler im Meeting (nur fuer Guesser).
/// Die Tipps werden im Modifier gezaehlt, nicht ueber Mira (MaxUses = 0 = unbegrenzt).
/// </summary>
public class GuessButton : TargetedMeetingButton
{
    public override string Name => Loc.T("Raten", "Guess");
    public override int MaxUses => 0;
    // Nicht 0: MiraAPI teilt fuer die Abklingzeit-Anzeige durch die Abklingzeit -> 0/0 = NaN -> Button unsichtbar.
    public override float Cooldown => 0.01f;
    public override LoadableAsset<Sprite> Sprite => HviKAssets.GuessButton;
    public override Color OutlineColor => HviKColors.Guesser;

    private static GuesserModifier? LocalGuesser => PlayerControl.LocalPlayer.GetModifier<GuesserModifier>();

    public override bool Enabled(RoleBehaviour r) => LocalGuesser is { CanGuessNow: true };

    public override bool IsTargetValid(PlayerVoteArea voteArea)
    {
        if (!base.IsTargetValid(voteArea) || voteArea.AmDead) return false;

        // Impostor-Guesser kennt seine Mit-Impostor - die muss er nicht raten.
        var target = voteArea.GetPlayer();
        return target != null && !(LocalGuesser is ImpostorGuesserModifier && target.Data.Role.IsImpostor);
    }

    protected override void OnClick(PlayerVoteArea voteArea)
    {
        var guesser = LocalGuesser;
        var target = voteArea.GetPlayer();
        if (guesser is not { CanGuessNow: true } || target == null) return;

        var roles = GuessRoles.For(guesser);
        if (roles.Count == 0) return;

        GuessMenu.Create().Begin(roles, role => MakeGuess(guesser, target, role));
    }

    private static void MakeGuess(GuesserModifier guesser, PlayerControl target, RoleBehaviour guessedRole)
    {
        if (!guesser.CanGuessNow || target.Data.IsDead || !MeetingHud.Instance) return;

        guesser.GuessesLeft--;
        guesser.GuessedThisMeeting = true;

        var me = PlayerControl.LocalPlayer;
        var correct = target.Data.Role.Role == guessedRole.Role;
        var victim = correct ? target : me;

        // Der Guess Master bekommt jeden Tipp mit
        Roles.GuessMaster.GuessMasterInfo.RpcAnnounceGuess(me, target, GuessRoles.DisplayName(guessedRole), correct);

        if (!correct && !guesser.Options.WrongGuessKillsGuesser)
        {
            HudManager.Instance.Chat.AddChat(me, Loc.T("Falsch geraten!", "Wrong guess!"));
            HideButtonsIfDone(guesser);
            return;
        }

        me.RpcCustomMurder(
            victim,
            MeetingCheck.ForMeeting,
            createDeadBody: false,
            teleportMurderer: false,
            showKillAnim: false,
            playKillSound: true);

        HideButtonsIfDone(guesser);
    }

    /// <summary>Keine Tipps mehr (oder nur einer pro Meeting) -> Buttons ausblenden.</summary>
    private static void HideButtonsIfDone(GuesserModifier guesser)
    {
        if (guesser.CanGuessNow || !MeetingHud.Instance) return;

        foreach (var ability in MeetingHud.Instance.playerStates
                     .SelectMany(p => p.GetComponentsInChildren<MeetingAbilityBehaviour>(true)))
        {
            ability.gameObject.SetActive(false);
        }
    }
}
