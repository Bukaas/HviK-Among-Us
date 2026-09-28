using HviKMod.Localization;
using MiraAPI.Roles;
using UnityEngine;

namespace HviKMod.Roles.Transporter;

public class TransporterRole : CrewmateRole, ICustomRole
{
    public string RoleName => "Transporter";
    public string RoleDescription => Loc.T(
        "Erledige Aufgaben, um 2 Spieler zu tauschen",
        "Complete tasks to swap 2 players");
    public string RoleMedDescription => Loc.T(
        "Erledigte Aufgaben laden deinen Button auf. Damit tauschst du die Positionen von 2 zufälligen Spielern.",
        "Completed tasks charge your button. Use it to swap the positions of 2 random players.");
    public string RoleLongDescription => Loc.T(
        "Jede erledigte Aufgabe bringt dich einer Ladung näher.\n" +
        "Mit einer Ladung tauschst du die Positionen von 2 zufälligen lebenden Spielern.",
        "Every completed task brings you closer to a charge.\n" +
        "With a charge you swap the positions of 2 random living players.");

    public Color RoleColor => HviKColors.Transporter;
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;

    public CustomRoleConfiguration Configuration => new(this)
    {
        MaxRoleCount = 3,
    };
}
