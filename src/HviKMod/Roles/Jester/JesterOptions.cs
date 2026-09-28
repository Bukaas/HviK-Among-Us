using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;

namespace HviKMod.Roles.Jester;

public class JesterOptions : AbstractRoleOptionGroup<JesterRole>
{
    public override string GroupName => "Jester";

    [ModdedToggleOption("HviK.Jester.CanUseVents")]
    public bool CanUseVents { get; set; } = false;
}
