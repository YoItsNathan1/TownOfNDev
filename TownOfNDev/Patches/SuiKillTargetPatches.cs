using HarmonyLib;
using TownOfNDev.Roles.Crewmate;
using TownOfNDev.Systems;
using UnityEngine;

namespace TownOfNDev.Patches;

[HarmonyPatch]
public static class SuiKillTargetPatches
{
    [HarmonyPrefix]
    [HarmonyPriority(Priority.First)]
    [HarmonyPatch(typeof(KillButton), nameof(KillButton.SetTarget))]
    public static void SuiHuntTargetPrefix(ref PlayerControl target)
    {
        var local = PlayerControl.LocalPlayer;
        if (!local || local.Data == null || local.Data.Role is not SuiRole)
        {
            return;
        }

        target = SuiSystem.GetLocalHuntTargetInRange(local)!;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(KillButton), nameof(KillButton.SetTarget))]
    public static void ProtectedTargetPostfix(KillButton __instance)
    {
        var local = PlayerControl.LocalPlayer;
        if (local && local.Data?.Role is SuiRole)
        {
            return;
        }

        var target = __instance.currentTarget;
        if (!target || !SuiSystem.ShouldSuppressKillTarget(target))
        {
            return;
        }

        if (target.cosmetics != null)
        {
            target.cosmetics.SetOutline(false, new Il2CppSystem.Nullable<Color>(Color.clear));
        }

        __instance.currentTarget = null;
        __instance.SetDisabled();
    }
}
