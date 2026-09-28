using System.Linq;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Player;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Utilities;
using Reactor.Networking.Attributes;
using UnityEngine;

namespace HviKMod.Roles.Transporter;

public static class TransporterAbility
{
    /// <summary>Erledigte Aufgaben geben Ladungen (nur fuer den lokalen Transporter).</summary>
    [RegisterEvent]
    public static void OnTaskComplete(CompleteTaskEvent @event)
    {
        if (!@event.Player.AmOwner || @event.Player.Data.Role is not TransporterRole) return;

        var tasksPerCharge = Mathf.Max(1, (int)OptionGroupSingleton<TransporterOptions>.Instance.TasksPerCharge);
        var done = @event.Player.myTasks.ToArray().Count(t => t.IsComplete);
        if (done % tasksPerCharge == 0)
        {
            CustomButtonSingleton<TransporterButton>.Instance.IncreaseUses();
        }
    }

    public static PlayerControl[] GetTransportablePlayers() =>
        Helpers.GetAlivePlayers().Where(RoleHelpers.IsFreeToMove).ToArray();

    /// <summary>Tauscht zwei Spieler. Laeuft auf allen Clients, damit alle dasselbe sehen.</summary>
    [MethodRpc((uint)HviKRpc.Transport)]
    public static void RpcTransport(PlayerControl transporter, PlayerControl player1, PlayerControl player2)
    {
        if (transporter.Data.Role is not TransporterRole) return;
        if (!RoleHelpers.IsFreeToMove(player1) || !RoleHelpers.IsFreeToMove(player2)) return;

        Vector2 pos1 = player1.transform.position;
        Vector2 pos2 = player2.transform.position;
        MoveTo(player1, pos2);
        MoveTo(player2, pos1);
    }

    private static void MoveTo(PlayerControl player, Vector2 position)
    {
        player.MyPhysics.ResetMoveState();
        player.NetTransform.SnapTo(position, (ushort)(player.NetTransform.lastSequenceId + 1));

        if (!player.AmOwner) return;

        if (Minigame.Instance) Minigame.Instance.Close();
        var cam = Camera.main!.GetComponent<FollowerCamera>();
        if (cam) cam.SnapToTarget();
    }
}
