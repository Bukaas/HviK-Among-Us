using System.IO;
using System.Reflection;
using MiraAPI.Utilities.Assets;

namespace HviKMod.Cosmetics;

/// <summary>
/// Laedt das Hut-Paket (touhats.catalog/.bundle), das beim Bauen neben unsere DLL gelegt wird
/// (siehe scripts/common.ps1 -> Install-Hats). MiraAPI fuegt die Huete dann ins Inventar ein.
/// Muss waehrend Load() passieren - MiraAPI laedt die Pakete beim Spielstart.
/// </summary>
public static class HatLoader
{
    public const string CatalogFile = "touhats.catalog";
    public const string HatsKey = "touhats";

    public static bool Registered { get; private set; }

    public static bool Register()
    {
        var dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
        var catalog = Path.Combine(dir, CatalogFile);
        if (!File.Exists(catalog))
        {
            HviKPlugin.Instance.Log.LogWarning($"{CatalogFile} nicht gefunden - keine zusaetzlichen Huete.");
            return false;
        }

        AddressablesLoader.RegisterCatalog(catalog);
        AddressablesLoader.RegisterHats(HatsKey);
        Registered = true;
        return true;
    }
}
