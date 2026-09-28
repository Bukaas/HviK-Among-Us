using System.Linq;
using HviKMod.Localization;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Utilities;
using MiraAPI.Voting;
using UnityEngine;

namespace HviKMod.Modifiers.Guesser;

/// <summary>
/// Stirbt jemand WAEHREND eines Meetings (Guesser), muss das Stimmfeld aktualisiert werden.
/// Laeuft auf allen Clients; die Stimmen raeumt der Host auf.
/// </summary>
public static class MeetingDeathHandler
{
    [RegisterEvent]
    public static void OnMurder(AfterMurderEvent @event)
    {
        var meeting = MeetingHud.Instance;
        if (!meeting) return;

        var dead = @event.Target;

        var deadArea = meeting.playerStates.ToArray().FirstOrDefault(p => p.PlayerId == dead.PlayerId);
        if (deadArea != null)
        {
            deadArea.AmDead = true;
            deadArea.Overlay.gameObject.SetActive(true);
            deadArea.XMark.gameObject.SetActive(true);
        }

        if (dead.AmOwner)
        {
            if (Minigame.Instance) Minigame.Instance.Close();
            meeting.SkipVoteButton.gameObject.SetActive(false);
        }

        var message = Loc.T($"{dead.Data.PlayerName} ist gestorben.", $"{dead.Data.PlayerName} died.");
        var color = ColorUtility.ToHtmlStringRGB(HviKColors.Guesser);
        HudManager.Instance.Chat.AddChat(PlayerControl.LocalPlayer, $"<color=#{color}>{message}</color>");

        if (!AmongUsClient.Instance.AmHost) return;

        // Eigene Stimmen des Toten zuruecknehmen ...
        foreach (var vote in dead.GetVoteData().Votes.ToList())
        {
            VotingUtils.RpcRemoveVote(PlayerControl.LocalPlayer, dead.PlayerId, vote.Suspect);
        }

        // ... und Stimmen FUER den Toten: das macht MiraAPI beim Verlassen eines Spielers schon genauso
        // (inkl. Pruefung, ob die Abstimmung jetzt vorbei ist).
        meeting.HandleDisconnect(dead, DisconnectReasons.ExitGame);
    }
}
