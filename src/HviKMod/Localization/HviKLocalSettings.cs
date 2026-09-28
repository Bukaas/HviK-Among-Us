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
    }

    public override string TabName => "HviK.Settings.Tab";

    [LocalEnumSetting("HviK.Settings.Language", null, ["Auto", "Deutsch", "English"])]
    public ConfigEntry<ModLanguage> Language { get; private set; }
}

/// <summary>Beim Oeffnen der Lobby-Einstellungen Texte auffrischen (falls die Spielsprache gewechselt wurde).</summary>
[HarmonyPatch(typeof(GameSettingMenu), nameof(GameSettingMenu.Start))]
public static class RefreshTextsPatch
{
    public static void Prefix() => Loc.Apply();
}
