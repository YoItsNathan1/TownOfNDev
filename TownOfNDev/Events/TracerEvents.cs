using System.Collections;
using MiraAPI.Events;
using MiraAPI.Events.Mira;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Meeting;
using MiraAPI.Events.Vanilla.Player;
using MiraAPI.Hud;
using Reactor.Utilities;
using TownOfNDev.Buttons.Crewmate;
using TownOfNDev.Systems;
using UnityEngine;

namespace TownOfNDev.Events;

public static class TracerEvents
{
    [RegisterEvent]
    public static void RoundStartHandler(RoundStartEvent @event)
    {
        if (@event.TriggeredByIntro)
        {
            TracerSystem.BeginGame();
            return;
        }

        TracerSystem.BeginRoundAfterMeeting();
        TracerDustButton.RecoverLocalAfterMeeting();
    }

    [RegisterEvent]
    public static void StartMeetingHandler(StartMeetingEvent @event)
    {
        TracerSystem.BeginMeeting();

        // Meeting vote areas can finish layout one frame after the event. Wait a
        // frame before attaching the private Trace markers beside marked players.
        Coroutines.Start(CoApplyMeetingMarkers(@event.MeetingHud));
    }

    [RegisterEvent]
    public static void PlayerDeathHandler(PlayerDeathEvent @event)
    {
        TracerSystem.HandleDeath(@event.Player);
        TracerTraceRenderer.UpdateAll();
    }

    [RegisterEvent(10000)]
    public static void AfterMurderHandler(AfterMurderEvent @event)
    {
        // AfterMurderEvent is only fired for a successful murder. The host can use
        // its synchronized Dust state directly, including the victim's Dust that
        // remains for this frame, to leave evidence on the living killer.
        if (AmongUsClient.Instance != null && AmongUsClient.Instance.AmHost)
        {
            TracerSystem.HostHandleMurder(@event.Source, @event.Target);
        }
    }

    [RegisterEvent(9990)]
    public static void PlayerAbilityInteractionHandler(MiraButtonClickEvent @event)
    {
        // Applying Dust itself must not make the Tracer pick up their own powder.
        // Every other successful player-targeted custom ability is eligible.
        if (@event.Button is TracerDustButton || @event.Button is not CustomActionButton<PlayerControl> button)
        {
            return;
        }

        var source = PlayerControl.LocalPlayer;
        var target = button.Target;
        if (source is null || !source || target is null || !target || source == target || !button.CanClick())
        {
            return;
        }

        Coroutines.Start(CoConfirmSuccessfulInteraction(
            @event,
            button,
            source.PlayerId,
            target.PlayerId,
            button.Timer,
            button.UsesLeft,
            button.EffectActive));
    }

    private static IEnumerator CoConfirmSuccessfulInteraction(
        MiraButtonClickEvent clickEvent,
        CustomActionButton<PlayerControl> button,
        byte sourceId,
        byte targetId,
        float timerBefore,
        int usesBefore,
        bool effectBefore)
    {
        var elapsed = 0f;
        const float timeout = 2.25f;

        while (elapsed < timeout)
        {
            yield return null;
            elapsed += Time.deltaTime;

            if (clickEvent.IsCancelled)
            {
                yield break;
            }

            var timerCommitted = timerBefore <= 0.05f && button.Timer > 0.05f;
            var usesCommitted = button.LimitedUses && button.UsesLeft < usesBefore;
            var effectCommitted = !effectBefore && button.EffectActive;
            if (!timerCommitted && !usesCommitted && !effectCommitted)
            {
                continue;
            }

            var source = FindPlayer(sourceId);
            var target = FindPlayer(targetId);
            if (source is null || !source || target is null || !target ||
                source.Data is null || target.Data is null)
            {
                yield break;
            }

            // Successful non-kill interactions only leave evidence if both players
            // remain alive. Successful murders are handled separately above.
            if (source.Data.IsDead || target.Data.IsDead || source.Data.Disconnected || target.Data.Disconnected)
            {
                yield break;
            }

            TracerSystem.RpcRequestInteraction(source, target);
            yield break;
        }
    }

    private static IEnumerator CoApplyMeetingMarkers(MeetingHud meeting)
    {
        yield return null;
        if (meeting)
        {
            TracerMeetingRenderer.Apply(meeting);
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
}
