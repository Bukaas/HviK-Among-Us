using System.Collections.Generic;
using System.Linq;
using AmongUs.GameOptions;
using MiraAPI.Roles;
using UnityEngine;

namespace HviKMod.Modifiers.Guesser;

/// <summary>Welche Rollen stehen im Guesser-Menue zur Auswahl?</summary>
public static class GuessRoles
{
    private static readonly RoleTypes[] GhostRoles =
        [RoleTypes.CrewmateGhost, RoleTypes.ImpostorGhost, RoleTypes.GuardianAngel];

    /// <summary>Alle Rollen, die in dieser Runde vorkommen koennen und nicht zum eigenen Team gehoeren.</summary>
    public static List<RoleBehaviour> For(GuesserModifier guesser)
    {
        return RoleManager.Instance.AllRoles.ToArray()
            .Where(r => r && !GhostRoles.Contains(r.Role) && guesser.CanGuessTeamOf(r) && CanAppear(r, guesser))
            .GroupBy(r => r.Role).Select(g => g.First())
            .OrderBy(r => r.IsImpostor ? 0 : 1).ThenBy(DisplayName)
            .ToList();
    }

    private static bool CanAppear(RoleBehaviour role, GuesserModifier guesser)
    {
        if (role.Role is RoleTypes.Crewmate or RoleTypes.Impostor)
        {
            return guesser.Options.CanGuessBasicRoles;
        }

        if (role is ICustomRole custom)
        {
            return custom.GetCount() > 0 && custom.GetChance() > 0;
        }

        var roleOptions = GameOptionsManager.Instance.CurrentGameOptions.RoleOptions;
        return roleOptions.GetNumPerGame(role.Role) > 0 && roleOptions.GetChancePerGame(role.Role) > 0;
    }

    public static string DisplayName(RoleBehaviour role) =>
        role is ICustomRole custom ? custom.RoleName : role.NiceName;

    public static Color DisplayColor(RoleBehaviour role) =>
        role is ICustomRole custom ? custom.RoleColor : role.TeamColor;
}
