using System.Collections.Generic;
using MiraAPI.Translation;

namespace HviKMod.Localization;

public enum ModLanguage
{
    Auto,
    Deutsch,
    English,
}

/// <summary>
/// Zweisprachige Texte (Deutsch / Englisch). Die Sprache stellt jeder Spieler selbst ein
/// (Optionen -> Tab "HviK"). "Auto" folgt der Sprache von Among Us.
/// </summary>
public static class Loc
{
    public static ModLanguage Setting { get; set; } = ModLanguage.Auto;

    public static bool IsGerman => Setting switch
    {
        ModLanguage.Deutsch => true,
        ModLanguage.English => false,
        _ => MiraLocaleManager.CurrentLanguage == MiraLanguage.German,
    };

    /// <summary>Text in der gewaehlten Sprache.</summary>
    public static string T(string de, string en) => IsGerman ? de : en;

    // ---------------------------------------------------------------------------------------
    // Texte, die MiraAPI ueber Schluessel nachschlaegt (Titel der Lobby-Einstellungen).
    // Schluessel nie aendern - sie stehen auch in gespeicherten Presets.
    // ---------------------------------------------------------------------------------------
    private static readonly Dictionary<string, (string De, string En)> Keys = new()
    {
        ["HviK.Option.Cooldown"] = ("Abklingzeit", "Cooldown"),

        ["HviK.Sheriff.Shots"] = ("Schüsse pro Spiel", "Shots per game"),
        ["HviK.Sheriff.MisfireKillsTarget"] = ("Fehlschuss tötet auch das Ziel", "Misfire also kills the target"),
        ["HviK.Sheriff.CanShootJester"] = ("Darf den Jester erschießen", "Can shoot the Jester"),

        ["HviK.Transporter.TasksPerCharge"] = ("Aufgaben pro Ladung", "Tasks per charge"),
        ["HviK.Transporter.StartCharges"] = ("Ladungen zu Spielbeginn", "Charges at game start"),

        ["HviK.Puppeteer.ControlDuration"] = ("Kontrolldauer", "Control duration"),
        ["HviK.Puppeteer.PuppetCanKillImpostors"] = ("Puppe kann Impostor töten", "Puppet can kill Impostors"),

        ["HviK.Penguin.DragDuration"] = ("Ziehdauer", "Drag duration"),

        ["HviK.Jester.CanUseVents"] = ("Kann Vents benutzen", "Can use vents"),

        ["HviK.Guesser.Amount"] = ("Anzahl", "Amount"),
        ["HviK.Guesser.Chance"] = ("Wahrscheinlichkeit", "Chance"),
        ["HviK.Guesser.Guesses"] = ("Tipps pro Spiel", "Guesses per game"),
        ["HviK.Guesser.MultiplePerMeeting"] = ("Mehrere Tipps pro Meeting", "Multiple guesses per meeting"),
        ["HviK.Guesser.WrongGuessKills"] = ("Falscher Tipp tötet den Guesser", "Wrong guess kills the Guesser"),
        ["HviK.Guesser.CanGuessBasicRoles"] = ("Kann Crewmate/Impostor raten", "Can guess Crewmate/Impostor"),

        ["HviK.Settings.Tab"] = ("HviK", "HviK"),
        ["HviK.Settings.Language"] = ("Sprache der Mod", "Mod language"),
    };

    /// <summary>
    /// Schreibt alle Schluessel-Texte in MiraAPIs Uebersetzungstabelle - fuer jede Spielsprache,
    /// damit unsere Einstellung Vorrang vor der Among-Us-Sprache hat.
    /// </summary>
    public static void Apply()
    {
        foreach (var language in MiraLocaleManager.LangList.Keys)
        {
            if (!MiraLocaleManager.Locale.TryGetValue(language, out var table))
            {
                table = new Dictionary<string, string>();
                MiraLocaleManager.Locale[language] = table;
            }

            foreach (var (key, text) in Keys)
            {
                table[key] = IsGerman ? text.De : text.En;
            }
        }
    }
}
