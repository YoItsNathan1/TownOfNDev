using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Player;
using TownOfNDev.Roles.Crewmate;
using TownOfNDev.Systems;

namespace TownOfNDev.Events;

public static class TankEvents
{
    [RegisterEvent]
    public static void CompleteTaskHandler(CompleteTaskEvent @event)
    {
        var player = @event.Player;
        if (!player || player.Data?.Role is not TankRole || !TankSystem.HasCompletedAllTasks(player))
        {
            return;
        }

        TankSystem.HostRefreshProtection(player);
        TankSystem.NotifyProtectionActivated(player);
    }

    [RegisterEvent(-650)]
    public static void BeforeMurderHandler(BeforeMurderEvent @event)
    {
        if (@event.IsCancelled || MeetingHud.Instance || ExileController.Instance ||
            @event.IsIndirectAttack || @event.IgnoreDefense)
        {
            return;
        }

        var source = @event.Source;
        var target = @event.Target;
        if (source is null || !source || target is null || !target || source == target ||
            !TankSystem.IsProtected(target))
        {
            return;
        }

        @event.Cancel();
        TankSystem.NotifyBlockedAttacker(target, source);
    }
}
