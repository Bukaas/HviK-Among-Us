using System;
using MiraAPI.GameOptions;

namespace HviKMod.Roles;

public static class RoleHelpers
{
    /// <summary>
    /// Liest eine Einstellung. Waehrend des Mod-Starts (Rollen werden vor den Optionen registriert)
    /// gibt es die Optionen noch nicht - dann wird <paramref name="fallback"/> zurueckgegeben.
    /// </summary>
    public static T Option<TGroup, T>(Func<TGroup, T> read, T fallback) where TGroup : AbstractOptionGroup
    {
        try
        {
            return read(OptionGroupSingleton<TGroup>.Instance);
        }
        catch (InvalidOperationException)
        {
            return fallback;
        }
    }

    public static PlayerControl? PlayerById(byte id)
    {
        var data = GameData.Instance ? GameData.Instance.GetPlayerById(id) : null;
        return data != null ? data.Object : null;
    }

    /// <summary>Spieler lebt, ist verbunden und nicht gerade in einer Sonder-Bewegung (Leiter, Vent, Plattform).</summary>
    public static bool IsFreeToMove(PlayerControl p)
    {
        return p && !p.Data.IsDead && !p.Data.Disconnected && !p.inVent && !p.walkingToVent && !p.onLadder &&
               !p.inMovingPlat;
    }
}
