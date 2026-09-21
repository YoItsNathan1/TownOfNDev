using HarmonyLib;
using TownOfNDev.Systems;

namespace TownOfNDev.Patches;

[HarmonyPatch(typeof(MiraAPI.Utilities.Extensions), nameof(MiraAPI.Utilities.Extensions.ProtectedByGa))]
public static class MiracleProtectionPatch
{
    [HarmonyPostfix]
    public static void ProtectedByGaPostfix(PlayerControl playerControl, ref bool __result)
    {
        if (__result || !playerControl)
        {
            return;
        }

        if (MiracleSystem.ConsumePendingProtectedResult(playerControl))
        {
            __result = true;
        }
    }
}
