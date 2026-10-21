using System.Globalization;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using MiraAPI;
using MiraAPI.PluginLoading;
using Reactor;
using Reactor.Networking;
using Reactor.Networking.Attributes;
using Reactor.Utilities;
using TownOfNDev.Modules;
using TownOfNDev.WinConditions;
using TownOfUs;
using TownOfUs.Patches;

namespace TownOfNDev;

[BepInAutoPlugin("ndevstudios.townofndev", "TownOfNDev")]
[BepInProcess("Among Us.exe")]
[BepInDependency(ReactorPlugin.Id)]
[BepInDependency(MiraApiPlugin.Id)]
[BepInDependency(TownOfUsPlugin.Id)]
[ReactorModFlags(ModFlags.RequireOnAllClients)]
public partial class TownOfNDevPlugin : BasePlugin, IMiraPlugin
{
    public static CultureInfo Culture => TownOfUsPlugin.Culture;

    public string OptionsTitleText => "TownOfNDev";

    public static bool IsDevBuild => true;

    public ConfigFile GetConfigFile() => Config;

    public Harmony Harmony { get; } = new(Id);

    public override void Load()
    {
        TownOfNDevLocale.LoadInternalLocale();

        // TOU 1.7.3 exposes an explicit extension registry for custom win
        // conditions. Troll resolves at priority 3, Fungi at priority 4, and
        // both therefore run ahead of TOU's generic neutral condition (5).
        WinConditionRegistry.Register(new TrollWinCondition());
        WinConditionRegistry.Register(new FungiWinCondition());

        // Reactor limits the combined name/version length to 60 characters.
        // The generated +commit SHA exceeds that limit, so use a short display version.
        // PingTracker has a separate colour-aware display patch below.
        ReactorCredits.Register("TownOfNDev", "0.1.0-beta.2.0.0-tou179b1", IsDevBuild,
            location => location == ReactorCredits.Location.MainMenu);
        Harmony.PatchAll();
        Log.LogInfo($"TownOfNDev {Version} loaded.");
    }
}
