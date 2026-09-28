using MiraAPI.Roles;
using UnityEngine;

namespace HviKMod.Roles.Penguin;

public class PenguinRole : ImpostorRole, ICustomRole
{
    public string RoleName => "Penguin";
    public string RoleDescription => "Ziehe deine Opfer";
    public string RoleMedDescription => "Pack ein Opfer und zieh es mit dir. Am Ende stirbt es.";
    public string RoleLongDescription =>
        "Pack einen Spieler in deiner Nähe. Er kann sich nicht mehr bewegen und wird mitgezogen.\n" +
        "Nach Ablauf der Zeit (oder wenn du nochmal drückst) stirbt er. Bei einem Meeting kommt er frei.";

    public Color RoleColor => HviKColors.Penguin;
    public ModdedRoleTeams Team => ModdedRoleTeams.Impostor;

    public CustomRoleConfiguration Configuration => new(this)
    {
        MaxRoleCount = 1,
    };
}
