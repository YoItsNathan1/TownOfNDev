using System.Linq;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Utilities;
using Reactor.Networking.Attributes;
using TownOfNDev.Assets;
using TownOfNDev.Buttons.Crewmate;
using TownOfNDev.Modifiers.Crewmate;
using TownOfNDev.Modifiers.Internal;
using TownOfNDev.Networking;
using TownOfNDev.Options;
using TownOfNDev.Roles.Crewmate;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfNDev.Systems;

public static class SuiSystem
{
    private static byte? LocalHuntTargetId;

    public static void BeginGame()
    {
        SuiOutlineRenderer.ClearAll();
        ClearLocalHuntState();

        if (AmongUsClient.Instance == null || !AmongUsClient.Instance.AmHost)
        {
            return;
        }

        HostClearAllState();
    }

    public static bool IsActiveSui(byte playerId)
    {
        var player = FindPlayer(playerId);
        return IsValidLiving(player) && player!.Data.Role is SuiRole;
    }

    public static bool IsProtected(PlayerControl? player)
    {
        return player is not null && player &&
               player.GetModifiers<SuiProtectedModifier>().Any(x => IsActiveSui(x.SuiId));
    }

    public static bool IsProtectedBy(PlayerControl? player, byte suiId)
    {
        return player is not null && player && IsActiveSui(suiId) &&
               player.GetModifiers<SuiProtectedModifier>().Any(x => x.SuiId == suiId);
    }

    public static bool IsProtectionTriggered(PlayerControl? player)
    {
        if (player is null || !player)
        {
            return false;
        }

        return player.GetModifiers<SuiTriggeredProtectionModifier>()
            .Any(x => IsActiveSui(x.SuiId) && IsProtectedBy(player, x.SuiId));
    }

    public static bool IsProtectionTriggeredBy(PlayerControl? player, byte suiId)
    {
        return player is not null && player && IsProtectedBy(player, suiId) &&
               player.GetModifiers<SuiTriggeredProtectionModifier>().Any(x => x.SuiId == suiId);
    }

    public static bool ShouldSuppressKillTarget(PlayerControl? target) =>
        IsValidLiving(target) && IsProtectionTriggered(target);

    public static bool IsRevealedTo(PlayerControl? player, byte suiId)
    {
        return player is not null && player && IsActiveSui(suiId) &&
               player.GetModifiers<SuiRevealedAttackerModifier>().Any(x => x.SuiId == suiId);
    }

    public static PlayerControl? GetActiveHuntTarget(byte suiId)
    {
        if (!IsActiveSui(suiId))
        {
            return null;
        }

        foreach (var player in PlayerControl.AllPlayerControls.ToArray())
        {
            if (IsValidLiving(player) && IsRevealedTo(player, suiId))
            {
                return player;
            }
        }

        return null;
    }

    public static bool HasActiveHuntTarget(byte suiId) => GetActiveHuntTarget(suiId) != null;

    public static void SetLocalHuntTarget(PlayerControl sui, PlayerControl attacker)
    {
        if (!sui || !attacker || !sui.AmOwner || sui.Data == null || sui.Data.Role is not SuiRole)
        {
            return;
        }

        LocalHuntTargetId = attacker.PlayerId;
    }

    public static void ClearLocalHuntState()
    {
        LocalHuntTargetId = null;

        if (!HudManager.InstanceExists)
        {
            return;
        }

        var killButton = HudManager.Instance.KillButton;
        if (killButton != null)
        {
            killButton.SetTarget(null);
            killButton.ToggleVisible(false);
        }
    }

    public static PlayerControl? GetLocalHuntTarget(PlayerControl? sui)
    {
        if (!IsValidLiving(sui) || sui!.Data.Role is not SuiRole || !sui.AmOwner || !LocalHuntTargetId.HasValue)
        {
            return null;
        }

        var target = FindPlayer(LocalHuntTargetId.Value);
        if (!IsValidLiving(target) || target == sui)
        {
            LocalHuntTargetId = null;
            return null;
        }

        return target;
    }

    public static PlayerControl? GetLocalHuntTargetInRange(PlayerControl? sui)
    {
        var marked = GetLocalHuntTarget(sui);
        if (marked == null || sui == null || MeetingHud.Instance != null || ExileController.Instance != null)
        {
            return null;
        }

        var maxDistance = sui.Data.Role.GetAbilityDistance();
        var closest = sui.GetClosestLivingPlayer(
            true,
            maxDistance,
            predicate: player => player != null && player.PlayerId == marked.PlayerId);

        return closest != null && closest.PlayerId == marked.PlayerId ? marked : null;
    }

    public static bool IsLocalHuntTarget(PlayerControl? player, PlayerControl? sui)
    {
        var marked = GetLocalHuntTarget(sui);
        return player != null && marked != null && player.PlayerId == marked.PlayerId;
    }

    public static PlayerControl? GetClosestHuntTarget(PlayerControl? sui)
    {
        if (!IsValidLiving(sui) || sui!.Data.Role is not SuiRole ||
            MeetingHud.Instance != null || ExileController.Instance != null)
        {
            return null;
        }

        var maxDistance = sui.Data.Role.GetAbilityDistance();
        return sui.GetClosestLivingPlayer(
            true,
            maxDistance,
            predicate: player => player != null && IsRevealedTo(player, sui.PlayerId));
    }

    [MethodRpc((uint)TownOfNDevRpc.SuiHuntActivated)]
    public static void RpcHuntActivated(PlayerControl sui, PlayerControl attacker)
    {
        if (!sui || !attacker || !sui.AmOwner || sui.Data == null || sui.Data.Role is not SuiRole || sui.HasDied())
        {
            return;
        }

        SetLocalHuntTarget(sui, attacker);
        sui.SetKillTimer(0f);

        var notification = Helpers.CreateAndShowNotification(
            "<b>Hunt down the player marked <color=#FF0000>RED</color></b>",
            Color.white,
            new Vector3(0f, 1f, -20f),
            spr: TownOfNDevAssets.SuiRoleIcon.LoadAsset());
        notification.AdjustNotification();
        notification.alphaTimer = 5f;
    }

    public static int GetActiveProtectionCount(byte suiId)
    {
        var count = 0;
        foreach (var player in PlayerControl.AllPlayerControls.ToArray())
        {
            if (IsValidLiving(player) && IsProtectedBy(player, suiId))
            {
                count++;
            }
        }

        return count;
    }

    public static bool HasProtectionCapacity(PlayerControl? sui)
    {
        if (!IsValidLiving(sui) || sui!.Data.Role is not SuiRole)
        {
            return false;
        }

        var options = OptionGroupSingleton<SuiOptions>.Instance;
        if (!options.LimitProtectedPlayers.Value)
        {
            return true;
        }

        var maximum = Mathf.Clamp((int)options.MaximumProtectedPlayers.Value, 1, 5);
        return GetActiveProtectionCount(sui.PlayerId) < maximum;
    }

    [MethodRpc((uint)TownOfNDevRpc.SuiProtectRequest)]
    public static void RpcRequestProtect(PlayerControl source, PlayerControl target)
    {
        if (AmongUsClient.Instance == null || !AmongUsClient.Instance.AmHost)
        {
            return;
        }

        HostMaintenance();
        var success = ValidateProtect(source, target);
        if (success)
        {
            target.RpcAddModifier<SuiProtectedModifier>(source.PlayerId);
        }

        RpcProtectResult(source, target, success);
    }

    [MethodRpc((uint)TownOfNDevRpc.SuiProtectResult)]
    public static void RpcProtectResult(PlayerControl source, PlayerControl target, bool success)
    {
        if (source && source.AmOwner)
        {
            SuiProtectButton.HandleHostResult(success);
        }
    }

    [MethodRpc((uint)TownOfNDevRpc.SuiProtectionTriggerRequest)]
    public static void RpcRequestProtectionTrigger(PlayerControl source, PlayerControl target)
    {
        if (AmongUsClient.Instance == null || !AmongUsClient.Instance.AmHost ||
            MeetingHud.Instance != null || ExileController.Instance != null ||
            !IsValidLiving(source) || !IsValidLiving(target) || source == target ||
            !IsProtected(target))
        {
            return;
        }

        HostTriggerProtection(source, target);
    }

    public static bool HostTryBlockMurder(PlayerControl source, PlayerControl target)
    {
        if (AmongUsClient.Instance == null || !AmongUsClient.Instance.AmHost ||
            MeetingHud.Instance != null || ExileController.Instance != null ||
            !IsValidLiving(source) || !IsValidLiving(target) || source == target ||
            !IsProtected(target))
        {
            return false;
        }

        HostTriggerProtection(source, target);
        return true;
    }

    public static void HostHandlePlayerDeath(PlayerControl player)
    {
        if (AmongUsClient.Instance == null || !AmongUsClient.Instance.AmHost || !player)
        {
            return;
        }

        // Any player id can safely be checked as a possible SUI owner. If this was
        // the SUI, all protections and reveal marks owned by them disappear now.
        HostRemoveOwnedBy(player.PlayerId);

        // If a protected target or a revealed attacker dies (including a meeting
        // guess), only state attached to that dead player is removed. A reveal mark
        // earned on a different living attacker remains until SUI or that attacker dies.
        HostRemoveProtectionStateFromTarget(player);
        HostRemoveRevealStateFromAttacker(player);
    }

    public static void HostHandlePlayerLeave(PlayerControl player)
    {
        HostHandlePlayerDeath(player);
    }

    public static void HostMaintenance()
    {
        if (AmongUsClient.Instance == null || !AmongUsClient.Instance.AmHost || LobbyBehaviour.Instance)
        {
            return;
        }

        foreach (var player in PlayerControl.AllPlayerControls.ToArray())
        {
            if (!player)
            {
                continue;
            }

            foreach (var protection in player.GetModifiers<SuiProtectedModifier>().ToArray())
            {
                if (!IsValidLiving(player) || !IsActiveSui(protection.SuiId))
                {
                    player.RpcRemoveModifier(protection.UniqueId);
                }
            }

            foreach (var triggered in player.GetModifiers<SuiTriggeredProtectionModifier>().ToArray())
            {
                if (!IsValidLiving(player) || !IsActiveSui(triggered.SuiId) || !IsProtectedBy(player, triggered.SuiId))
                {
                    player.RpcRemoveModifier(triggered.UniqueId);
                }
            }

            foreach (var revealed in player.GetModifiers<SuiRevealedAttackerModifier>().ToArray())
            {
                if (!IsValidLiving(player) || !IsActiveSui(revealed.SuiId))
                {
                    player.RpcRemoveModifier(revealed.UniqueId);
                }
            }
        }
    }

    private static void HostTriggerProtection(PlayerControl source, PlayerControl target)
    {
        if (AmongUsClient.Instance == null || !AmongUsClient.Instance.AmHost)
        {
            return;
        }

        foreach (var protection in target.GetModifiers<SuiProtectedModifier>().ToArray())
        {
            var suiId = protection.SuiId;
            if (!IsActiveSui(suiId) || IsProtectionTriggeredBy(target, suiId))
            {
                continue;
            }

            // The first attack to activate this protection permanently changes the
            // protected target into a non-selectable normal-kill target. Only the
            // first active attacker becomes this SUI's red hunt target.
            target.RpcAddModifier<SuiTriggeredProtectionModifier>(suiId);

            if (HasActiveHuntTarget(suiId))
            {
                continue;
            }

            if (!source.GetModifiers<SuiRevealedAttackerModifier>().Any(x => x.SuiId == suiId))
            {
                source.RpcAddModifier<SuiRevealedAttackerModifier>(suiId);

                var sui = FindPlayer(suiId);
                if (sui)
                {
                    RpcHuntActivated(sui!, source);
                }
            }
        }
    }

    private static bool ValidateProtect(PlayerControl source, PlayerControl target)
    {
        if (MeetingHud.Instance != null || ExileController.Instance != null ||
            !IsValidLiving(source) || !IsValidLiving(target) || source == target ||
            source.Data.Role is not SuiRole || IsProtected(target) || !HasProtectionCapacity(source))
        {
            return false;
        }

        var maxDistance = source.Data.Role.GetAbilityDistance();
        var closest = source.GetClosestLivingPlayer(
            true,
            maxDistance,
            predicate: player =>
                player != null &&
                player != source &&
                !IsProtected(player));

        return closest is not null && closest && closest.PlayerId == target.PlayerId;
    }

    private static void HostRemoveOwnedBy(byte suiId)
    {
        foreach (var player in PlayerControl.AllPlayerControls.ToArray())
        {
            if (!player)
            {
                continue;
            }

            foreach (var protection in player.GetModifiers<SuiProtectedModifier>()
                         .Where(x => x.SuiId == suiId).ToArray())
            {
                player.RpcRemoveModifier(protection.UniqueId);
            }

            foreach (var triggered in player.GetModifiers<SuiTriggeredProtectionModifier>()
                         .Where(x => x.SuiId == suiId).ToArray())
            {
                player.RpcRemoveModifier(triggered.UniqueId);
            }

            foreach (var revealed in player.GetModifiers<SuiRevealedAttackerModifier>()
                         .Where(x => x.SuiId == suiId).ToArray())
            {
                player.RpcRemoveModifier(revealed.UniqueId);
            }
        }
    }

    private static void HostRemoveProtectionStateFromTarget(PlayerControl player)
    {
        foreach (var protection in player.GetModifiers<SuiProtectedModifier>().ToArray())
        {
            player.RpcRemoveModifier(protection.UniqueId);
        }

        foreach (var triggered in player.GetModifiers<SuiTriggeredProtectionModifier>().ToArray())
        {
            player.RpcRemoveModifier(triggered.UniqueId);
        }
    }

    private static void HostRemoveRevealStateFromAttacker(PlayerControl player)
    {
        foreach (var revealed in player.GetModifiers<SuiRevealedAttackerModifier>().ToArray())
        {
            player.RpcRemoveModifier(revealed.UniqueId);
        }
    }

    private static void HostClearAllState()
    {
        foreach (var player in PlayerControl.AllPlayerControls.ToArray())
        {
            if (!player)
            {
                continue;
            }

            HostRemoveProtectionStateFromTarget(player);
            HostRemoveRevealStateFromAttacker(player);
        }
    }

    private static PlayerControl? FindPlayer(byte playerId)
    {
        foreach (var player in PlayerControl.AllPlayerControls)
        {
            if (player && player.PlayerId == playerId)
            {
                return player;
            }
        }

        return null;
    }

    private static bool IsValidLiving(PlayerControl? player)
    {
        if (player is null || !player || player.Data is null)
        {
            return false;
        }

        return !player.Data.Disconnected && !player.HasDied();
    }
}
