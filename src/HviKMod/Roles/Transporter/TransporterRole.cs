using MiraAPI.Roles;
using UnityEngine;

namespace HviKMod.Roles.Transporter;

public class TransporterRole : CrewmateRole, ICustomRole
{
    public string RoleName => "Transporter";
    public string RoleDescription => "Erledige Aufgaben, um 2 Spieler zu tauschen";
    public string RoleMedDescription => "Erledigte Aufgaben laden deinen Button auf. Damit tauschst du die Positionen von 2 zufälligen Spielern.";
    public string RoleLongDescription =>
        "Jede erledigte Aufgabe bringt dich einer Ladung näher.\n" +
        "Mit einer Ladung tauschst du die Positionen von 2 zufälligen lebenden Spielern.";

    public Color RoleColor => HviKColors.Transporter;
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;

    public CustomRoleConfiguration Configuration => new(this)
    {
        MaxRoleCount = 3,
    };
}
