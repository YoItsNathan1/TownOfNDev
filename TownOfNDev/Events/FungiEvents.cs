using System.Collections;
using MiraAPI.Events;
using MiraAPI.Events.Mira;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Player;
using MiraAPI.Hud;
using Reactor.Utilities;
using TownOfUs.Events.TouEvents;
using TownOfNDev.Buttons.Neutral;
using TownOfNDev.Systems;
using UnityEngine;

namespace TownOfNDev.Events;

public static class FungiEvents
{
    [RegisterEvent]
    public static void RoundStartHandler(RoundStartEvent @event)
    {
        if (@event.TriggeredByIntro)
        {
            FungiInfectionSystem.BeginGame();
            return;
        }

        FungiInfectionSystem.AdvanceGrowthAfterMeeting();
    }

    [RegisterEvent]
    public static void PlayerDeathHandler(PlayerDeathEvent @event)
    {
        FungiInfectionSystem.HandleDeath(@event.Player);
        FungiInfectionSystem.HostUpdateObjective();
    }

    [RegisterEvent]
    public static void PlayerReviveHandler(PlayerReviveEvent @event)
    {
        FungiInfectionSystem.HandleRevive(@event.Player);
        FungiInfectionSystem.HostUpdateObjective();
    }

    [RegisterEvent(10000)]
    public static void PlayerAbilityInteractionHandler(MiraButtonClickEvent @event)
    {
        if (@event.Button is FungiInfectButton || @event.Button is not CustomActionButton<PlayerControl> button)
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

            var sourceData = source.Data;
            var targetData = target.Data;
            if (sourceData.IsDead || targetData.IsDead || sourceData.Disconnected || targetData.Disconnected)
            {
                yield break;
            }

            FungiInfectionSystem.RpcRequestSpread(source, target);
            yield break;
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
