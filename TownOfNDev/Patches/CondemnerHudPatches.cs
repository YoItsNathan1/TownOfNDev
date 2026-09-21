using HarmonyLib;
using TownOfNDev.Systems;
using TownOfUs.Modules.Components;

namespace TownOfNDev.Patches;

[HarmonyPatch]
public static class CondemnerHudPatches
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(HudManagerHelper), nameof(HudManagerHelper.FixedUpdate))]
    public static void HudFixedUpdatePostfix()
    {
        CondemnerTargetTrackerRenderer.UpdateAll();
    }
}
