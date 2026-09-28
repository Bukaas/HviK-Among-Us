using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;

namespace HviKMod.Roles.Puppeteer;

public class PuppeteerOptions : AbstractRoleOptionGroup<PuppeteerRole>
{
    public override string GroupName => "Puppeteer";

    [ModdedNumberOption("HviK.Option.Cooldown", 10, 60, 2.5f, MiraNumberSuffixes.Seconds)]
    public float Cooldown { get; set; } = 30f;

    [ModdedNumberOption("HviK.Puppeteer.ControlDuration", 5, 60, 5f, MiraNumberSuffixes.Seconds)]
    public float ControlDuration { get; set; } = 20f;

    [ModdedToggleOption("HviK.Puppeteer.PuppetCanKillImpostors")]
    public bool PuppetCanKillImpostors { get; set; } = false;
}
