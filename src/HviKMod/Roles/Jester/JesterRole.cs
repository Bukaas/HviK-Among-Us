using HviKMod.Localization;
using MiraAPI.GameEnd;
using MiraAPI.Roles;
using UnityEngine;

namespace HviKMod.Roles.Jester;

public class JesterRole : CrewmateRole, ICustomRole
{
    public string RoleName => "Jester";
    public string RoleDescription => Loc.T("Lass dich rausvoten", "Get voted out");
    public string RoleMedDescription => Loc.T(
        "Du gewinnst allein, wenn du im Meeting rausgewählt wirst.",
        "You win alone if you get voted out in a meeting.");
    public string RoleLongDescription => Loc.T(
        "Benimm dich verdächtig! Wirst du im Meeting rausgewählt, gewinnst du allein und das Spiel endet.\n" +
        "Deine Aufgaben zählen nicht für die Crew.",
        "Act suspicious! If you get voted out in a meeting, you win alone and the game ends.\n" +
        "Your tasks don't count for the crew.");

    public Color RoleColor => HviKColors.Jester;
    public ModdedRoleTeams Team => ModdedRoleTeams.Custom;

    public CustomRoleConfiguration Configuration => new(this)
    {
        MaxRoleCount = 1,
        CanUseVent = RoleHelpers.Option<JesterOptions, bool>(o => o.CanUseVents, false),
        TasksCountForProgress = false,
    };

    public override bool DidWin(GameOverReason gameOverReason)
    {
        return gameOverReason == CustomGameOver.GameOverReason<JesterGameOver>();
    }
}
