using HarmonyLib;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Modifiers;

namespace HviKMod.Modifiers.FirstDeathShield;

/// <summary>
/// Merkt sich den ersten Toten eines Spiels (fuer den Schild im naechsten Spiel)
/// und blockt Kills gegen Spieler mit Schild.
/// </summary>
public static class FirstDeathTracker
{
    /// <summary>Erster Toter des letzten Spiels - bekommt im naechsten Spiel den Schild.</summary>
    public static string? LastFirstDeadName { get; private set; }

    private static bool _recordedThisGame = true;

    // Rollen/Modifier sind beim Intro schon verteilt -> gemerkten Namen jetzt verwerfen,
    // damit derselbe Spieler nicht dauerhaft geschuetzt ist, und neu mitschreiben.
    [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.CoBegin))]
    public static class ResetPatch
    {
        public static void Prefix()
        {
            LastFirstDeadName = null;
            _recordedThisGame = false;
        }
    }

    [RegisterEvent]
    public static void OnMurder(AfterMurderEvent @event)
    {
        if (_recordedThisGame || MeetingHud.Instance || ExileController.Instance) return;
        if (@event.Source == @event.Target) return; // Sheriff-Fehlschuss zaehlt nicht

        LastFirstDeadName = @event.Target.Data.PlayerName;
        _recordedThisGame = true;
    }

    [RegisterEvent]
    public static void OnBeforeMurder(BeforeMurderEvent @event)
    {
        if (MeetingHud.Instance || ExileController.Instance) return;
        if (@event.Source == @event.Target || !@event.Target.HasModifier<FirstDeathShieldModifier>()) return;

        @event.Cancel();
        if (@event.Source.AmOwner) @event.Source.SetKillTimer(10f);
    }
}
