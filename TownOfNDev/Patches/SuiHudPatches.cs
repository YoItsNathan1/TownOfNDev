using HarmonyLib;
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
        EnsureHuntKillButtonState();
    }

    [HarmonyPostfix]
    [HarmonyPriority(Priority.Last)]
    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
    public static void HudUpdatePostfix()
    {
        EnsureHuntKillButtonState();
    }

    // SUI's temporary hunt uses the vanilla Kill button, so this remains NDev-owned.
    // The Protect custom button itself is now left entirely to Mira/TOU.
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
