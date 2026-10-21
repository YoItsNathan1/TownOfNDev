using HarmonyLib;
using TownOfNDev.Systems;
using TownOfUs.Modules.Components;

namespace TownOfNDev.Patches;

[HarmonyPatch]
public static class FungiHudPatches
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(HudManagerHelper), nameof(HudManagerHelper.FixedUpdate))]
    public static void HudFixedUpdatePostfix()
    {
        FungiGrowthRenderer.UpdateAll();
        FungiInfectionSystem.HostUpdateObjective();
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(HudManagerHelper), nameof(HudManagerHelper.UpdateRoleNameText))]
    public static void RoleNameTextPostfix()
    {
        var local = PlayerControl.LocalPlayer;
        if (!local || !FungiInfectionSystem.IsFungi(local))
        {
            return;
        }

        foreach (var player in PlayerControl.AllPlayerControls)
        {
            if (!player || player.Data == null || player.Data.Disconnected || !FungiInfectionSystem.IsInfected(player))
            {
                continue;
            }

            var text = player.cosmetics.nameText;
            if (!text)
            {
                continue;
            }

            var color = TownOfNDevColors.Fungi;
            color.a = text.color.a;
            text.color = color;
        }
    }
}
