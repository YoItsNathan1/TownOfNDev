using System;
using System.Collections.Generic;
using System.Linq;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Utilities;
using Reactor.Networking.Attributes;
using TownOfNDev.Buttons.Neutral;
using TownOfNDev.Modifiers.Neutral;
using TownOfNDev.Networking;
using TownOfNDev.Options;
using TownOfNDev.Roles.Neutral;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfNDev.Systems;

public static class FungiInfectionSystem
{
    private static readonly HashSet<byte> FungiMemberIds = [];
    private static readonly Dictionary<byte, byte> DeathStageSnapshots = [];

    public static bool ObjectiveAchieved { get; private set; }

    public static IReadOnlyCollection<byte> Members => FungiMemberIds;

    public static void BeginGame()
    {
        FungiGrowthRenderer.ClearAll();
        DeathStageSnapshots.Clear();
        FungiMemberIds.Clear();
        ObjectiveAchieved = false;

        foreach (var player in PlayerControl.AllPlayerControls.ToArray())
        {
            if (player is null || !player)
            {
                continue;
            }

            // Defensive cleanup in case a prior match ended while an infection
            // modifier was active.
            if (player.TryGetModifier<FungiInfectedModifier>(out var staleInfection))
            {
                player.RemoveModifier(staleInfection);
            }

            if (player.Data?.Role is FungiRole)
            {
                FungiMemberIds.Add(player.PlayerId);
            }
        }
    }

    public static void RegisterFungi(PlayerControl player)
    {
        if (player is not null && player)
        {
            FungiMemberIds.Add(player.PlayerId);
        }
    }

    public static bool IsFungi(PlayerControl? player)
    {
        if (player is null || !player)
        {
            return false;
        }

        return FungiMemberIds.Contains(player.PlayerId) || player.Data?.Role is FungiRole;
    }

    public static bool IsInfected(PlayerControl? player) =>
        player is not null && player && player.HasModifier<FungiInfectedModifier>();

    // Fungi members are the original infection source even though they do not carry
    // the victim-side FungiInfectedModifier. This helper is the authoritative test
    // for interaction transmission.
    public static bool IsContagious(PlayerControl? player) =>
        IsFungi(player) || IsInfected(player);

    public static byte GetStage(PlayerControl? player)
    {
        if (player is not null && player &&
            player.TryGetModifier<FungiInfectedModifier>(out var modifier) && modifier is not null)
        {
            return modifier.Stage;
        }
        return 0;
    }

    public static void HandleDeath(PlayerControl player)
    {
        if (!player || !player.TryGetModifier<FungiInfectedModifier>(out var modifier))
        {
            return;
        }

        DeathStageSnapshots[player.PlayerId] = modifier.Stage;
        player.RemoveModifier(modifier);
        FungiGrowthRenderer.Remove(player.PlayerId);
    }

    public static void HandleRevive(PlayerControl player)
    {
        if (!player || !DeathStageSnapshots.TryGetValue(player.PlayerId, out var oldStage))
        {
            return;
        }

        if (OptionGroupSingleton<FungiOptions>.Instance.RestoreInfectionAfterRevival.Value)
        {
            ApplyInfectionLocal(player, oldStage);
        }

        DeathStageSnapshots.Remove(player.PlayerId);
    }

    public static void AdvanceGrowthAfterMeeting()
    {
        foreach (var player in PlayerControl.AllPlayerControls.ToArray())
        {
            if (!IsValidLiving(player) || !player.TryGetModifier<FungiInfectedModifier>(out var modifier))
            {
                continue;
            }

            modifier.SetStage((byte)Math.Min(3, modifier.Stage + 1));
        }
    }

    public static (int Infected, int Eligible, int Required) GetProgress()
    {
        var eligiblePlayers = PlayerControl.AllPlayerControls.ToArray()
            .Where(IsValidLiving)
            .Where(x => !IsFungi(x))
            .ToArray();

        var infected = eligiblePlayers.Count(IsInfected);
        var percentage = Mathf.Clamp(
            OptionGroupSingleton<FungiOptions>.Instance.RequiredInfectionPercentage.Value,
            1f,
            100f);
        var required = eligiblePlayers.Length == 0
            ? int.MaxValue
            : Mathf.CeilToInt(eligiblePlayers.Length * percentage / 100f);

        return (infected, eligiblePlayers.Length, required);
    }

    public static bool CurrentThresholdMet
    {
        get
        {
            var progress = GetProgress();
            return progress.Eligible > 0 && progress.Infected >= progress.Required;
        }
    }

    public static bool AnyFungiAlive() => PlayerControl.AllPlayerControls.ToArray()
        .Any(x => IsValidLiving(x) && IsFungi(x));

    public static NetworkedPlayerInfo[] GetFungiWinnerData() => PlayerControl.AllPlayerControls.ToArray()
        .Where(x => x is not null && x && x.Data is not null && !x.Data.Disconnected && FungiMemberIds.Contains(x.PlayerId))
        .Select(x => x.Data!)
        .Distinct()
        .ToArray();

    public static void HostUpdateObjective()
    {
        if (AmongUsClient.Instance == null || !AmongUsClient.Instance.AmHost || ObjectiveAchieved || FungiMemberIds.Count == 0)
        {
            return;
        }

        if (CurrentThresholdMet)
        {
            var sender = PlayerControl.LocalPlayer;
            if (sender is not null && sender)
            {
                RpcSetObjectiveAchieved(sender, true);
            }
        }
    }

    [MethodRpc((uint)TownOfNDevRpc.FungiInfectRequest)]
    public static void RpcRequestInfect(PlayerControl source, PlayerControl target)
    {
        if (AmongUsClient.Instance == null || !AmongUsClient.Instance.AmHost)
        {
            return;
        }

        var success = ValidateDirectInfect(source, target);
        if (success)
        {
            // Use MiraAPI's built-in modifier RPC for the actual infection state.
            // This is important: a local AddModifier/our old apply RPC can leave
            // the host knowing a player is infected while another client still
            // considers that player clean. RpcAddModifier gives every client the
            // same modifier GUID/state and is the canonical Mira synchronization
            // path for modifiers.
            success = HostApplyInfectionNetworked(target, 0);
        }

        // This result RPC only controls the Fungi owner's button/cooldown. The
        // infection itself has already been synchronized independently above.
        RpcInfectResult(source, target, success);
    }

    [MethodRpc((uint)TownOfNDevRpc.FungiInfectResult)]
    public static void RpcInfectResult(PlayerControl source, PlayerControl target, bool success)
    {
        if (source && source.AmOwner)
        {
            FungiInfectButton.HandleHostResult(success);
        }
    }

    [MethodRpc((uint)TownOfNDevRpc.FungiSpreadRequest)]
    public static void RpcRequestSpread(PlayerControl source, PlayerControl target)
    {
        if (AmongUsClient.Instance == null || !AmongUsClient.Instance.AmHost)
        {
            return;
        }

        if (!IsValidLiving(source) || !IsValidLiving(target) || source == target)
        {
            return;
        }

        // A Fungi member is contagious by definition, even though Fungi itself does
        // not receive FungiInfectedModifier. Likewise, an infected victim remains a
        // contagious source. Successful interaction transmission only occurs when
        // exactly one participant is contagious.
        var sourceContagious = IsContagious(source);
        var targetContagious = IsContagious(target);
        if (sourceContagious == targetContagious)
        {
            return;
        }

        var cleanPlayer = sourceContagious ? target : source;
        if (IsFungi(cleanPlayer) || IsInfected(cleanPlayer))
        {
            return;
        }

        HostApplyInfectionNetworked(cleanPlayer, 0);
    }

    [MethodRpc((uint)TownOfNDevRpc.FungiSetObjectiveAchieved)]
    public static void RpcSetObjectiveAchieved(PlayerControl sender, bool achieved)
    {
        ObjectiveAchieved = achieved;
    }


    private static bool HostApplyInfectionNetworked(PlayerControl target, byte stage)
    {
        if (AmongUsClient.Instance == null || !AmongUsClient.Instance.AmHost ||
            !IsValidLiving(target) || IsFungi(target) || IsInfected(target))
        {
            return false;
        }

        var clampedStage = (byte)Math.Clamp(stage, (byte)0, (byte)3);

        // Do not use AddModifier directly here. MiraAPI provides RpcAddModifier
        // specifically so the modifier instance, GUID and constructor arguments
        // are installed identically on the host and every connected client. This
        // keeps Fungi target filtering/name tint/growth state in sync immediately.
        target.RpcAddModifier<FungiInfectedModifier>(clampedStage);
        HostUpdateObjective();
        return true;
    }

    private static bool ValidateDirectInfect(PlayerControl source, PlayerControl target)
    {
        if (MeetingHud.Instance != null || ExileController.Instance != null)
        {
            return false;
        }

        if (!IsValidLiving(source) || !IsValidLiving(target) || !IsFungi(source) || IsFungi(target) || IsInfected(target))
        {
            return false;
        }

        var maxDistance = source.Data.Role.GetAbilityDistance();
        var closest = source.GetClosestLivingPlayer(
            true,
            maxDistance,
            predicate: player =>
                player != null &&
                !IsFungi(player) &&
                !IsInfected(player));

        return closest is not null && closest && closest.PlayerId == target.PlayerId;
    }

    private static void ApplyInfectionLocal(PlayerControl target, byte stage)
    {
        if (!IsValidLiving(target) || IsFungi(target) || IsInfected(target))
        {
            return;
        }

        target.AddModifier<FungiInfectedModifier>((byte)Math.Clamp(stage, (byte)0, (byte)3));
        HostUpdateObjective();
    }

    private static bool IsValidLiving(PlayerControl? player)
    {
        if (player is null || !player || player.Data is null)
        {
            return false;
        }

        var data = player.Data;
        return !data.Disconnected && !player.HasDied();
    }
}
