using HarmonyLib;
using MiraAPI.Utilities.Assets;
using UnityEngine;

namespace HviKMod.Patches;

/// <summary>
/// Hauptmenue im HviK-Look: eigener Hintergrund und "HviK Community"-Logo statt "AMONG US".
/// </summary>
[HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.Start))]
public static class MainMenuPatch
{
    public static void Postfix()
    {
        // Ab hier sind alle Einstellungen registriert -> Texte in der gewaehlten Sprache setzen.
        Localization.Loc.Apply();

        // Hintergrund soll die ganze Flaeche abdecken, das Logo komplett sichtbar bleiben.
        ReplaceSprite("MainMenuManager/MainUI/AspectScaler/BackgroundTexture", HviKAssets.MenuBackground, cover: true);
        ReplaceSprite("MainMenuManager/MainUI/AspectScaler/LeftPanel/Sizer/LOGO-AU", HviKAssets.MenuLogo, cover: false);
    }

    private static void ReplaceSprite(string path, LoadableResourceAsset asset, bool cover)
    {
        var obj = GameObject.Find(path);
        var render = obj != null ? obj.GetComponent<SpriteRenderer>() : null;
        if (render == null)
        {
            HviKPlugin.Instance.Log.LogWarning($"{path} nicht gefunden - bleibt Standard.");
            return;
        }

        var oldSize = render.bounds.size;
        render.sprite = asset.LoadAsset();
        render.flipX = false;
        render.flipY = false;
        render.color = Color.white;

        // Neues Bild auf die Groesse des Originals skalieren.
        var newSize = render.bounds.size;
        if (newSize.x <= 0 || newSize.y <= 0) return;
        var fx = oldSize.x / newSize.x;
        var fy = oldSize.y / newSize.y;
        obj!.transform.localScale *= cover ? Mathf.Max(fx, fy) : Mathf.Min(fx, fy);
    }
}
