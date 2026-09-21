using HarmonyLib;
using MiraAPI.Hud;
using TownOfNDev.Systems;

namespace TownOfNDev.Patches;

[HarmonyPatch]
public static class EyeMaskActionLockPatches
{
    private static bool CanAct() => !EyeMaskSleepController.IsSleeping;

    [HarmonyPrefix]
    [HarmonyPatch(typeof(ReportButton), nameof(ReportButton.DoClick))]
    [HarmonyPatch(typeof(UseButton), nameof(UseButton.DoClick))]
    [HarmonyPatch(typeof(KillButton), nameof(KillButton.DoClick))]
    [HarmonyPatch(typeof(VentButton), nameof(VentButton.DoClick))]
    [HarmonyPatch(typeof(AbilityButton), nameof(AbilityButton.DoClick))]
    [HarmonyPatch(typeof(SecondaryAbilityButton), nameof(SecondaryAbilityButton.DoClick))]
    public static bool VanillaButtons() => CanAct();

    [HarmonyPrefix]
    [HarmonyPatch(typeof(CustomActionButton), nameof(CustomActionButton.ClickHandler))]
    public static bool MiraButtons() => CanAct();
}
