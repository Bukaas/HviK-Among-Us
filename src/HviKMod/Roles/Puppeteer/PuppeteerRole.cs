using MiraAPI.Roles;
using UnityEngine;

namespace HviKMod.Roles.Puppeteer;

public class PuppeteerRole : ImpostorRole, ICustomRole
{
    public string RoleName => "Puppeteer";
    public string RoleDescription => "Bring andere Spieler dazu, für dich zu töten";
    public string RoleMedDescription => "Mach einen Spieler zur Puppe. Sie tötet den ersten Spieler, dem sie zu nahe kommt.";
    public string RoleLongDescription =>
        "Markiere einen Spieler in deiner Nähe als Puppe.\n" +
        "Die Puppe tötet automatisch den ersten Spieler, dem sie zu nahe kommt, und steht dann neben der Leiche.";

    public Color RoleColor => HviKColors.Puppeteer;
    public ModdedRoleTeams Team => ModdedRoleTeams.Impostor;

    public CustomRoleConfiguration Configuration => new(this)
    {
        MaxRoleCount = 1,
    };
}
