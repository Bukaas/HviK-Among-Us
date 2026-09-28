using MiraAPI.GameEnd;
using MiraAPI.Roles;
using UnityEngine;

namespace HviKMod.Roles.Jester;

public class JesterRole : CrewmateRole, ICustomRole
{
    public string RoleName => "Jester";
    public string RoleDescription => "Lass dich rausvoten";
    public string RoleMedDescription => "Du gewinnst allein, wenn du im Meeting rausgewählt wirst.";
    public string RoleLongDescription =>
        "Benimm dich verdächtig! Wirst du im Meeting rausgewählt, gewinnst du allein und das Spiel endet.\n" +
        "Deine Aufgaben zählen nicht für die Crew.";

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
