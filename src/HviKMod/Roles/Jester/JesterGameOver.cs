using System.Linq;
using HarmonyLib;
using MiraAPI.GameEnd;

namespace HviKMod.Roles.Jester;

public class JesterGameOver : CustomGameOver
{
    /// <summary>Spieler-ID des rausgewaehlten Jesters (wird auf allen Clients beim Rauswurf gesetzt).</summary>
    public static byte? ExiledJesterId { get; set; }

    public override bool VerifyCondition(PlayerControl playerControl, NetworkedPlayerInfo[] winners)
    {
        return winners.Length == 1 && winners[0].PlayerId == ExiledJesterId;
    }

    public override void AfterEndGameSetup(EndGameManager endGameManager)
    {
        endGameManager.WinText.text = "Jester gewinnt!";
        endGameManager.WinText.color = HviKColors.Jester;
        endGameManager.BackgroundBar.material.SetColor("_Color", HviKColors.Jester);
    }
}

[HarmonyPatch(typeof(ExileController))]
public static class JesterExilePatch
{
    // Beim Start der Rauswurf-Animation ist die Rolle noch "Jester" (danach wird er zum Geist).
    [HarmonyPostfix]
    [HarmonyPatch(nameof(ExileController.Begin))]
    public static void BeginPostfix(ExileController __instance)
    {
        var exiled = __instance.initData?.networkedPlayer;
        JesterGameOver.ExiledJesterId = exiled != null && exiled.Role is JesterRole ? exiled.PlayerId : null;
    }

    // Nach der Animation beendet der Host das Spiel.
    [HarmonyPrefix]
    [HarmonyPatch(nameof(ExileController.ReEnableGameplay))]
    public static void ReEnableGameplayPrefix()
    {
        if (!AmongUsClient.Instance.AmHost || JesterGameOver.ExiledJesterId is not { } id) return;

        var jester = GameData.Instance.GetPlayerById(id);
        if (jester != null) CustomGameOver.Trigger<JesterGameOver>([jester]);
    }
}

/// <summary>
/// Mira ermittelt die Gewinner ueber die aktuelle Rolle - der Jester ist dann aber schon ein Geist.
/// Deshalb setzen wir die Gewinnerliste bei einem Jester-Sieg selbst.
/// </summary>
[HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnGameEnd))]
[HarmonyAfter("mira.api")]
public static class JesterWinnersPatch
{
    public static void Postfix(EndGameResult endGameResult)
    {
        if (endGameResult.GameOverReason != CustomGameOver.GameOverReason<JesterGameOver>()) return;

        var jester = GameData.Instance.AllPlayers.ToArray().FirstOrDefault(p => p.PlayerId == JesterGameOver.ExiledJesterId);
        EndGameResult.CachedWinners.Clear();
        if (jester != null) EndGameResult.CachedWinners.Add(new CachedPlayerData(jester));
    }
}

/// <summary>Zustand zu Spielbeginn zuruecksetzen.</summary>
[HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.CoBegin))]
public static class JesterResetPatch
{
    public static void Prefix() => JesterGameOver.ExiledJesterId = null;
}
