using System.Collections;
using HviKMod.Localization;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using Reactor.Utilities;
using UnityEngine;

namespace HviKMod.Roles.Stealth;

public class StealthRole : ImpostorRole, ICustomRole
{
    public string RoleName => "Stealth";
    public string RoleDescription => Loc.T("Dein Kill blendet jeden im Raum", "Your kill blinds everyone in the room");
    public string RoleMedDescription => Loc.T(
        "Wenn du tötest, sehen alle anderen im selben Raum kurz nichts mehr.",
        "When you kill, everyone else in the same room is briefly blinded.");
    public string RoleLongDescription => RoleMedDescription;

    public Color RoleColor => HviKColors.Stealth;
    public ModdedRoleTeams Team => ModdedRoleTeams.Impostor;
    public CustomRoleConfiguration Configuration => new(this) { MaxRoleCount = 1 };
}

public class StealthOptions : AbstractRoleOptionGroup<StealthRole>
{
    public override string GroupName => "Stealth";

    [ModdedNumberOption("HviK.Stealth.BlindDuration", 1, 10, 0.5f, MiraNumberSuffixes.Seconds)]
    public float BlindDuration { get; set; } = 3f;
}

public static class StealthEvents
{
    /// <summary>Laeuft auf allen Clients: ist man selbst im selben Raum wie das Opfer, wird es kurz schwarz.</summary>
    [RegisterEvent]
    public static void OnMurder(AfterMurderEvent @event)
    {
        if (@event.Source.Data.Role is not StealthRole || MeetingHud.Instance) return;

        var me = PlayerControl.LocalPlayer;
        if (!me || me == @event.Source || me == @event.Target || me.Data.IsDead || me.Data.Role.IsImpostor) return;

        var room = RoomAt(@event.Target.GetTruePosition());
        if (room == null || RoomAt(me.GetTruePosition()) != room) return;

        Coroutines.Start(CoBlind(OptionGroupSingleton<StealthOptions>.Instance.BlindDuration));
    }

    private static PlainShipRoom? RoomAt(Vector2 position)
    {
        if (!ShipStatus.Instance) return null;
        foreach (var room in ShipStatus.Instance.AllRooms)
        {
            if (room.roomArea && room.roomArea.OverlapPoint(position)) return room;
        }

        return null;
    }

    private static IEnumerator CoBlind(float seconds)
    {
        var screen = HudManager.Instance ? HudManager.Instance.FullScreen : null;
        if (!screen) yield break;

        var wasActive = screen.gameObject.activeSelf;
        var oldColor = screen.color;
        screen.gameObject.SetActive(true);
        screen.color = new Color(0f, 0f, 0f, 0.97f);

        yield return new WaitForSeconds(seconds);

        if (!screen) yield break;
        screen.color = oldColor;
        screen.gameObject.SetActive(wasActive);
    }
}
