using HviKMod.Localization;
using HviKMod.Roles;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers.Types;
using MiraAPI.PluginLoading;
using MiraAPI.Roles;
using MiraAPI.Utilities.Assets;
using UnityEngine;

namespace HviKMod.Modifiers.Guesser;

/// <summary>
/// Gemeinsame Logik fuer beide Guesser. Der Modifier kommt zusaetzlich zur normalen Rolle.
/// Im Meeting erscheint neben jedem Spieler ein "Raten"-Button: richtige Rolle = Ziel stirbt,
/// falsche Rolle = man stirbt selbst (einstellbar).
/// </summary>
[MiraIgnore]
public abstract class GuesserModifier : GameModifier
{
    public abstract GuesserOptions Options { get; }

    /// <summary>Verbleibende Tipps (nur auf dem eigenen Client relevant).</summary>
    public int GuessesLeft { get; set; }

    public bool GuessedThisMeeting { get; set; }

    public override LoadableAsset<Sprite>? ModifierIcon => HviKAssets.GuessButton;

    public override int GetAssignmentChance() => (int)Options.Chance;

    public override int GetAmountPerGame() => (int)Options.Amount;

    public override void OnActivate()
    {
        GuessesLeft = (int)Options.Guesses;
    }

    public override void OnMeetingStart()
    {
        GuessedThisMeeting = false;
    }

    public bool CanGuessNow => !Player.Data.IsDead && GuessesLeft > 0 && (Options.MultipleGuessesPerMeeting || !GuessedThisMeeting);

    /// <summary>Darf diese Rolle im Menue angeboten werden? (eigenes Team wird nicht angeboten)</summary>
    public abstract bool CanGuessTeamOf(RoleBehaviour role);
}

public class CrewGuesserModifier : GuesserModifier
{
    public override GuesserOptions Options => OptionGroupSingleton<CrewGuesserOptions>.Instance;

    public override string ModifierName => Loc.T("Guesser (Crew)", "Guesser (Crew)");

    public override string GetDescription() => Loc.T(
        "Errate im Meeting die Rolle eines Spielers, um ihn zu töten.",
        "Guess a player's role in a meeting to kill them.");

    public override bool IsModifierValidOn(RoleBehaviour role) => RoleTeams.IsCrew(role);

    public override bool CanGuessTeamOf(RoleBehaviour role) => !RoleTeams.IsCrew(role);
}

public class ImpostorGuesserModifier : GuesserModifier
{
    public override GuesserOptions Options => OptionGroupSingleton<ImpostorGuesserOptions>.Instance;

    public override string ModifierName => Loc.T("Guesser (Impostor)", "Guesser (Impostor)");

    public override string GetDescription() => Loc.T(
        "Errate im Meeting die Rolle eines Spielers, um ihn zu töten.",
        "Guess a player's role in a meeting to kill them.");

    public override bool IsModifierValidOn(RoleBehaviour role) => role.IsImpostor;

    public override bool CanGuessTeamOf(RoleBehaviour role) => !role.IsImpostor;
}

public static class RoleTeams
{
    public static bool IsCrew(RoleBehaviour role) => role is ICustomRole custom
        ? custom.Team == ModdedRoleTeams.Crewmate
        : !role.IsImpostor;
}
