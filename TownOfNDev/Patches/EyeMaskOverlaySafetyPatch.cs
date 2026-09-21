using HarmonyLib;
using TownOfNDev.Systems;

namespace TownOfNDev.Patches;

[HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
public static class EyeMaskOverlaySafetyPatch
{
    [HarmonyPostfix]
    public static void Postfix()
    {
        EyeMaskSleepController.SafetyTick();
    }
}
