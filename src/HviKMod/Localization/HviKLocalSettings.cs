using BepInEx.Configuration;
using HarmonyLib;
using MiraAPI.LocalSettings;
using MiraAPI.LocalSettings.Attributes;

namespace HviKMod.Localization;

/// <summary>
/// Eigener Tab in den Among-Us-Optionen (nur fuer den eigenen PC, wird nicht an andere gesendet).
/// </summary>
public class HviKLocalSettings : LocalSettingsTab
{
    public HviKLocalSettings(ConfigFile config) : base(config)
    {
        Language = config.Bind("General", "Language", ModLanguage.Auto, "Auto / Deutsch / English");
        Loc.Setting = Language.Value;
        Language.SettingChanged += (_, _) =>
        {
            Loc.Setting = Language.Value;
            Loc.Apply();
        };

        SendLobbyCodeEntry = config.Bind("General", "SendLobbyCode", true,
            "Als Host den Lobby-Code und die Spielerliste an hvik.org / den HviK-Discord senden");
        _sendLobbyCode = SendLobbyCodeEntry;
    }

    private static ConfigEntry<bool>? _sendLobbyCode;

    /// <summary>Darf der LobbyReporter senden? (Standard: ja)</summary>
    public static bool SendLobbyCode => _sendLobbyCode?.Value ?? true;

    public override string TabName => "HviK.Settings.Tab";

    [LocalEnumSetting("HviK.Settings.Language", null, ["Auto", "Deutsch", "English"])]
    public ConfigEntry<ModLanguage> Language { get; private set; }

    [LocalToggleSetting("HviK.Settings.SendLobbyCode")]
    public ConfigEntry<bool> SendLobbyCodeEntry { get; private set; }
}

/// <summary>Beim Oeffnen der Lobby-Einstellungen Texte auffrischen (falls die Spielsprache gewechselt wurde).</summary>
[HarmonyPatch(typeof(GameSettingMenu), nameof(GameSettingMenu.Start))]
public static class RefreshTextsPatch
{
    public static void Prefix() => Loc.Apply();
}
