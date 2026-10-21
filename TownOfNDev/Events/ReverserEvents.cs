using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Modifiers;
using MiraAPI.Networking;
using MiraAPI.Utilities;
using TownOfNDev.Modifiers.Impostor;
using TownOfNDev.Roles.Impostor;

namespace TownOfNDev.Events;

public static class ReverserEvents
{
    [RegisterEvent(-675)]
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
            source.Data == null || source.Data.IsDead || source.Data.Disconnected ||
            target.Data == null || target.Data.IsDead || target.Data.Disconnected ||
            target.Data.Role is not ReverserRole ||
            !target.HasModifier<ReverserAlertModifier>())
        {
            return;
        }

        if (source.HasModifier<ReverserAlertModifier>())
        {
            @event.Cancel();
            return;
        }

        @event.Cancel();

        if (TutorialManager.InstanceExists || source.AmOwner)
        {
            target.RpcCustomMurder(
                source,
                MeetingCheck.OutsideMeeting,
                resetKillTimer: false);
        }
    }
}
