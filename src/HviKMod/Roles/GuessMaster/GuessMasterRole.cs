using HviKMod.Localization;
using MiraAPI.Roles;
using Reactor.Networking.Attributes;
using UnityEngine;

namespace HviKMod.Roles.GuessMaster;

public class GuessMasterRole : CrewmateRole, ICustomRole
{
    public string RoleName => "Guess Master";
    public string RoleDescription => Loc.T("Du hörst jeden Tipp mit", "You hear every guess");
    public string RoleMedDescription => Loc.T(
        "Du erfährst im Chat jeden Tipp der Guesser: wer, auf wen, welche Rolle, richtig oder falsch.",
        "You see every Guesser guess in chat: who, on whom, which role, right or wrong.");
    public string RoleLongDescription => RoleMedDescription;

    public Color RoleColor => HviKColors.GuessMaster;
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;
    public CustomRoleConfiguration Configuration => new(this) { MaxRoleCount = 1 };
}

public static class GuessMasterInfo
{
    /// <summary>Wird bei jedem Guesser-Tipp an alle geschickt - nur der Guess Master zeigt ihn an.</summary>
    [MethodRpc((uint)HviKRpc.AnnounceGuess)]
    public static void RpcAnnounceGuess(PlayerControl guesser, PlayerControl target, string roleName, bool correct)
    {
        var me = PlayerControl.LocalPlayer;
        if (!me || me.Data.Role is not GuessMasterRole || !HudManager.Instance) return;

        var color = ColorUtility.ToHtmlStringRGB(HviKColors.GuessMaster);
        var result = correct ? Loc.T("RICHTIG", "CORRECT") : Loc.T("FALSCH", "WRONG");
        var text = Loc.T(
            $"{guesser.Data.PlayerName} hat {target.Data.PlayerName} als {roleName} geraten: {result}",
            $"{guesser.Data.PlayerName} guessed {target.Data.PlayerName} as {roleName}: {result}");
        HudManager.Instance.Chat.AddChat(me, $"<color=#{color}>🔮 {text}</color>");
    }
}
