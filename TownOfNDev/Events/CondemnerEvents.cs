using System.Collections;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Meeting;
using MiraAPI.Events.Vanilla.Meeting.Voting;
using MiraAPI.Events.Vanilla.Player;
using MiraAPI.Modifiers;
using MiraAPI.Translation;
using Reactor.Utilities;
using TownOfNDev.Systems;
using TownOfUs.Modifiers.Game.Assailant;
using TownOfUs.Modules;

namespace TownOfNDev.Events;

public static class CondemnerEvents
{
    [RegisterEvent]
    public static void RoundStartHandler(RoundStartEvent @event)
    {
        CondemnerTargetTrackerRenderer.Reset();

        if (@event.TriggeredByIntro)
        {
            CondemnerSystem.BeginGame();
            return;
        }

        CondemnerSystem.BeginRoundAfterMeeting();
    }

    [RegisterEvent]
    public static void StartMeetingHandler(StartMeetingEvent @event)
    {
        CondemnerSystem.BeginMeeting();
        CondemnerMeetingRenderer.BeginMeeting(@event.MeetingHud);
        Coroutines.Start(CoApplyDeathRowMarkers(@event.MeetingHud));
    }

    [RegisterEvent]
    public static void PlayerDeathHandler(PlayerDeathEvent @event)
    {
        CondemnerMeetingRenderer.HandlePlayerUnavailable(@event.Player.PlayerId);
        CondemnerTargetTrackerRenderer.HandlePlayerUnavailable(@event.Player.PlayerId);
        CondemnerSystem.HandleDeath(@event.Player);
    }

    [RegisterEvent]
    public static void PlayerLeaveHandler(PlayerLeaveEvent @event)
    {
        var player = @event.ClientData.Character;
        if (player)
        {
            CondemnerMeetingRenderer.HandlePlayerUnavailable(player.PlayerId);
            CondemnerTargetTrackerRenderer.HandlePlayerUnavailable(player.PlayerId);
            CondemnerSystem.HandleLeave(player);
        }
    }

    [RegisterEvent(-9000)]
    public static void ProcessVotesHandler(ProcessVotesEvent @event)
    {
        CondemnerSystem.ResolveDeathRow(@event);
    }

    [RegisterEvent(10000)]
    public static void DeathRowAfterMurderHandler(AfterMurderEvent @event)
    {
        if (!MeetingHud.Instance && !ExileController.Instance)
        {
            return;
        }

        if (!GameHistory.PlayerStats.TryGetValue(@event.Target.PlayerId, out var victimStats) ||
            victimStats.DeathString != MiraLocaleManager.Get("DiedToCondemnerDeathRow"))
        {
            return;
        }

        GameHistory.ClearMurder(@event.Target);

        if (@event.Source.HasModifier<AssassinModifier>() &&
            GameHistory.PlayerStats.TryGetValue(@event.Source.PlayerId, out var sourceStats) &&
            sourceStats.CorrectAssassinKills > 0)
        {
            sourceStats.CorrectAssassinKills--;
        }
    }

    private static IEnumerator CoApplyDeathRowMarkers(MeetingHud meeting)
    {
        yield return null;
        if (meeting)
        {
            CondemnerMeetingRenderer.Apply(meeting);
        }
    }
}
