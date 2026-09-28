using System.Collections.Generic;
using AmongUs.Data;
using HarmonyLib;
using HviKMod.Localization;
using TMPro;
using UnityEngine;

namespace HviKMod.Cosmetics;

/// <summary>
/// Hut-Shop im normalen Hut-Inventar: Huete aus dem Hut-Paket sind erst nach dem Kauf anziehbar.
/// Nicht gekaufte Huete sind abgedunkelt; erster Klick auf "Anziehen" fragt, zweiter Klick kauft.
/// Oben im Tab steht der Kontostand (Minispiel-Coins aus Discord).
/// </summary>
[HarmonyPatch(typeof(HatsTab))]
public static class HatShopPatches
{
    private const string NoHat = "hat_NoHat";
    private const float ConfirmSeconds = 5f;

    private static string? _pendingHat;
    private static float _pendingSince;
    private static string? _equipAfterBuy;
    private static bool _buying;
    private static TextMeshPro? _coinText;
    private static readonly Dictionary<int, bool> DimmedChips = new();

    /// <summary>Kommt der Hut aus unserem Hut-Paket? (Liste aus HatFixups - StoreName ist bei Paket-Hueten kaputt)</summary>
    public static bool IsShopHat(HatData? hat) => hat != null && HatFixups.IsShopHat(hat.ProductId);

    private static bool IsLocked(HatData? hat) => IsShopHat(hat) && !ShopClient.Owns(hat!.ProductId);

    [HarmonyPrefix]
    [HarmonyPatch(nameof(HatsTab.ClickEquip))]
    public static bool ClickEquipPrefix(HatsTab __instance)
    {
        var hat = __instance.currentHat;
        if (!IsLocked(hat) || _buying) return !_buying;

        if (!ShopClient.IsConnected)
        {
            ShowMessage(Loc.T(
                "Shop-Code fehlt: /amongus shopcode in Discord, dann Einstellungen → HviK",
                "Shop code missing: /amongus shopcode in Discord, then Settings → HviK"));
            return false;
        }

        if (_pendingHat != hat.ProductId || Time.realtimeSinceStartup - _pendingSince > ConfirmSeconds)
        {
            _pendingHat = hat.ProductId;
            _pendingSince = Time.realtimeSinceStartup;
            ShowMessage(Loc.T(
                $"Nochmal klicken zum Kaufen: {ShopClient.HatPrice} Coins",
                $"Click again to buy: {ShopClient.HatPrice} coins"));
            return false;
        }

        _pendingHat = null;
        _buying = true;
        var hatId = hat.ProductId;
        ShowMessage(Loc.T("Kaufe...", "Buying..."));
        ShopClient.BuyAsync(hatId).ContinueWith(t =>
        {
            if (t.Result) _equipAfterBuy = hatId; // Anziehen passiert im Update (Unity-Hauptthread)
            _buying = false;
        });
        return false;
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(HatsTab.Update))]
    public static void UpdatePostfix(HatsTab __instance)
    {
        // Nach erfolgreichem Kauf den Hut direkt anziehen
        if (_equipAfterBuy != null && ShopClient.Owns(_equipAfterBuy))
        {
            var bought = HatManager.Instance.GetHatById(_equipAfterBuy);
            _equipAfterBuy = null;
            if (bought != null)
            {
                __instance.SelectHat(bought);
                __instance.ClickEquip();
            }
        }

        UpdateCoinText(__instance);
        DimLockedChips(__instance);
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(HatsTab.OnEnable))]
    public static void OnEnablePostfix()
    {
        DimmedChips.Clear();
        _ = ShopClient.RefreshAsync();
        RemoveUnownedEquippedHat();
    }

    /// <summary>Traegt jemand einen nicht gekauften Shop-Hut (z.B. anderer Account/Code), wird er abgenommen.</summary>
    public static void RemoveUnownedEquippedHat()
    {
        if (!ShopClient.IsConnected && ShopClient.HasShopCode) return; // Stand noch unbekannt
        var equipped = HatManager.Instance ? HatManager.Instance.GetHatById(DataManager.Player.Customization.Hat) : null;
        if (IsLocked(equipped)) DataManager.Player.Customization.Hat = NoHat;
    }

    private static void UpdateCoinText(HatsTab tab)
    {
        if (!_coinText)
        {
            var template = tab.GetComponentInChildren<TextMeshPro>(true);
            if (!template) return;
            _coinText = Object.Instantiate(template, tab.transform);
            _coinText.name = "HviKCoins";
            var translator = _coinText.GetComponent<TextTranslatorTMP>();
            if (translator) Object.Destroy(translator);
            _coinText.alignment = TextAlignmentOptions.Right;
            _coinText.fontSize = _coinText.fontSizeMax = 2.2f;
            _coinText.fontSizeMin = 1f;
            _coinText.transform.localPosition = new Vector3(3.9f, 2.55f, -5f);
            _coinText.gameObject.SetActive(true);
        }

        var coins = ShopClient.IsConnected
            ? Loc.T($"HviK-Coins: <b>{ShopClient.Coins}</b>", $"HviK coins: <b>{ShopClient.Coins}</b>")
            : Loc.T("HviK-Shop: nicht verbunden", "HviK shop: not connected");
        var message = ShopClient.LastMessage ?? _message;
        _coinText!.text = message == null ? coins : $"{coins}\n<size=75%>{message}</size>";
    }


    private static string? _message;

    private static void ShowMessage(string text)
    {
        _message = text;
        ShopClient.LastMessage = null; // alte Server-Meldung soll den neuen Hinweis nicht ueberdecken
    }

    private static void DimLockedChips(HatsTab tab)
    {
        foreach (var chip in tab.ColorChips)
        {
            if (!chip) continue;
            var hat = chip.Tag?.TryCast<HatData>();
            var locked = IsLocked(hat);
            var id = chip.GetInstanceID();
            if (DimmedChips.TryGetValue(id, out var wasLocked) && wasLocked == locked) continue;

            DimmedChips[id] = locked;
            var tint = locked ? new Color(0.35f, 0.35f, 0.35f, 1f) : Color.white;
            foreach (var renderer in chip.Inner.GetComponentsInChildren<SpriteRenderer>(true))
            {
                renderer.color = tint;
            }
        }
    }
}
