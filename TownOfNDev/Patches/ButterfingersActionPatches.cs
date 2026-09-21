using HarmonyLib;
using MiraAPI.Hud;
using TownOfNDev.Systems;

namespace TownOfNDev.Patches;

[HarmonyPatch]
public static class ButterfingersActionPatches
{
    [HarmonyPrefix]
    [HarmonyPriority(Priority.Last)]
    [HarmonyPatch(typeof(KillButton), nameof(KillButton.DoClick))]
    public static bool KillButtonPrefix(KillButton __instance)
    {
        return !ButterfingersController.TryFumble(FumbleActionKind.Kill, __instance.DoClick);
    }

    [HarmonyPrefix]
    [HarmonyPriority(Priority.Last)]
    [HarmonyPatch(typeof(ReportButton), nameof(ReportButton.DoClick))]
    public static bool ReportButtonPrefix(ReportButton __instance)
    {
        return !ButterfingersController.TryFumble(FumbleActionKind.Report, __instance.DoClick);
    }

    [HarmonyPrefix]
    [HarmonyPriority(Priority.Last)]
    [HarmonyPatch(typeof(VentButton), nameof(VentButton.DoClick))]
    public static bool VentButtonPrefix(VentButton __instance)
    {
        return !ButterfingersController.TryFumble(FumbleActionKind.Vent, __instance.DoClick);
    }

    [HarmonyPrefix]
    [HarmonyPriority(Priority.Last)]
    [HarmonyPatch(typeof(UseButton), nameof(UseButton.DoClick))]
    public static bool UseButtonPrefix(UseButton __instance)
    {
        return !ButterfingersController.TryFumble(FumbleActionKind.Use, __instance.DoClick);
    }

    [HarmonyPrefix]
    [HarmonyPriority(Priority.Last)]
    [HarmonyPatch(typeof(AbilityButton), nameof(AbilityButton.DoClick))]
    public static bool AbilityButtonPrefix(AbilityButton __instance)
    {
        return !ButterfingersController.TryFumble(FumbleActionKind.Ability, __instance.DoClick);
    }

    [HarmonyPrefix]
    [HarmonyPriority(Priority.Last)]
    [HarmonyPatch(typeof(SecondaryAbilityButton), nameof(SecondaryAbilityButton.DoClick))]
    public static bool SecondaryAbilityButtonPrefix(SecondaryAbilityButton __instance)
    {
        return !ButterfingersController.TryFumble(FumbleActionKind.Ability, __instance.DoClick);
    }

    [HarmonyPrefix]
    [HarmonyPriority(Priority.Last)]
    [HarmonyPatch(typeof(CustomActionButton), nameof(CustomActionButton.ClickHandler))]
    public static bool CustomActionButtonPrefix(CustomActionButton __instance)
    {
        return !ButterfingersController.TryFumble(FumbleActionKind.Ability, __instance.ClickHandler);
    }
}
