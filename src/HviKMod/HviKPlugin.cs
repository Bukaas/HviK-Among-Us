using BepInEx;
using BepInEx.Configuration;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using MiraAPI;
using MiraAPI.PluginLoading;
using Reactor;
using Reactor.Networking;
using Reactor.Networking.Attributes;

namespace HviKMod;

[BepInAutoPlugin("de.hvik.mod", "HviK Mod")]
[BepInProcess("Among Us.exe")]
[BepInDependency(ReactorPlugin.Id)]
[BepInDependency(MiraApiPlugin.Id)]
[ReactorModFlags(ModFlags.RequireOnAllClients)]
public partial class HviKPlugin : BasePlugin, IMiraPlugin
{
    public static HviKPlugin Instance { get; private set; } = null!;

    public Harmony Harmony { get; } = new(Id);

    public string OptionsTitleText => "HviK\nCommunity";

    public ConfigFile GetConfigFile() => Config;

    public override void Load()
    {
        Instance = this;
        Harmony.PatchAll();
        // Hinweis: Im alten Test-Ordner (.game) stuerzte das Hut-Menue ab, in frischen Installationen nicht.
        Cosmetics.HatLoader.Register();
        Lobby.LobbyReporter.Start();
        Log.LogInfo($"HviK Mod {Version} geladen.");
    }
}
