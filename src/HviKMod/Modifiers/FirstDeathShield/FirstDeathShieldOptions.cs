using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;

namespace HviKMod.Modifiers.FirstDeathShield;

public class FirstDeathShieldOptions : AbstractOptionGroup
{
    public override string GroupName => "Died Round 1";
    public override MenuCategory ParentMenu => MenuCategory.Modifiers;
    public override System.Type OptionableType => typeof(FirstDeathShieldModifier);

    [ModdedToggleOption("HviK.FirstDeathShield.Enabled")]
    public bool Enabled { get; set; } = true;
}
