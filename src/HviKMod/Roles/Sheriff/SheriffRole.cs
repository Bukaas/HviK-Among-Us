using HviKMod.Localization;
using MiraAPI.Roles;
using UnityEngine;

namespace HviKMod.Roles.Sheriff;

public class SheriffRole : CrewmateRole, ICustomRole
{
    public string RoleName => "Sheriff";
    public string RoleDescription => Loc.T("Erschieße die Verräter", "Shoot the traitors");
    public string RoleMedDescription => Loc.T(
        "Erschieße Impostor. Triffst du einen Unschuldigen, stirbst du selbst.",
        "Shoot Impostors. If you hit an innocent, you die yourself.");
    public string RoleLongDescription => Loc.T(
        "Du kannst Spieler erschießen. Ist das Ziel ein Impostor, stirbt es.\n" +
        "Erwischst du einen Crewmate, stirbst du selbst (je nach Einstellung auch das Ziel).",
        "You can shoot players. If the target is an Impostor, it dies.\n" +
        "If you hit a Crewmate, you die yourself (depending on settings, the target too).");

    public Color RoleColor => HviKColors.Sheriff;
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;

    public CustomRoleConfiguration Configuration => new(this)
    {
        MaxRoleCount = 3,
    };
}
