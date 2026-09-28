using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;

namespace HviKMod.Roles.Sheriff;

public class SheriffOptions : AbstractRoleOptionGroup<SheriffRole>
{
    public override string GroupName => "Sheriff";

    [ModdedNumberOption("HviK.Option.Cooldown", 10, 60, 2.5f, MiraNumberSuffixes.Seconds)]
    public float Cooldown { get; set; } = 25f;

    [ModdedNumberOption("HviK.Sheriff.Shots", 0, 10, 1, MiraNumberSuffixes.None, null, true)]
    public float Shots { get; set; } = 0f;

    [ModdedToggleOption("HviK.Sheriff.MisfireKillsTarget")]
    public bool MisfireKillsTarget { get; set; } = false;

    [ModdedToggleOption("HviK.Sheriff.CanShootJester")]
    public bool CanShootJester { get; set; } = false;
}
