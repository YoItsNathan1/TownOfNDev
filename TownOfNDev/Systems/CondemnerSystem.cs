using System;
using System.Collections.Generic;
using System.Linq;
using MiraAPI.Events.Vanilla.Meeting.Voting;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Utilities;
using Reactor.Networking.Attributes;
using TownOfNDev.Buttons.Impostor;
using TownOfNDev.Modifiers.Impostor;
using TownOfNDev.Networking;
using TownOfNDev.Options;
using TownOfNDev.Roles.Impostor;
using TownOfUs.Modifiers.Crewmate;
using TownOfUs.Networking;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfNDev.Systems;

public static class CondemnerSystem
{
    private static readonly Dictionary<byte, int> UsesThisRound = [];
    private static readonly HashSet<byte> CancelledOwnersThisMeeting = [];
    private static bool _meetingActive;

    public static void BeginGame()
    {
        UsesThisRound.Clear();
        CancelledOwnersThisMeeting.Clear();
        _meetingActive = false;
        CondemnerMeetingRenderer.Reset();

        if (AmongUsClient.Instance != null && AmongUsClient.Instance.AmHost)
        {
            HostClearAllCondemned();
        }
    }

    public static void BeginRoundAfterMeeting()
    {
        UsesThisRound.Clear();
        CancelledOwnersThisMeeting.Clear();
        _meetingActive = false;
        CondemnerMeetingRenderer.Reset();

        if (AmongUsClient.Instance != null && AmongUsClient.Instance.AmHost)
        {
            // Safety cleanup for aborted/custom meeting transitions. A sentence is
            // always for the next meeting only and can never roll into another round.
            HostClearAllCondemned();
        }
    }

    public static void BeginMeeting()
    {
        _meetingActive = true;
        CancelledOwnersThisMeeting.Clear();

        if (AmongUsClient.Instance == null || !AmongUsClient.Instance.AmHost)
        {
            return;
        }

        // If persistence is disabled and the Condemner died/disconnected immediately
        // before the meeting event, remove those sentences before public skulls render.
        if (!OptionGroupSingleton<CondemnerOptions>.Instance.PersistAfterCondemnerDeath.Value)
        {
            foreach (var player in PlayerControl.AllPlayerControls.ToArray())
            {
                if (!player)
                {
                    continue;
                }

                foreach (var mod in player.GetModifiers<CondemnedModifier>().ToArray())
                {
                    if (!IsLivingCondemner(mod.CondemnerId))
                    {
                        player.RpcRemoveModifier(mod.UniqueId);
                    }
                }
            }
        }
    }

    public static bool IsCondemned(PlayerControl? player)
    {
        return player is not null && player && player.HasModifier<CondemnedModifier>();
    }

    public static bool IsCondemnedBy(PlayerControl? player, byte condemnerId)
    {
        if (player is null || !player)
        {
            return false;
        }

        return player.GetModifiers<CondemnedModifier>().Any(mod => mod.CondemnerId == condemnerId);
    }

    public static void HandleDeath(PlayerControl player)
    {
        if (AmongUsClient.Instance == null || !AmongUsClient.Instance.AmHost || !player)
        {
            return;
        }

        // A prisoner who is already dead cannot later be executed. Removing the
        // modifier does not remove an already-created meeting skull GameObject, so a
        // guessed Death Row player can keep the visual mark for the rest of the meeting.
        HostRemoveCondemnedFrom(player);

        if (player.Data?.Role is not CondemnerRole)
        {
            return;
        }

        HandleCondemnerUnavailable(player.PlayerId);
    }

    public static void HandleLeave(PlayerControl player)
    {
        if (AmongUsClient.Instance == null || !AmongUsClient.Instance.AmHost || !player)
        {
            return;
        }

        HostRemoveCondemnedFrom(player);

        if (player.Data?.Role is CondemnerRole)
        {
            HandleCondemnerUnavailable(player.PlayerId);
        }
    }

    private static void HandleCondemnerUnavailable(byte condemnerId)
    {
        if (OptionGroupSingleton<CondemnerOptions>.Instance.PersistAfterCondemnerDeath.Value)
        {
            return;
        }

        if (_meetingActive)
        {
            // Keep all sentence modifiers in place until voting resolves so the red
            // skulls remain visible and nobody gets confirmation they were saved.
            CancelledOwnersThisMeeting.Add(condemnerId);
            return;
        }

        HostClearOwnedBy(condemnerId);
    }

    [MethodRpc((uint)TownOfNDevRpc.CondemnerDeathNoteRequest)]
    public static void RpcRequestDeathNote(PlayerControl source, PlayerControl target)
    {
        if (AmongUsClient.Instance == null || !AmongUsClient.Instance.AmHost)
        {
            return;
        }

        var success = ValidateDeathNote(source, target);
        if (success)
        {
            target.RpcAddModifier<CondemnedModifier>(source.PlayerId);
            UsesThisRound[source.PlayerId] = GetUsesThisRound(source.PlayerId) + 1;
        }

        RpcDeathNoteResult(source, target, success);
    }

    [MethodRpc((uint)TownOfNDevRpc.CondemnerDeathNoteResult)]
    public static void RpcDeathNoteResult(PlayerControl source, PlayerControl target, bool success)
    {
        if (source && source.AmOwner)
        {
            CondemnerDeathNoteButton.HandleHostResult(success);
        }
    }

    public static void ResolveDeathRow(ProcessVotesEvent @event)
    {
        if (AmongUsClient.Instance == null || !AmongUsClient.Instance.AmHost)
        {
            return;
        }

        var exiledId = @event.ExiledPlayer?.PlayerId;
        var persist = OptionGroupSingleton<CondemnerOptions>.Instance.PersistAfterCondemnerDeath.Value;

        // Snapshot first. RpcMeetingMurder/PlayerDeathEvent can mutate the modifier
        // collection synchronously while we execute everybody on this same host frame.
        var prisoners = new List<(PlayerControl Target, byte OwnerId)>();
        foreach (var player in PlayerControl.AllPlayerControls.ToArray())
        {
            if (!player || player.Data == null || player.Data.Disconnected || player.HasDied())
            {
                continue;
            }

            if (exiledId.HasValue && player.PlayerId == exiledId.Value)
            {
                // Vote/ejection takes priority over Death Row; never double-kill.
                continue;
            }

            var mod = player.GetModifier<CondemnedModifier>();
            if (mod == null)
            {
                continue;
            }

            if (!SentenceOwnerValid(mod.CondemnerId, exiledId, persist))
            {
                continue;
            }

            prisoners.Add((player, mod.CondemnerId));
        }

        // Dispatch every execution in this one ProcessVotesEvent invocation with no
        // waits/coroutines between calls. Different vote-area nameplate animations can
        // therefore begin together rather than producing a deliberate one-by-one chain.
        foreach (var group in prisoners.GroupBy(x => x.OwnerId))
        {
            var source = FindPlayer(group.Key);
            if (source is null || !source)
            {
                // Persistence after a fully disconnected Condemner has no safe source
                // object to attribute the meeting murder to. Leave those prisoners
                // alive rather than fabricating another player's kill attribution.
                continue;
            }

            foreach (var prisoner in group)
            {
                source.RpcMeetingMurder(
                    prisoner.Target,
                    MeetingAnimation.PlayerNameplateAnimation,
                    CustomTouMurderRpcs.GetRandomMeetingAnim(DeathAnimType.Nameplate),
                    didSucceed: true,
                    causeOfDeath: "CondemnerDeathRow");
            }
        }

        // Sentence state is consumed by this meeting whether execution happened,
        // was cancelled by the Condemner dying, or the prisoner was voted/guessed.
        HostClearAllCondemned();
        CancelledOwnersThisMeeting.Clear();
    }

    private static bool SentenceOwnerValid(byte ownerId, byte? exiledId, bool persist)
    {
        if (persist)
        {
            var owner = FindPlayer(ownerId);
            return owner is not null && owner;
        }

        if (CancelledOwnersThisMeeting.Contains(ownerId))
        {
            return false;
        }

        if (exiledId.HasValue && exiledId.Value == ownerId)
        {
            return false;
        }

        return IsLivingCondemner(ownerId);
    }

    private static bool ValidateDeathNote(PlayerControl source, PlayerControl target)
    {
        var role = source.Data?.Role;
        if (MeetingHud.Instance != null || ExileController.Instance != null ||
            !IsValidLiving(source) || !IsValidLiving(target) || source == target ||
            role is not CondemnerRole || target.IsImpostorAligned() ||
            IsCondemned(target) || target.HasModifier<WardenFortifiedModifier>())
        {
            return false;
        }

        var options = OptionGroupSingleton<CondemnerOptions>.Instance;
        if (options.LimitUsesPerRound.Value &&
            GetUsesThisRound(source.PlayerId) >= Mathf.Clamp((int)options.MaxUsesPerRound.Value, 1, 5))
        {
            return false;
        }

        var maxDistance = role.GetAbilityDistance();
        var closest = source.GetClosestLivingPlayer(
            true,
            maxDistance,
            predicate: player =>
                player != null &&
                player != source &&
                !player.IsImpostorAligned() &&
                !IsCondemned(player));

        return closest is not null && closest && closest.PlayerId == target.PlayerId;
    }

    private static int GetUsesThisRound(byte playerId) =>
        UsesThisRound.TryGetValue(playerId, out var uses) ? uses : 0;

    private static bool IsLivingCondemner(byte playerId)
    {
        var player = FindPlayer(playerId);
        return player is not null && player && player.Data is not null &&
               !player.Data.Disconnected && !player.HasDied() && player.Data.Role is CondemnerRole;
    }

    private static void HostRemoveCondemnedFrom(PlayerControl player)
    {
        foreach (var mod in player.GetModifiers<CondemnedModifier>().ToArray())
        {
            player.RpcRemoveModifier(mod.UniqueId);
        }
    }

    private static void HostClearOwnedBy(byte condemnerId)
    {
        foreach (var player in PlayerControl.AllPlayerControls.ToArray())
        {
            if (!player)
            {
                continue;
            }

            foreach (var mod in player.GetModifiers<CondemnedModifier>()
                         .Where(x => x.CondemnerId == condemnerId).ToArray())
            {
                player.RpcRemoveModifier(mod.UniqueId);
            }
        }
    }

    private static void HostClearAllCondemned()
    {
        foreach (var player in PlayerControl.AllPlayerControls.ToArray())
        {
            if (!player)
            {
                continue;
            }

            foreach (var mod in player.GetModifiers<CondemnedModifier>().ToArray())
            {
                player.RpcRemoveModifier(mod.UniqueId);
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
