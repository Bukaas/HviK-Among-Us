using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.PluginLoading;
using MiraAPI.Utilities;

namespace HviKMod.Modifiers.Guesser;

/// <summary>
/// Einstellungen, die beide Guesser haben. Jede Seite hat ihre eigene Seite im Menue.
/// Hinweis: MiraAPI patcht die Properties jeder Klasse einzeln - deshalb muessen sie in den
/// konkreten Klassen stehen (override) und nicht nur in der Basisklasse.
/// </summary>
[MiraIgnore]
public abstract class GuesserOptions : AbstractOptionGroup
{
    public override MenuCategory ParentMenu => MenuCategory.Modifiers;

    public abstract float Amount { get; set; }
    public abstract float Chance { get; set; }
    public abstract float Guesses { get; set; }
    public abstract bool MultipleGuessesPerMeeting { get; set; }
    public abstract bool WrongGuessKillsGuesser { get; set; }
    public abstract bool CanGuessBasicRoles { get; set; }
}

public class CrewGuesserOptions : GuesserOptions
{
    public override string GroupName => "Guesser (Crew)";
    public override System.Type OptionableType => typeof(CrewGuesserModifier);

    [ModdedNumberOption("HviK.Guesser.Amount", 0, 5)]
    public override float Amount { get; set; } = 0f;

    [ModdedNumberOption("HviK.Guesser.Chance", 0, 100, 10, MiraNumberSuffixes.Percent)]
    public override float Chance { get; set; } = 100f;

    [ModdedNumberOption("HviK.Guesser.Guesses", 1, 15)]
    public override float Guesses { get; set; } = 3f;

    [ModdedToggleOption("HviK.Guesser.MultiplePerMeeting")]
    public override bool MultipleGuessesPerMeeting { get; set; } = true;

    [ModdedToggleOption("HviK.Guesser.WrongGuessKills")]
    public override bool WrongGuessKillsGuesser { get; set; } = true;

    [ModdedToggleOption("HviK.Guesser.CanGuessBasicRoles")]
    public override bool CanGuessBasicRoles { get; set; } = true;
}

public class ImpostorGuesserOptions : GuesserOptions
{
    public override string GroupName => "Guesser (Impostor)";
    public override System.Type OptionableType => typeof(ImpostorGuesserModifier);

    [ModdedNumberOption("HviK.Guesser.Amount", 0, 5)]
    public override float Amount { get; set; } = 0f;

    [ModdedNumberOption("HviK.Guesser.Chance", 0, 100, 10, MiraNumberSuffixes.Percent)]
    public override float Chance { get; set; } = 100f;

    [ModdedNumberOption("HviK.Guesser.Guesses", 1, 15)]
    public override float Guesses { get; set; } = 3f;

    [ModdedToggleOption("HviK.Guesser.MultiplePerMeeting")]
    public override bool MultipleGuessesPerMeeting { get; set; } = true;

    [ModdedToggleOption("HviK.Guesser.WrongGuessKills")]
    public override bool WrongGuessKillsGuesser { get; set; } = true;

    [ModdedToggleOption("HviK.Guesser.CanGuessBasicRoles")]
    public override bool CanGuessBasicRoles { get; set; } = true;
}
