using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using BepInEx.Configuration;
using HviKMod.Lobby;

namespace HviKMod.Cosmetics;

/// <summary>
/// Verbindung zum Hut-Shop auf hvik.org (web/among_us_shop.py im Bot-Projekt).
/// Bezahlt wird mit den Minispiel-Coins aus Discord; erkannt wird der Spieler ueber seinen
/// persoenlichen Shop-Code (/amongus shopcode), der lokal in der BepInEx-Config liegt.
/// </summary>
public static class ShopClient
{
    private const string BaseUrl = "https://hvik.org/api/among-us/shop";
    private static readonly HttpClient Http = new() { Timeout = TimeSpan.FromSeconds(10) };

    private static ConfigEntry<string>? _shopCode;

    public static int HatPrice { get; private set; } = 5;
    public static int? Coins { get; private set; }
    public static string? DiscordName { get; private set; }
    public static HashSet<string> OwnedHats { get; } = new();

    /// <summary>true, sobald der Kontostand einmal erfolgreich geladen wurde.</summary>
    public static bool IsConnected => Coins.HasValue;

    /// <summary>Letzte Meldung fuer die Anzeige (Fehler, "Gekauft!" ...).</summary>
    public static string? LastMessage { get; internal set; }

    public static bool HasShopCode => !string.IsNullOrWhiteSpace(_shopCode?.Value);

    public static void Init(ConfigFile config)
    {
        _shopCode = config.Bind("Shop", "ShopCode", "", "Persoenlicher Shop-Code aus Discord (/amongus shopcode)");
        _ = RefreshAsync();
    }

    public static void SetShopCode(string code)
    {
        if (_shopCode == null) return;
        _shopCode.Value = code.Trim().ToUpperInvariant();
        Coins = null;
        OwnedHats.Clear();
        _ = RefreshAsync();
    }

    public static bool Owns(string hatId) => OwnedHats.Contains(hatId);

    public static async Task RefreshAsync()
    {
        var result = await SendAsync(HttpMethod.Get, "/me", null);
        if (result != null) Apply(result.Value);
    }

    /// <summary>Kauft einen Hut. Gibt true zurueck, wenn er danach dem Spieler gehoert.</summary>
    public static async Task<bool> BuyAsync(string hatId)
    {
        var body = JsonSerializer.Serialize(new Dictionary<string, string> { ["hatId"] = hatId });
        var result = await SendAsync(HttpMethod.Post, "/buy", body);
        if (result == null) return false;
        Apply(result.Value);
        return Owns(hatId);
    }

    private static void Apply(JsonElement json)
    {
        if (json.TryGetProperty("coins", out var coins)) Coins = coins.GetInt32();
        if (json.TryGetProperty("price", out var price)) HatPrice = price.GetInt32();
        if (json.TryGetProperty("name", out var name) && name.ValueKind == JsonValueKind.String) DiscordName = name.GetString();
        if (json.TryGetProperty("owned", out var owned))
        {
            OwnedHats.Clear();
            foreach (var hat in owned.EnumerateArray()) OwnedHats.Add(hat.GetString() ?? "");
        }
        if (json.TryGetProperty("message", out var message)) LastMessage = message.GetString();
    }

    private static async Task<JsonElement?> SendAsync(HttpMethod method, string path, string? body)
    {
        var apiKey = LobbyReporter.ApiKey;
        if (apiKey == null || !HasShopCode)
        {
            LastMessage = null;
            return null;
        }

        try
        {
            using var request = new HttpRequestMessage(method, BaseUrl + path);
            request.Headers.Add("X-HviK-Key", apiKey);
            request.Headers.Add("X-HviK-Shop-Token", _shopCode!.Value);
            if (body != null) request.Content = new StringContent(body, Encoding.UTF8, "application/json");

            using var response = await Http.SendAsync(request);
            var text = await response.Content.ReadAsStringAsync();
            var json = JsonDocument.Parse(text).RootElement.Clone();

            if ((int)response.StatusCode == 403)
            {
                Coins = null;
                LastMessage = "Shop-Code ungueltig - neuen mit /amongus shopcode holen";
                return null;
            }

            // 402 = zu wenig Coins: Antwort enthaelt trotzdem den aktuellen Stand
            return response.IsSuccessStatusCode || (int)response.StatusCode == 402 ? json : null;
        }
        catch (Exception e)
        {
            LastMessage = "hvik.org nicht erreichbar";
            HviKPlugin.Instance.Log.LogWarning($"Shop: {e.GetType().Name}");
            return null;
        }
    }
}
