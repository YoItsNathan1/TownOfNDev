using HarmonyLib;
using TownOfNDev.Systems;
using TownOfUs.Modules.Components;

namespace TownOfNDev.Patches;

[HarmonyPatch]
public static class TracerHudPatches
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(HudManagerHelper), nameof(HudManagerHelper.FixedUpdate))]
    public static void HudFixedUpdatePostfix()
    {
        TracerTraceRenderer.UpdateAll();
        TracerSystem.HostMaintenance();
    }
}
