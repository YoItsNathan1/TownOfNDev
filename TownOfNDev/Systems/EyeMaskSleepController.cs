using System.Collections;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using TownOfNDev.Modifiers.Crewmate;
using Reactor.Utilities;
using TownOfNDev.Options;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace TownOfNDev.Systems;

public static class EyeMaskSleepController
{
    private const float FadeInDuration = 0.75f;
    private const float FadeOutDuration = 0.4f;
    public static bool IsPending { get; private set; }
    public static bool IsSleeping { get; private set; }

    private static bool _previousMoveable;
    private static SpriteRenderer? _sleepOverlay;

    public static void OnTaskCompleted()
    {
        if (IsPending || IsSleeping)
        {
            return;
        }

        var options = OptionGroupSingleton<EyeMaskOptions>.Instance;
        if (Random.Range(0f, 100f) > options.TriggerChance.Value)
        {
            return;
        }

        var min = Mathf.Min(options.MinimumDelay.Value, options.MaximumDelay.Value);
        var max = Mathf.Max(options.MinimumDelay.Value, options.MaximumDelay.Value);
        Coroutines.Start(CoSleepAfterDelay(Random.Range(min, max)));
    }

    public static void SafetyTick()
    {
        // A coroutine can be interrupted by a scene/round transition before EndSleep executes.
        // Never allow an old Eye Mask overlay or sleep state to survive into a player/round that
        // does not currently own Eye Mask.
        var player = PlayerControl.LocalPlayer;

        if (_sleepOverlay != null && !IsSleeping)
        {
            DestroySleepOverlay();
        }

        if (!IsSleeping)
        {
            return;
        }

        if (player == null || player.Data == null || player.Data.IsDead || player.Data.Disconnected ||
            !player.HasModifier<EyeMaskModifier>() || MeetingHud.Instance != null || ExileController.Instance != null)
        {
            ForceWake();
        }
    }

    public static void ForceWake()
    {
        IsPending = false;
        if (IsSleeping)
        {
            EndSleep();
        }
        else
        {
            DestroySleepOverlay();
        }
    }

    private static IEnumerator CoSleepAfterDelay(float delay)
    {
        IsPending = true;
        var elapsed = 0f;

        while (elapsed < delay)
        {
            if (ShouldCancel())
            {
                IsPending = false;
                yield break;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        while (!CanBeginSleep())
        {
            if (ShouldCancel())
            {
                IsPending = false;
                yield break;
            }

            yield return null;
        }

        IsPending = false;
        BeginSleep();

        var duration = OptionGroupSingleton<EyeMaskOptions>.Instance.SleepDuration.Value;
        elapsed = 0f;
        while (elapsed < duration)
        {
            if (ShouldCancel())
            {
                break;
            }

            var player = PlayerControl.LocalPlayer;
            if (player != null)
            {
                player.moveable = false;
                player.MyPhysics?.ResetMoveState();
            }

            UpdateSleepOverlay(elapsed, duration);

            elapsed += Time.deltaTime;
            yield return null;
        }

        EndSleep();
    }

    private static bool ShouldCancel()
    {
        var player = PlayerControl.LocalPlayer;
        return player == null ||
               player.Data == null ||
               player.Data.IsDead ||
               player.Data.Disconnected ||
               MeetingHud.Instance != null ||
               ExileController.Instance != null;
    }

    private static bool CanBeginSleep()
    {
        var player = PlayerControl.LocalPlayer;
        if (player == null || player.Data == null)
        {
            return false;
        }

        return !player.inVent &&
               !player.onLadder &&
               !player.inMovingPlat &&
               !player.walkingToVent &&
               player.moveable &&
               MeetingHud.Instance == null &&
               ExileController.Instance == null;
    }

    private static void BeginSleep()
    {
        if (IsSleeping)
        {
            return;
        }

        var player = PlayerControl.LocalPlayer;
        if (player == null)
        {
            return;
        }

        IsSleeping = true;
        _previousMoveable = player.moveable;
        player.moveable = false;
        player.MyPhysics?.ResetMoveState();
        player.NetTransform?.Halt();

        CreateSleepOverlay();
    }

    private static void CreateSleepOverlay()
    {
        DestroySleepOverlay();

        if (HudManager.Instance?.FullScreen == null)
        {
            return;
        }

        // Never recolor or activate Among Us' own FullScreen renderer. Other game systems use it,
        // and mutating it can leave the whole screen tinted outside of Eye Mask. Clone it instead
        // and own the clone exclusively for TownOfNDev's sleep effect.
        var template = HudManager.Instance.FullScreen;
        _sleepOverlay = Object.Instantiate(template, template.transform.parent);
        _sleepOverlay.gameObject.name = "TownOfNDev_EyeMaskSleepOverlay";
        _sleepOverlay.color = Color.clear;
        _sleepOverlay.gameObject.SetActive(true);
    }

    private static void UpdateSleepOverlay(float elapsed, float duration)
    {
        if (_sleepOverlay == null || duration <= 0f)
        {
            return;
        }

        var fadeIn = FadeInDuration;
        var fadeOut = FadeOutDuration;
        var fadeTotal = fadeIn + fadeOut;
        if (fadeTotal > duration)
        {
            var scale = duration / fadeTotal;
            fadeIn *= scale;
            fadeOut *= scale;
        }

        float alpha;
        if (fadeIn > 0f && elapsed < fadeIn)
        {
            alpha = Mathf.SmoothStep(0f, 1f, elapsed / fadeIn);
        }
        else if (fadeOut > 0f && elapsed > duration - fadeOut)
        {
            alpha = 1f - Mathf.SmoothStep(0f, 1f, (elapsed - (duration - fadeOut)) / fadeOut);
        }
        else
        {
            alpha = 1f;
        }

        _sleepOverlay.color = new Color(0f, 0f, 0f, Mathf.Clamp01(alpha));
        _sleepOverlay.gameObject.SetActive(true);
    }

    private static void EndSleep()
    {
        var player = PlayerControl.LocalPlayer;
        if (player != null && player.Data != null && !player.Data.IsDead)
        {
            player.moveable = _previousMoveable;
            player.MyPhysics?.ResetMoveState();
        }

        DestroySleepOverlay();
        IsSleeping = false;
    }

    private static void DestroySleepOverlay()
    {
        if (_sleepOverlay != null)
        {
            Object.Destroy(_sleepOverlay.gameObject);
            _sleepOverlay = null;
        }
    }
}
