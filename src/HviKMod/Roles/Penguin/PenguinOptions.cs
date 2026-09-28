using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;

namespace HviKMod.Roles.Penguin;

public class PenguinOptions : AbstractRoleOptionGroup<PenguinRole>
{
    public override string GroupName => "Penguin";

    [ModdedNumberOption("HviK.Option.Cooldown", 10, 60, 2.5f, MiraNumberSuffixes.Seconds)]
    public float Cooldown { get; set; } = 30f;

    [ModdedNumberOption("HviK.Penguin.DragDuration", 2, 15, 1f, MiraNumberSuffixes.Seconds)]
    public float DragDuration { get; set; } = 6f;
}
