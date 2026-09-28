using System.Collections;
using HviKMod.Localization;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Modifiers;
using MiraAPI.Modifiers.Types;
using MiraAPI.Utilities;
using Reactor.Utilities;
using UnityEngine;

namespace HviKMod.Modifiers.Bait;

/// <summary>Wer den Bait toetet, meldet die Leiche automatisch selbst.</summary>
public class BaitModifier : GameModifier
{
    public override string ModifierName => "Bait";

    public override string GetDescription() => Loc.T(
        "Wer dich tötet, meldet deine Leiche sofort selbst.",
        "Whoever kills you reports your body immediately.");

    public override int GetAssignmentChance() => (int)OptionGroupSingleton<BaitOptions>.Instance.Chance;
    public override int GetAmountPerGame() => (int)OptionGroupSingleton<BaitOptions>.Instance.Amount;
    public override bool IsModifierValidOn(RoleBehaviour role) => !role.IsImpostor;
}

public class BaitOptions : AbstractOptionGroup
{
    public override string GroupName => "Bait";
    public override MenuCategory ParentMenu => MenuCategory.Modifiers;
    public override System.Type OptionableType => typeof(BaitModifier);

    [ModdedNumberOption("HviK.Guesser.Amount", 0, 5)]
    public float Amount { get; set; } = 0f;

    [ModdedNumberOption("HviK.Guesser.Chance", 0, 100, 10, MiraNumberSuffixes.Percent)]
    public float Chance { get; set; } = 100f;
}

public static class BaitEvents
{
    [RegisterEvent]
    public static void OnMurder(AfterMurderEvent @event)
    {
        var killer = @event.Source;
        var bait = @event.Target;
        if (!killer.AmOwner || killer == bait || !bait.HasModifier<BaitModifier>() || MeetingHud.Instance) return;

        Coroutines.Start(CoSelfReport(killer, bait));
    }

    private static IEnumerator CoSelfReport(PlayerControl killer, PlayerControl bait)
    {
        yield return new WaitForSeconds(0.4f);
        if (killer && !killer.Data.IsDead && !MeetingHud.Instance) killer.CmdReportDeadBody(bait.Data);
    }
}
