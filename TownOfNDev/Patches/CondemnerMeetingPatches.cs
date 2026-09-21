using HarmonyLib;
using TownOfNDev.Systems;

namespace TownOfNDev.Patches;

[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Update))]
public static class CondemnerMeetingPatches
{
    [HarmonyPostfix]
    public static void MeetingUpdatePostfix(MeetingHud __instance)
    {
        CondemnerMeetingRenderer.Apply(__instance);
    }
}
