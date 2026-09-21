using AmongUs.GameOptions;
using HarmonyLib;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using TownOfNDev.Modifiers.Impostor;
using TownOfNDev.Options;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfNDev.Patches;

[HarmonyPatch(typeof(TownOfUs.Utilities.Extensions), nameof(TownOfUs.Utilities.Extensions.GetKillCooldown))]
public static class MenaceKillCooldownPatch
{
    [HarmonyPostfix]
    public static void GetKillCooldownPostfix(PlayerControl player, ref float __result)
    {
        if (!player || !player.HasModifier<MenaceModifier>())
        {
            return;
        }

        var reductionPercent = OptionGroupSingleton<MenaceOptions>.Instance.KillCooldownReduction.Value;
        var multiplier = 1f - Mathf.Clamp(reductionPercent, 0f, 100f) / 100f;

        // TOU itself treats 5 seconds as the safe lower bound for its effective
        // kill cooldown. Keep Menace inside the same invariant after applying the
        // percentage reduction to the already-resolved TOU cooldown.
        __result = Mathf.Clamp(__result * multiplier, 5f, 120f);
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.SetKillTimer))]
public static class MenaceSetKillTimerPatch
{
    [HarmonyPrefix]
    [HarmonyPriority(Priority.First)]
    public static void SetKillTimerPrefix(PlayerControl __instance, ref float time)
    {
        if (!__instance || __instance.Data?.Role?.CanUseKillButton != true ||
            !__instance.HasModifier<MenaceModifier>())
        {
            return;
        }

        // GetKillCooldown already includes the Menace postfix above, plus TOU's own
        // cooldown modifiers and map adjustment. This is the value the player should
        // actually receive after a normal kill reset.
        var reducedCooldown = __instance.GetKillCooldown();
        var lobbyCooldown = GameOptionsManager.Instance.CurrentGameOptions
            .GetFloat(FloatOptionNames.KillCooldown);

        // The route that escaped the GetKillCooldown postfix is the normal post-kill
        // reset passing the lobby's raw KillCooldown directly to SetKillTimer. Rewrite
        // only that exact full-reset value. Calls that already pass GetKillCooldown are
        // already reduced, while special short timers (0, intro delays, abilities, etc.)
        // remain untouched.
        if (Mathf.Abs(time - lobbyCooldown) < 0.05f)
        {
            time = reducedCooldown;
        }
    }
}
