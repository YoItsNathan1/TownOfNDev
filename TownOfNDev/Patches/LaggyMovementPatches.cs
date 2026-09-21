using HarmonyLib;
using TownOfNDev.Systems;

namespace TownOfNDev.Patches;

[HarmonyPatch]
public static class LaggyMovementPatches
{
    // Do not suppress PlayerPhysics.FixedUpdate. Among Us performs more than raw movement there,
    // and skipping the whole method can interfere with local visual state such as the living-player
    // vision/shadow mask. Laggy now observes movement after vanilla/TOU physics has run and applies
    // its short freeze through PlayerControl.moveable instead.
    [HarmonyPostfix]
    [HarmonyPriority(Priority.Last)]
    [HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.FixedUpdate))]
    public static void PlayerPhysicsFixedUpdatePostfix(PlayerPhysics __instance)
    {
        LaggyController.Tick(__instance);
    }
}
