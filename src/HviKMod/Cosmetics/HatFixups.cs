using System.Collections.Generic;
using HarmonyLib;
using UnityEngine.AddressableAssets;

namespace HviKMod.Cosmetics;

/// <summary>
/// Die Huete aus dem Hut-Paket haben in v18 einen kaputten StoreName (ungueltiger Text im Speicher).
/// MiraAPI sortiert das Hut-Inventar nach StoreName -> Fehler, Blaettern kaputt, auf manchen PCs Absturz.
/// Deshalb setzen wir VOR MiraAPI fuer alle Paket-Huete einen gueltigen Namen - ohne den kaputten zu lesen.
/// </summary>
[HarmonyPatch(typeof(HatsTab), nameof(HatsTab.OnEnable))]
[HarmonyBefore("mira.api")]
[HarmonyPriority(Priority.First)]
public static class HatFixups
{
    public const string ShopGroupName = "HviK Shop";

    private static readonly HashSet<string> ShopHatIds = new();
    private static bool _done;

    /// <summary>Kommt der Hut aus dem Hut-Paket (= kaufbar)?</summary>
    public static bool IsShopHat(string productId) => ShopHatIds.Contains(productId);

    public static void Prefix() => EnsureFixed();

    public static void EnsureFixed()
    {
        if (_done || !HatLoader.Registered) return;

        try
        {
            var locations = Addressables.LoadResourceLocationsAsync(HatLoader.HatsKey).WaitForCompletion();
            var assets = Addressables.LoadAssetsAsync<HatData>(locations, null, false).WaitForCompletion();
            foreach (var hat in new Il2CppSystem.Collections.Generic.List<HatData>(assets.Pointer).ToArray())
            {
                if (!hat) continue;
                hat.StoreName = ShopGroupName;
                ShopHatIds.Add(hat.ProductId);
            }

            _done = true;
            HviKPlugin.Instance.Log.LogInfo($"{ShopHatIds.Count} Shop-Huete vorbereitet.");
        }
        catch (System.Exception e)
        {
            HviKPlugin.Instance.Log.LogWarning($"Shop-Huete konnten nicht vorbereitet werden: {e.Message}");
        }
    }
}
