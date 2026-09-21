using HarmonyLib;
using TownOfNDev.Systems;

namespace TownOfNDev.Patches;

[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Update))]
public static class SoulhandlerMeetingPatches
{
    [HarmonyPostfix]
    public static void MeetingUpdatePostfix(MeetingHud __instance)
    {
        SoulhandlerMeetingRenderer.Apply(__instance);
    }
}
