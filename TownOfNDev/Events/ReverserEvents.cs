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
    // Run with the other direct-kill defensive mechanics and well before Miracle's
    // fallback survival roll. Alert is an active role ability, so it owns the
    // direct attack when it is currently active.
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

        // If two Alert states ever meet through a developer override or another
        // extension, cancel the initiating attack without recursively reflecting
        // the reflected kill forever.
        if (source.HasModifier<ReverserAlertModifier>())
        {
            @event.Cancel();
            return;
        }

        @event.Cancel();

        // BeforeMurderEvent is observed by every modded client. Only the owner of
        // the original attacker sends the reflected murder request, matching the
        // established Town of Us Veteran pattern and preventing duplicate kills.
        if (TutorialManager.InstanceExists || source.AmOwner)
        {
            // Reflection is not the Reverser's normal Kill-button action, so do
            // not reset their ordinary Impostor kill cooldown.
            target.RpcCustomMurder(
                source,
                MeetingCheck.OutsideMeeting,
                resetKillTimer: false);
        }
    }
}
