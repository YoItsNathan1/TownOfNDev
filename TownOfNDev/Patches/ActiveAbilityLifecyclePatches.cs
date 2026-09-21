using BepInEx.Logging;
using HarmonyLib;
using MiraAPI.Hud;
using MiraAPI.Patches;
using TownOfNDev.Buttons.Crewmate;
using TownOfNDev.Buttons.Impostor;
using TownOfNDev.Buttons.Neutral;
using TownOfNDev.Roles.Crewmate;
using TownOfNDev.Roles.Impostor;
using TownOfNDev.Roles.Neutral;
using TownOfNDev.Systems;
using TownOfUs.Modules.Components;

namespace TownOfNDev.Patches;

[HarmonyPatch]
public static class ActiveAbilityLifecyclePatches
{
    private static readonly ManualLogSource Log =
        BepInEx.Logging.Logger.CreateLogSource("TownOfNDev.AbilityLifecycle");

    private static int _lastRecoveredRound = -1;
    private static byte _lastLocalPlayerId = byte.MaxValue;
    private static System.Type? _lastRoleType;

    [HarmonyPostfix]
    [HarmonyPriority(Priority.Last)]
    [HarmonyPatch(typeof(ExileController), nameof(ExileController.ReEnableGameplay))]
    public static void ReEnableGameplayPostfix()
    {
        // ReEnableGameplay runs while the ExileController object can still exist.
        // State recovery is safe here; only HUD visibility waits until the exile
        // object has actually disappeared.
        if (RecoverCurrentRole("ReEnableGameplay", allowExile: true))
        {
            RememberCurrentRound();
        }
    }

    [HarmonyPostfix]
    [HarmonyPriority(Priority.Last)]
    [HarmonyPatch(typeof(HudManagerHelper), nameof(HudManagerHelper.FixedUpdate))]
    public static void HudFixedUpdatePostfix()
    {
        var local = PlayerControl.LocalPlayer;
        if (!IsGameplayReady(local))
        {
            return;
        }

        var role = local!.Data.Role;
        if (role == null)
        {
            return;
        }

        var round = HudManagerHelper.Instance.CurrentRound;
        var identityChanged = _lastLocalPlayerId != local.PlayerId || _lastRoleType != role.GetType();

        // Mira handles round one normally. From round two onward, recover once
        // per round as a fallback in case another patch ordering skipped the
        // normal ReEnableGameplay recovery path.
        if (round > 1 && (identityChanged || _lastRecoveredRound != round))
        {
            RecoverCurrentRole("round fallback");
            RememberCurrentRound();
        }
        else if (round <= 1 && (identityChanged || _lastRecoveredRound != round))
        {
            RememberCurrentRound();
        }

        EnsureCurrentRoleButton();
    }

    private static bool IsGameplayReady(PlayerControl? local, bool allowExile = false)
    {
        return local != null && local && local.Data != null &&
               !local.Data.Disconnected && !local.Data.IsDead &&
               HudManager.InstanceExists && !LobbyBehaviour.Instance &&
               !MeetingHud.Instance && (allowExile || !ExileController.Instance);
    }

    private static bool RecoverCurrentRole(string source, bool allowExile = false)
    {
        var local = PlayerControl.LocalPlayer;
        if (!IsGameplayReady(local, allowExile))
        {
            return false;
        }

        var recovered = true;
        switch (local!.Data.Role)
        {
            case SuiRole:
                SuiProtectButton.RecoverLocalAfterMeeting();
                break;
            case TracerRole:
                TracerDustButton.RecoverLocalAfterMeeting();
                break;
            case CondemnerRole:
                CondemnerDeathNoteButton.RecoverLocalAfterMeeting();
                break;
            case ReverserRole:
                ReverserAlertButton.RecoverLocalAfterMeeting();
                break;
            case FungiRole:
                FungiInfectButton.RecoverLocalAfterMeeting();
                break;
            default:
                if (FungiInfectionSystem.IsFungi(local))
                {
                    FungiInfectButton.RecoverLocalAfterMeeting();
                }
                else
                {
                    recovered = false;
                }
                break;
        }

        if (recovered)
        {
            Log.LogInfo(
                $"Recovered {local.Data.Role.GetType().Name} active ability for round " +
                $"{HudManagerHelper.Instance.CurrentRound} via {source}.");
        }

        return recovered;
    }

    private static void EnsureCurrentRoleButton()
    {
        var local = PlayerControl.LocalPlayer;
        if (!IsGameplayReady(local) || HudManagerPatches.BottomRight == null)
        {
            return;
        }

        try
        {
            switch (local!.Data.Role)
            {
                case SuiRole:
                    EnsureButton(CustomButtonSingleton<SuiProtectButton>.Instance, local.Data.Role);
                    break;
                case TracerRole:
                    EnsureButton(CustomButtonSingleton<TracerDustButton>.Instance, local.Data.Role);
                    break;
                case CondemnerRole:
                    EnsureButton(CustomButtonSingleton<CondemnerDeathNoteButton>.Instance, local.Data.Role);
                    break;
                case ReverserRole:
                    EnsureButton(CustomButtonSingleton<ReverserAlertButton>.Instance, local.Data.Role);
                    break;
                case FungiRole:
                    EnsureButton(CustomButtonSingleton<FungiInfectButton>.Instance, local.Data.Role);
                    break;
                default:
                    if (FungiInfectionSystem.IsFungi(local))
                    {
                        EnsureButton(CustomButtonSingleton<FungiInfectButton>.Instance, local.Data.Role);
                    }
                    break;
            }
        }
        catch (System.Exception ex)
        {
            Log.LogWarning($"Active ability HUD recovery failed: {ex.Message}");
        }
    }

    private static void EnsureButton(CustomActionButton button, RoleBehaviour role)
    {
        if ((button.Button == null || !button.Button) && HudManagerPatches.BottomRight != null)
        {
            button.CreateButton(HudManagerPatches.BottomRight);
            Log.LogInfo($"Recreated {button.GetType().Name} after HUD transition.");
        }

        if (button.Button != null && button.Button)
        {
            button.SetActive(true, role);
        }
    }

    private static void RememberCurrentRound()
    {
        var local = PlayerControl.LocalPlayer;
        if (local == null || !local || local.Data == null)
        {
            _lastRecoveredRound = -1;
            _lastLocalPlayerId = byte.MaxValue;
            _lastRoleType = null;
            return;
        }

        _lastRecoveredRound = HudManagerHelper.Instance.CurrentRound;
        _lastLocalPlayerId = local.PlayerId;
        _lastRoleType = local.Data.Role?.GetType();
    }
}
