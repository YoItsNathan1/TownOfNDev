using System.Collections;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using Reactor.Utilities;
using TownOfNDev.Modifiers.Universal;
using TownOfNDev.Options;
using UnityEngine;
using Random = UnityEngine.Random;

namespace TownOfNDev.Systems;

public enum FumbleActionKind
{
    Kill,
    Ability,
    Report,
    Vent,
    Use
}

public static class ButterfingersController
{
    private static float _nextAllowedFumbleTime;
    private static int _replayDepth;

    public static bool IsReplaying => _replayDepth > 0;

    public static bool TryFumble(FumbleActionKind kind, Action replay)
    {
        if (IsReplaying || EyeMaskSleepController.IsSleeping)
        {
            return false;
        }

        var player = PlayerControl.LocalPlayer;
        if (player == null || player.Data == null || player.Data.IsDead ||
            !player.HasModifier<ButterfingersModifier>())
        {
            return false;
        }

        if (MeetingHud.Instance != null || ExileController.Instance != null || Time.time < _nextAllowedFumbleTime)
        {
            return false;
        }

        var options = OptionGroupSingleton<ButterfingersOptions>.Instance;
        if (!IsKindEnabled(options, kind) || Random.Range(0f, 100f) > options.FumbleChance.Value)
        {
            return false;
        }

        _nextAllowedFumbleTime = Time.time + options.FumbleCooldown.Value;
        var min = Mathf.Min(options.MinimumDelay.Value, options.MaximumDelay.Value);
        var max = Mathf.Max(options.MinimumDelay.Value, options.MaximumDelay.Value);
        Coroutines.Start(CoReplayAfterDelay(replay, Random.Range(min, max)));
        return true;
    }

    private static bool IsKindEnabled(ButterfingersOptions options, FumbleActionKind kind) => kind switch
    {
        FumbleActionKind.Kill => options.FumbleKills,
        FumbleActionKind.Ability => options.FumbleAbilities,
        FumbleActionKind.Report => options.FumbleReports,
        FumbleActionKind.Vent => options.FumbleVents,
        FumbleActionKind.Use => options.FumbleUse,
        _ => false
    };

    private static IEnumerator CoReplayAfterDelay(Action replay, float delay)
    {
        yield return new WaitForSeconds(delay);

        var player = PlayerControl.LocalPlayer;
        if (player == null || player.Data == null || player.Data.IsDead || player.Data.Disconnected ||
            MeetingHud.Instance != null || ExileController.Instance != null || EyeMaskSleepController.IsSleeping)
        {
            yield break;
        }

        _replayDepth++;
        try
        {
            replay();
        }
        finally
        {
            _replayDepth--;
        }
    }
}
