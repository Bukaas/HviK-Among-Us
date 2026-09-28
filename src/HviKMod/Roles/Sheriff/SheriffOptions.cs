using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;

namespace HviKMod.Roles.Sheriff;

public class SheriffOptions : AbstractRoleOptionGroup<SheriffRole>
{
    public override string GroupName => "Sheriff";

    [ModdedNumberOption("Abklingzeit", 10, 60, 2.5f, MiraNumberSuffixes.Seconds)]
    public float Cooldown { get; set; } = 25f;

    [ModdedNumberOption("Schüsse pro Spiel", 0, 10, 1, MiraNumberSuffixes.None, null, true)]
    public float Shots { get; set; } = 0f;

    [ModdedToggleOption("Fehlschuss tötet auch das Ziel")]
    public bool MisfireKillsTarget { get; set; } = false;

    [ModdedToggleOption("Darf den Jester erschießen")]
    public bool CanShootJester { get; set; } = false;
}
