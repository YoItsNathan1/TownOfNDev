using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.GameOptions;
using MiraAPI.Utilities;
using TownOfNDev.Options;
using TownOfNDev.Systems;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfNDev.Events;

public static class TrollEvents
{
    [RegisterEvent]
    public static void RoundStartHandler(RoundStartEvent @event)
    {
        if (@event.TriggeredByIntro)
        {
            TrollSystem.BeginGame();
        }
    }

    [RegisterEvent(11000)]
    public static void AfterMurderHandler(AfterMurderEvent @event)
    {
        var options = OptionGroupSingleton<TrollOptions>.Instance;

        // Meeting deaths are never a Troll objective kill. This excludes correct
        // guesses, Condemner Death Row, Jailor/Deputy-style executions and any
        // other meeting murder path before direct/indirect handling is considered.
        if (MeetingHud.Instance || ExileController.Instance)
        {
            return;
        }

        // Troll always accepts direct player-to-player murders. Indirect murders
        // (for example delayed/AOE/chain-kill paths flagged by MiraAPI) only count
        // when the lobby explicitly enables them. The Guessable option only
        // controls guess-menu availability; a meeting guess never awards the win.
        if (@event.IsIndirectAttack && !options.IndirectKillsCount.Value)
        {
            return;
        }

        if (!TrollSystem.HandleSuccessfulMurder(@event.Source, @event.Target))
        {
            return;
        }
        if (options.AfterWinType.Value != TrollAfterWinType.ContinuesGame || !options.AnnounceWin.Value)
        {
            return;
        }

        var text = $"<b>{@event.Target.Data.PlayerName} has secured a Troll win!</b>";
        var notification = Helpers.CreateAndShowNotification(
            text,
            Color.white,
            new Vector3(0f, 1f, -20f));
        notification.AdjustNotification();
        notification.alphaTimer = 5f;
    }
}
