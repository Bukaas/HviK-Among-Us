using MiraAPI.Roles;
using UnityEngine;

namespace HviKMod.Roles.Sheriff;

public class SheriffRole : CrewmateRole, ICustomRole
{
    public string RoleName => "Sheriff";
    public string RoleDescription => "Erschieße die Verräter";
    public string RoleMedDescription => "Erschieße Impostor. Triffst du einen Unschuldigen, stirbst du selbst.";
    public string RoleLongDescription =>
        "Du kannst Spieler erschießen. Ist das Ziel ein Impostor, stirbt es.\n" +
        "Erwischst du einen Crewmate, stirbst du selbst (je nach Einstellung auch das Ziel).";

    public Color RoleColor => HviKColors.Sheriff;
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;

    public CustomRoleConfiguration Configuration => new(this)
    {
        MaxRoleCount = 3,
    };
}
