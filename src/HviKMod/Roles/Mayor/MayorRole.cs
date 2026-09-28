using HviKMod.Localization;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Meeting;
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
        "Du hast in jedem Meeting zusätzliche Stimmen (einstellbar). Du kannst sie auf einen oder mehrere Spieler verteilen.",
        "You get extra votes in every meeting (configurable). You can put them on one or several players.");

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
    [RegisterEvent]
    public static void OnMeetingStart(StartMeetingEvent _)
    {
        var extra = (int)OptionGroupSingleton<MayorOptions>.Instance.ExtraVotes;
        foreach (var player in PlayerControl.AllPlayerControls.ToArray())
        {
            if (player.Data.Role is MayorRole && !player.Data.IsDead)
            {
                player.GetVoteData().IncreaseRemainingVotes(extra);
            }
        }
    }
}
