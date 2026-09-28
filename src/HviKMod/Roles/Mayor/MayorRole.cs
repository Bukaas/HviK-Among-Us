using HviKMod.Localization;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Meeting.Voting;
using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using UnityEngine;

namespace HviKMod.Roles.Mayor;

public class MayorRole : CrewmateRole, ICustomRole
{
    public string RoleName => "Mayor";
    public string RoleDescription => Loc.T("Deine Stimmen zählen mehrfach", "Your votes count multiple times");
    public string RoleMedDescription => Loc.T(
        "Du hast in jedem Meeting zusätzliche Stimmen.",
        "You get extra votes in every meeting.");
    public string RoleLongDescription => Loc.T(
        "Deine Stimme zählt mehrfach (einstellbar) - alle Stimmen gehen immer auf denselben Spieler.",
        "Your vote counts multiple times (configurable) - all votes always go to the same player.");

    public Color RoleColor => HviKColors.Mayor;
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;
    public CustomRoleConfiguration Configuration => new(this) { MaxRoleCount = 1 };
}

public class MayorOptions : AbstractRoleOptionGroup<MayorRole>
{
    public override string GroupName => "Mayor";

    [ModdedNumberOption("HviK.Mayor.ExtraVotes", 1, 3)]
    public float ExtraVotes { get; set; } = 1f;
}

public static class MayorEvents
{
    /// <summary>
    /// Der Mayor stimmt wie alle nur einmal ab - diese eine Stimme zaehlt aber (1 + Zusatzstimmen) mal
    /// fuer dasselbe Ziel. So kann er seine Stimmen nicht auf mehrere Spieler verteilen.
    /// </summary>
    [RegisterEvent(15)]
    public static void OnVote(HandleVoteEvent @event)
    {
        if (@event.VoteData.Owner.Data.Role is not MayorRole) return;

        var votes = 1 + (int)OptionGroupSingleton<MayorOptions>.Instance.ExtraVotes;
        @event.VoteData.SetRemainingVotes(0);
        for (var i = 0; i < votes; i++)
        {
            @event.VoteData.VoteForPlayer(@event.TargetId);
        }

        @event.Cancel();
    }
}
