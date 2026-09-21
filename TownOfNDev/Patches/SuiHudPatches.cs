using HarmonyLib;
using MiraAPI.Hud;
using MiraAPI.Patches;
using TownOfNDev.Buttons.Crewmate;
using TownOfNDev.Roles.Crewmate;
using TownOfNDev.Systems;
using TownOfUs.Modules.Components;

namespace TownOfNDev.Patches;

[HarmonyPatch]
public static class SuiHudPatches
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(HudManagerHelper), nameof(HudManagerHelper.FixedUpdate))]
    public static void HudFixedUpdatePostfix()
    {
        SuiOutlineRenderer.UpdateAll();
        SuiSystem.HostMaintenance();
        EnsureProtectButtonState();
        EnsureHuntKillButtonState();
    }

    [HarmonyPostfix]
    [HarmonyPriority(Priority.Last)]
    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
    public static void HudUpdatePostfix()
    {
        EnsureHuntKillButtonState();
    }

    private static void EnsureProtectButtonState()
    {
        var local = PlayerControl.LocalPlayer;
        if (!local || local.Data == null || !HudManager.InstanceExists)
        {
            return;
        }

        SuiProtectButton protectButton;
        try
        {
            protectButton = CustomButtonSingleton<SuiProtectButton>.Instance;
        }
        catch
        {
            return;
        }

        var hudAvailable = !MeetingHud.Instance &&
                           (HudManager.Instance.UseButton.isActiveAndEnabled ||
                            HudManager.Instance.PetButton.isActiveAndEnabled);
        var shouldShow = local.Data.Role is SuiRole &&
                         !local.Data.IsDead && !local.Data.Disconnected && hudAvailable;

        if (protectButton.Button == null && shouldShow && HudManagerPatches.BottomRight != null)
        {
            try
            {
                protectButton.CreateButton(HudManagerPatches.BottomRight);
            }
            catch
            {
                return;
            }
        }

        if (protectButton.Button != null)
        {
            protectButton.Button.ToggleVisible(shouldShow);
        }
    }

    private static void EnsureHuntKillButtonState()
    {
        var local = PlayerControl.LocalPlayer;
        if (!local || local.Data == null || !HudManager.InstanceExists)
        {
            return;
        }

        if (local.Data.Role is not SuiRole)
        {
            return;
        }

        var hudAvailable = !MeetingHud.Instance && !ExileController.Instance &&
                           (HudManager.Instance.UseButton.isActiveAndEnabled ||
                            HudManager.Instance.PetButton.isActiveAndEnabled);
        var huntTarget = SuiSystem.GetLocalHuntTarget(local);
        var shouldShow = !local.Data.IsDead && !local.Data.Disconnected && hudAvailable && huntTarget != null;

        var killButton = HudManager.Instance.KillButton;
        killButton.ToggleVisible(shouldShow);
        if (!shouldShow)
        {
            killButton.SetTarget(null);
            return;
        }

        killButton.SetTarget(SuiSystem.GetLocalHuntTargetInRange(local));
    }
}
