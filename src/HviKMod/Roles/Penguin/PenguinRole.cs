using HviKMod.Localization;
using MiraAPI.Roles;
using UnityEngine;

namespace HviKMod.Roles.Penguin;

public class PenguinRole : ImpostorRole, ICustomRole
{
    public string RoleName => "Penguin";
    public string RoleDescription => Loc.T("Ziehe deine Opfer", "Drag your victims");
    public string RoleMedDescription => Loc.T(
        "Pack ein Opfer und zieh es mit dir. Am Ende stirbt es.",
        "Grab a victim and drag it along. In the end, it dies.");
    public string RoleLongDescription => Loc.T(
        "Pack einen Spieler in deiner Nähe. Er kann sich nicht mehr bewegen und wird mitgezogen.\n" +
        "Nach Ablauf der Zeit (oder wenn du nochmal drückst) stirbt er. Bei einem Meeting kommt er frei.",
        "Grab a nearby player. They can no longer move and get dragged along.\n" +
        "When the time runs out (or you press again) they die. A meeting sets them free.");

    public Color RoleColor => HviKColors.Penguin;
    public ModdedRoleTeams Team => ModdedRoleTeams.Impostor;

    public CustomRoleConfiguration Configuration => new(this)
    {
        MaxRoleCount = 1,
    };
}
