using HarmonyLib;
using MiraAPI.Hud;
using MiraAPI.Patches;
using TownOfNDev.Buttons.Crewmate;
using TownOfNDev.Roles.Crewmate;
using TownOfNDev.Systems;
using TownOfUs.Modules.Components;

namespace TownOfNDev.Patches;

[HarmonyPatch]
public static class TracerHudPatches
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(HudManagerHelper), nameof(HudManagerHelper.FixedUpdate))]
    public static void HudFixedUpdatePostfix()
    {
        TracerTraceRenderer.UpdateAll();
        TracerSystem.HostMaintenance();
        EnsureDustButtonState();
    }

    private static void EnsureDustButtonState()
    {
        var local = PlayerControl.LocalPlayer;
        if (!local || local.Data == null || !HudManager.InstanceExists)
        {
            return;
        }

        TracerDustButton dustButton;
        try
        {
            dustButton = CustomButtonSingleton<TracerDustButton>.Instance;
        }
        catch
        {
            return;
        }

        var hudAvailable = !MeetingHud.Instance &&
                           (HudManager.Instance.UseButton.isActiveAndEnabled ||
                            HudManager.Instance.PetButton.isActiveAndEnabled);
        var shouldShow = local.Data.Role is TracerRole &&
                         !local.Data.IsDead && !local.Data.Disconnected && hudAvailable;

        if (dustButton.Button == null && shouldShow && HudManagerPatches.BottomRight != null)
        {
            try
            {
                dustButton.CreateButton(HudManagerPatches.BottomRight);
            }
            catch
            {
                return;
            }
        }

        if (dustButton.Button != null)
        {
            dustButton.Button.ToggleVisible(shouldShow);
        }
    }
}
