using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Player;
using MiraAPI.Modifiers;
using TownOfNDev.Modifiers.Crewmate;
using TownOfNDev.Systems;

namespace TownOfNDev.Events;

public static class EyeMaskEvents
{
    [RegisterEvent]
    public static void CompleteTaskEvent(CompleteTaskEvent @event)
    {
        if (!@event.Player.AmOwner || !@event.Player.HasModifier<EyeMaskModifier>())
        {
            return;
        }

        EyeMaskSleepController.OnTaskCompleted();
    }
}
