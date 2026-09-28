using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using BepInEx.Configuration;
using HarmonyLib;
using MiraAPI.Patches.Options;
using MiraAPI.PluginLoading;
using MiraAPI.Presets;
using MiraAPI.Roles;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace HviKMod.Patches;

/// <summary>
/// Ergaenzt im "Presets"-Tab (Lobby-Einstellungen) zwei Buttons:
///   Kopieren  - aktuelle Einstellungen als Text in die Zwischenablage (z.B. zum Teilen im Discord)
///   Einfuegen - Text aus der Zwischenablage als neues Preset speichern und laden
/// Speichern/Laden von Presets selbst bringt MiraAPI schon mit.
/// </summary>
[HarmonyPatch(typeof(GamePresetsTab))]
public static class PresetSharePatch
{
    private const string Prefix = "HVIK-PRESET:";

    private static PassiveButton? _copyButton;
    private static PassiveButton? _pasteButton;

    private static MiraPluginInfo? OurPlugin => MiraPluginManager.GetPluginByGuid(HviKPlugin.Id);

    [HarmonyPostfix]
    [HarmonyPatch(nameof(GamePresetsTab.OnEnable))]
    [HarmonyAfter("mira.api")]
    public static void OnEnablePostfix(GamePresetsTab __instance)
    {
        var saveButton = __instance.transform.Find("SaveButton");
        if (saveButton == null) return;

        // Unity-Objekte koennen "zerstoert" sein ohne null zu sein -> deshalb kein ??=
        if (!_copyButton) _copyButton = CreateButton(saveButton, "HviKCopyPreset", new Vector3(3.2f, 0.5f, -2), CopyPreset);
        if (!_pasteButton) _pasteButton = CreateButton(saveButton, "HviKPastePreset", new Vector3(3.2f, -0.1f, -2), PastePreset);

        var visible = saveButton.gameObject.activeSelf && MenuState.Instance &&
                      MenuState.Instance.CurrentModIdx != 0 && MenuState.Instance.CurrentMod.PluginId == HviKPlugin.Id;
        SetState(_copyButton, "Kopieren", visible);
        SetState(_pasteButton, "Einfügen", visible);
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(GamePresetsTab.OnDisable))]
    public static void OnDisablePostfix()
    {
        if (_copyButton) _copyButton!.gameObject.SetActive(false);
        if (_pasteButton) _pasteButton!.gameObject.SetActive(false);
    }

    private static PassiveButton CreateButton(Transform template, string name, Vector3 position, Action onClick)
    {
        var button = Object.Instantiate(template.gameObject, template.parent).GetComponent<PassiveButton>();
        button.name = name;
        button.transform.localPosition = position;
        button.OnClick = new Button.ButtonClickedEvent();
        button.OnClick.AddListener((UnityAction)onClick);
        return button;
    }

    private static void SetState(PassiveButton? button, string text, bool visible)
    {
        if (!button) return;
        button!.buttonText.text = text;
        button.gameObject.SetActive(visible);
    }

    private static void CopyPreset()
    {
        var plugin = OurPlugin;
        if (plugin == null) return;

        var tempFile = Path.Combine(Path.GetTempPath(), $"hvik-preset-{Guid.NewGuid():N}.cfg");
        try
        {
            var config = new ConfigFile(tempFile, false) { SaveOnConfigSet = false };
            foreach (var option in plugin.Options.Where(o => o.IncludeInPreset)) option.SaveToPreset(config);
            foreach (var role in plugin.Roles.Values.OfType<ICustomRole>().Where(r => !r.Configuration.HideSettings))
            {
                role.SaveToPreset(config);
            }

            config.Save();
            GUIUtility.systemCopyBuffer = Prefix + Compress(File.ReadAllText(tempFile));
            SetState(_copyButton, "Kopiert!", true);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    private static void PastePreset()
    {
        var plugin = OurPlugin;
        if (plugin == null) return;

        var clipboard = GUIUtility.systemCopyBuffer?.Trim() ?? string.Empty;
        if (!clipboard.StartsWith(Prefix, StringComparison.Ordinal))
        {
            SetState(_pasteButton, "Kein Preset!", true);
            return;
        }

        string content;
        try
        {
            content = Decompress(clipboard[Prefix.Length..]);
        }
        catch (Exception e) when (e is FormatException or InvalidDataException)
        {
            SetState(_pasteButton, "Ungültig!", true);
            return;
        }

        var name = $"Eingefügt {DateTime.Now:dd.MM HH-mm-ss}";
        var directory = Path.Combine(PresetManager.PresetDirectory, plugin.PluginId);
        Directory.CreateDirectory(directory);
        File.WriteAllText(Path.Combine(directory, name + ".cfg"), content);

        PresetManager.LoadPresets(plugin);
        plugin.Presets.FirstOrDefault(p => p.Name == name)?.LoadPreset();
        SetState(_pasteButton, "Geladen!", true);

        // Tab neu aufbauen, damit das neue Preset in der Liste erscheint.
        GameSettingMenu.Instance.ChangeTab(0, false);
    }

    private static string Compress(string text)
    {
        using var output = new MemoryStream();
        using (var gzip = new GZipStream(output, System.IO.Compression.CompressionLevel.Optimal))
        {
            var bytes = Encoding.UTF8.GetBytes(text);
            gzip.Write(bytes, 0, bytes.Length);
        }

        return Convert.ToBase64String(output.ToArray());
    }

    private static string Decompress(string base64)
    {
        using var input = new MemoryStream(Convert.FromBase64String(base64));
        using var gzip = new GZipStream(input, CompressionMode.Decompress);
        using var reader = new StreamReader(gzip, Encoding.UTF8);
        return reader.ReadToEnd();
    }
}
