using HarmonyLib;
using MiraAPI.Hud;
using MiraAPI.Patches;
using TownOfNDev.Buttons.Neutral;
using TownOfNDev.Roles.Neutral;
using TownOfNDev.Systems;
using TownOfUs.Modules.Components;
using UnityEngine;

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
        EnsureInfectButtonState();
    }

    private static void EnsureInfectButtonState()
    {
        var local = PlayerControl.LocalPlayer;
        if (!local || local.Data == null || !HudManager.InstanceExists)
        {
            return;
        }

        FungiInfectButton infectButton;
        try
        {
            infectButton = CustomButtonSingleton<FungiInfectButton>.Instance;
        }
        catch
        {
            return;
        }

        var isFungi = local.Data.Role is FungiRole || FungiInfectionSystem.IsFungi(local);
        var hudAvailable = !MeetingHud.Instance &&
                           (HudManager.Instance.UseButton.isActiveAndEnabled ||
                            HudManager.Instance.PetButton.isActiveAndEnabled);
        var shouldShow = isFungi && !local.Data.IsDead && !local.Data.Disconnected && hudAvailable;

        if (infectButton.Button == null && shouldShow && HudManagerPatches.BottomRight != null)
        {
            try
            {
                infectButton.CreateButton(HudManagerPatches.BottomRight);
            }
            catch
            {
                return;
            }
        }

        if (infectButton.Button != null)
        {
            infectButton.Button.ToggleVisible(shouldShow);
        }
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
