using System.Collections;
using MiraAPI.Events;
using MiraAPI.Events.Mira;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Meeting;
using MiraAPI.Events.Vanilla.Meeting.Voting;
using MiraAPI.Events.Vanilla.Player;
using MiraAPI.Modifiers;
using MiraAPI.Translation;
using Reactor.Utilities;
using TownOfNDev.Buttons.Impostor;
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
        CondemnerDeathNoteButton.RecoverLocalAfterMeeting();
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

    // ProcessVotesEvent runs host-only after votes are calculated but before the
    // results are displayed. A low priority lets ordinary vote/meeting mechanics
    // settle first, then Death Row resolves immediately before the results phase.
    [RegisterEvent(-9000)]
    public static void ProcessVotesHandler(ProcessVotesEvent @event)
    {
        CondemnerSystem.ResolveDeathRow(@event);
    }


    // Town of Us treats every successful meeting murder by an Assassin-holder as a
    // correct guess and also records it as a normal murder. Death Row executions
    // use the same meeting-murder transport, but they are sentences, not guesses
    // or ordinary kills. MiraAPI executes lower priority values first, so use a
    // high positive priority to guarantee TOU's default AfterMurder handlers
    // (including AssassinEvents and GameHistory.AddMurder) have already run.
    // Then undo only the bookkeeping associated with our explicit Death Row cause.
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

        // Do not let a Death Row sentence inflate the ordinary Kills tally.
        GameHistory.ClearMurder(@event.Target);

        // AssassinEvents counts any meeting murder as a correct guess. Reverse
        // that one increment for Death Row while leaving genuine guesses intact.
        if (@event.Source.HasModifier<AssassinModifier>() &&
            GameHistory.PlayerStats.TryGetValue(@event.Source.PlayerId, out var sourceStats) &&
            sourceStats.CorrectAssassinKills > 0)
        {
            sourceStats.CorrectAssassinKills--;
        }
    }

    private static IEnumerator CoApplyDeathRowMarkers(MeetingHud meeting)
    {
        // Vote areas can finish their layout a frame after StartMeetingEvent.
        yield return null;
        if (meeting)
        {
            CondemnerMeetingRenderer.Apply(meeting);
        }
    }
}
