using MiraAPI.Events;
using MiraAPI.Events.Mira;
using MiraAPI.Hud;
using TownOfNDev.Systems;

namespace TownOfNDev.Events;

public static class ButterfingersEvents
{
    // Intercept Mira custom buttons at the event boundary rather than patching
    // CustomActionButton.ClickHandler. Town of Us overrides ClickHandler in its
    // own base classes, so a Harmony patch on the Mira base implementation does
    // not reliably cover role buttons.
    [RegisterEvent(-10000)]
    public static void MiraButtonClickHandler(MiraButtonClickEvent @event)
    {
        if (@event.IsCancelled || ButterfingersController.IsReplaying)
        {
            return;
        }

        var button = @event.Button;
        if (button.Button == null || !button.Button)
        {
            return;
        }

        if (ButterfingersController.TryFumble(
                FumbleActionKind.Ability,
                () => ReplayButtonClick(button)))
        {
            @event.Cancel();
        }
    }

    private static void ReplayButtonClick(CustomActionButton button)
    {
        if (button.Button == null || !button.Button ||
            MeetingHud.Instance || ExileController.Instance)
        {
            return;
        }

        var passive = button.Button.GetComponent<PassiveButton>();
        if (!passive || passive.OnClick == null)
        {
            return;
        }

        // Re-enter the actual Mira click pipeline so click events, cancellation,
        // interaction tracking and the normal ClickHandler all run in the same
        // order as a real user click. ButterfingersController.IsReplaying prevents
        // this replay from being delayed a second time.
        passive.OnClick.Invoke();
    }
}
