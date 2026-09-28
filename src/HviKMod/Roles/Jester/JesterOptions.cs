using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;

namespace HviKMod.Roles.Jester;

public class JesterOptions : AbstractRoleOptionGroup<JesterRole>
{
    public override string GroupName => "Jester";

    [ModdedToggleOption("Kann Vents benutzen")]
    public bool CanUseVents { get; set; } = false;
}
