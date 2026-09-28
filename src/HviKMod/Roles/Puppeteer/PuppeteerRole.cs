using HviKMod.Localization;
using MiraAPI.Roles;
using UnityEngine;

namespace HviKMod.Roles.Puppeteer;

public class PuppeteerRole : ImpostorRole, ICustomRole
{
    public string RoleName => "Puppeteer";
    public string RoleDescription => Loc.T(
        "Bring andere Spieler dazu, für dich zu töten",
        "Make other players kill for you");
    public string RoleMedDescription => Loc.T(
        "Mach einen Spieler zur Puppe. Sie tötet den ersten Spieler, dem sie zu nahe kommt.",
        "Turn a player into your puppet. It kills the first player it gets close to.");
    public string RoleLongDescription => Loc.T(
        "Markiere einen Spieler in deiner Nähe als Puppe.\n" +
        "Die Puppe tötet automatisch den ersten Spieler, dem sie zu nahe kommt, und steht dann neben der Leiche.",
        "Mark a nearby player as your puppet.\n" +
        "The puppet automatically kills the first player it gets close to and ends up next to the body.");

    public Color RoleColor => HviKColors.Puppeteer;
    public ModdedRoleTeams Team => ModdedRoleTeams.Impostor;

    public CustomRoleConfiguration Configuration => new(this)
    {
        MaxRoleCount = 1,
    };
}
