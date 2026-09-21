using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Modifiers;
using MiraAPI.Utilities;
using TownOfNDev.Modifiers.Universal;
using TownOfNDev.Systems;

namespace TownOfNDev.Events;

public static class MiracleEvents
{
    // Run after ordinary protection handlers. If another mechanic already blocked
    // the attack, Miracle does not spend a random survival roll on the same attempt.
    [RegisterEvent(1000000)]
    public static void BeforeMurderHandler(BeforeMurderEvent @event)
    {
        if (@event.IsCancelled || MeetingHud.Instance || ExileController.Instance)
        {
            return;
        }

        var source = @event.Source;
        var target = @event.Target;
        if (!source || !target || source == target || target.Data == null || target.Data.IsDead ||
            target.Data.Disconnected || !target.HasModifier<MiracleModifier>())
        {
            return;
        }

        // The RNG decision is host-authoritative. Non-host callers still send the
        // murder request normally; the host decides whether it succeeds.
        if (AmongUsClient.Instance == null || !AmongUsClient.Instance.AmHost)
        {
            return;
        }

        // Do not consume a Miracle roll when vanilla Guardian Angel protection is
        // already responsible for blocking the attempt.
        if (!@event.IgnoreDefense && target.ProtectedByGa())
        {
            return;
        }

        if (!MiracleSystem.RollSurvival())
        {
            return;
        }

        // A successful Miracle roll is intentionally treated as a protected kill so
        // the attacker receives the familiar blocked-kill visual feedback. Clear
        // IgnoreDefense only for this cancelled attempt so even special murder
        // requests resolve as a Miracle protection rather than a generic failure.
        @event.IgnoreDefense = false;
        MiracleSystem.MarkPendingProtectedResult(target);
        @event.Cancel();
    }
}
