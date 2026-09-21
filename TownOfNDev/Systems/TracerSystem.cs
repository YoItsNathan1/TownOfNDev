using System;
using System.Collections.Generic;
using System.Linq;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Utilities;
using Reactor.Networking.Attributes;
using TownOfNDev.Buttons.Crewmate;
using TownOfNDev.Modifiers.Crewmate;
using TownOfNDev.Networking;
using TownOfNDev.Options;
using TownOfNDev.Roles.Crewmate;
using TownOfUs.Utilities;

namespace TownOfNDev.Systems;

public static class TracerSystem
{
    private static readonly HashSet<byte> DustUsedThisRound = [];

    public static void BeginGame()
    {
        DustUsedThisRound.Clear();
        TracerTraceRenderer.ClearAll();

        if (AmongUsClient.Instance == null || !AmongUsClient.Instance.AmHost)
        {
            return;
        }

        HostClearAllDust();
        HostClearAllTrace();
    }

    public static void BeginRoundAfterMeeting()
    {
        TracerTraceRenderer.ClearAll();

        if (AmongUsClient.Instance == null || !AmongUsClient.Instance.AmHost)
        {
            return;
        }

        DustUsedThisRound.Clear();
        HostClearAllTrace();
        HostClearAllDust();
    }

    public static void BeginMeeting()
    {
        TracerTraceRenderer.ClearWorldVisuals();

        if (AmongUsClient.Instance != null && AmongUsClient.Instance.AmHost)
        {
            // Dust stops producing new evidence as soon as the meeting starts.
            // Existing Trace remains until gameplay begins again.
            HostClearAllDust();
        }
    }

    public static bool IsActiveTracer(byte playerId)
    {
        var player = FindPlayer(playerId);
        return IsValidLiving(player) && player!.Data.Role is TracerRole;
    }

    public static bool IsDustedBy(PlayerControl? player, byte tracerId)
    {
        return player is not null && player &&
               player.GetModifiers<TracerDustedModifier>().Any(x => x.TracerId == tracerId);
    }

    public static bool HasTraceFrom(PlayerControl? player, byte tracerId)
    {
        return player is not null && player &&
               player.GetModifiers<TracerTraceModifier>().Any(x => x.TracerId == tracerId);
    }

    public static bool HasAnyTrace(PlayerControl? player)
    {
        return player is not null && player && player.GetModifiers<TracerTraceModifier>().Any();
    }

    public static IEnumerable<byte> GetTraceOwners(PlayerControl player) =>
        player.GetModifiers<TracerTraceModifier>().Select(x => x.TracerId).Distinct();

    public static void HandleDeath(PlayerControl player)
    {
        if (AmongUsClient.Instance == null || !AmongUsClient.Instance.AmHost || !player)
        {
            return;
        }

        // A dead Trace carrier cannot answer for the interaction and no longer
        // displays evidence. Leave Dust in place until the next host tick so an
        // AfterMurderEvent in the same frame can still transfer the victim's Dust
        // to their killer.
        HostRemoveTraceFrom(player);

        if (player.Data?.Role is TracerRole)
        {
            HostClearOwnedBy(player.PlayerId);
        }
    }

    public static void HostHandleMurder(PlayerControl source, PlayerControl target)
    {
        if (AmongUsClient.Instance == null || !AmongUsClient.Instance.AmHost ||
            !source || !target || source == target || !IsValidLiving(source))
        {
            return;
        }

        // Only the living killer can retain useful evidence. If the victim was
        // Dusted, every active Dust owner leaves a separate private Trace on the
        // killer. A Dusted killer would technically transfer powder to the victim,
        // but that victim is now dead and dead Trace is intentionally discarded.
        foreach (var dust in target.GetModifiers<TracerDustedModifier>().ToArray())
        {
            HostApplyTrace(source, dust.TracerId);
        }
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

            foreach (var dust in player.GetModifiers<TracerDustedModifier>().ToArray())
            {
                if (!IsActiveTracer(dust.TracerId))
                {
                    player.RpcRemoveModifier(dust.UniqueId);
                }
            }

            foreach (var trace in player.GetModifiers<TracerTraceModifier>().ToArray())
            {
                if (!IsActiveTracer(trace.TracerId) || player.Data == null || player.Data.Disconnected || player.HasDied())
                {
                    player.RpcRemoveModifier(trace.UniqueId);
                }
            }
        }
    }

    [MethodRpc((uint)TownOfNDevRpc.TracerDustRequest)]
    public static void RpcRequestDust(PlayerControl source, PlayerControl target)
    {
        if (AmongUsClient.Instance == null || !AmongUsClient.Instance.AmHost)
        {
            return;
        }

        var success = ValidateDust(source, target);
        if (success)
        {
            var duration = OptionGroupSingleton<TracerOptions>.Instance.DustDuration.Value;
            target.RpcAddModifier<TracerDustedModifier>(source.PlayerId, duration);
            DustUsedThisRound.Add(source.PlayerId);
        }

        RpcDustResult(source, target, success);
    }

    [MethodRpc((uint)TownOfNDevRpc.TracerDustResult)]
    public static void RpcDustResult(PlayerControl source, PlayerControl target, bool success)
    {
        if (source && source.AmOwner)
        {
            TracerDustButton.HandleHostResult(success);
        }
    }

    [MethodRpc((uint)TownOfNDevRpc.TracerInteractionRequest)]
    public static void RpcRequestInteraction(PlayerControl source, PlayerControl target)
    {
        if (AmongUsClient.Instance == null || !AmongUsClient.Instance.AmHost ||
            MeetingHud.Instance != null || ExileController.Instance != null ||
            !IsValidLiving(source) || !IsValidLiving(target) || source == target)
        {
            return;
        }

        HostTransferFromSuccessfulInteraction(source, target);
    }

    private static void HostTransferFromSuccessfulInteraction(PlayerControl source, PlayerControl target)
    {
        // Bidirectional contact: if either participant is Dusted, the OTHER
        // participant receives that Tracer's private evidence mark. Trace carriers
        // themselves never transmit anything, so this is exactly one hop.
        foreach (var dust in source.GetModifiers<TracerDustedModifier>().ToArray())
        {
            HostApplyTrace(target, dust.TracerId);
        }

        foreach (var dust in target.GetModifiers<TracerDustedModifier>().ToArray())
        {
            HostApplyTrace(source, dust.TracerId);
        }
    }

    private static void HostApplyTrace(PlayerControl recipient, byte tracerId)
    {
        if (!IsValidLiving(recipient) || !IsActiveTracer(tracerId) || HasTraceFrom(recipient, tracerId))
        {
            return;
        }

        recipient.RpcAddModifier<TracerTraceModifier>(tracerId);
    }

    private static bool ValidateDust(PlayerControl source, PlayerControl target)
    {
        if (MeetingHud.Instance != null || ExileController.Instance != null ||
            !IsValidLiving(source) || !IsValidLiving(target) || source == target ||
            source.Data.Role is not TracerRole || DustUsedThisRound.Contains(source.PlayerId) ||
            IsDustedBy(target, source.PlayerId))
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
                !IsDustedBy(player, source.PlayerId));

        return closest is not null && closest && closest.PlayerId == target.PlayerId;
    }

    private static void HostClearOwnedBy(byte tracerId)
    {
        foreach (var player in PlayerControl.AllPlayerControls.ToArray())
        {
            if (!player)
            {
                continue;
            }

            foreach (var dust in player.GetModifiers<TracerDustedModifier>()
                         .Where(x => x.TracerId == tracerId).ToArray())
            {
                player.RpcRemoveModifier(dust.UniqueId);
            }

            foreach (var trace in player.GetModifiers<TracerTraceModifier>()
                         .Where(x => x.TracerId == tracerId).ToArray())
            {
                player.RpcRemoveModifier(trace.UniqueId);
            }
        }
    }

    private static void HostRemoveTraceFrom(PlayerControl player)
    {
        foreach (var trace in player.GetModifiers<TracerTraceModifier>().ToArray())
        {
            player.RpcRemoveModifier(trace.UniqueId);
        }
    }

    private static void HostClearAllDust()
    {
        foreach (var player in PlayerControl.AllPlayerControls.ToArray())
        {
            if (!player)
            {
                continue;
            }

            foreach (var dust in player.GetModifiers<TracerDustedModifier>().ToArray())
            {
                player.RpcRemoveModifier(dust.UniqueId);
            }
        }
    }

    private static void HostClearAllTrace()
    {
        foreach (var player in PlayerControl.AllPlayerControls.ToArray())
        {
            if (!player)
            {
                continue;
            }

            foreach (var trace in player.GetModifiers<TracerTraceModifier>().ToArray())
            {
                player.RpcRemoveModifier(trace.UniqueId);
            }
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
        return player is not null && player && player.Data is not null &&
               !player.Data.Disconnected && !player.HasDied();
    }
}
