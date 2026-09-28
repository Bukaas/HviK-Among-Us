using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;

namespace HviKMod.Roles.Transporter;

public class TransporterOptions : AbstractRoleOptionGroup<TransporterRole>
{
    public override string GroupName => "Transporter";

    [ModdedNumberOption("HviK.Option.Cooldown", 5, 60, 2.5f, MiraNumberSuffixes.Seconds)]
    public float Cooldown { get; set; } = 20f;

    [ModdedNumberOption("HviK.Transporter.TasksPerCharge", 1, 5)]
    public float TasksPerCharge { get; set; } = 2f;

    [ModdedNumberOption("HviK.Transporter.StartCharges", 0, 5)]
    public float StartCharges { get; set; } = 0f;
}
