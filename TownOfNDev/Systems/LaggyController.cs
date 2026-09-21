using System.Collections;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using Reactor.Utilities;
using TownOfNDev.Modifiers.Universal;
using TownOfNDev.Options;
using TownOfUs.Modules;
using TownOfUs.Utilities;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace TownOfNDev.Systems;

/// <summary>
/// Implements Laggy as a local fake movement stutter. It never delays packets and, importantly,
/// never suppresses PlayerPhysics.FixedUpdate. The latter also services visual/gameplay state used
/// by Among Us and other mods, so Laggy only toggles local moveability for the short stall window.
/// </summary>
public static class LaggyController
{
    private const float SharpTurnDotThreshold = 0.5f;
    private const float AfterimageDuration = 0.3f;
    private const float AfterimageOffset = 0.14f;

    private static bool _initialized;
    private static bool _armed;
    private static bool _stuttering;
    private static float _nextArmCheckTime;
    private static Vector2 _lastInput = Vector2.zero;

    public static bool IsStuttering => _stuttering;

    public static void Reset()
    {
        _initialized = false;
        _armed = false;
        _stuttering = false;
        _nextArmCheckTime = 0f;
        _lastInput = Vector2.zero;
    }

    /// <summary>
    /// Called after the local PlayerPhysics.FixedUpdate has completed.
    /// </summary>
    public static void Tick(PlayerPhysics physics)
    {
        if (physics == null)
        {
            Reset();
            return;
        }

        var player = physics.myPlayer;
        if (player == null || !player.AmOwner || player.Data == null ||
            !player.HasModifier<LaggyModifier>())
        {
            Reset();
            return;
        }

        // The coroutine owns state while the short freeze is active. Do not reset just because
        // moveable is false during that window.
        if (_stuttering)
        {
            return;
        }

        if (!IsSafeGameplayState(player))
        {
            Reset();
            return;
        }

        if (!_initialized)
        {
            _initialized = true;
            ScheduleNextArmCheck();
        }

        var input = AdvancedMovementUtilities.GetRegularDirection();

        if (!_armed && Time.time >= _nextArmCheckTime)
        {
            var options = OptionGroupSingleton<LaggyOptions>.Instance;
            if (Random.Range(0f, 100f) <= options.TriggerChance.Value)
            {
                _armed = true;
            }
            else
            {
                ScheduleNextArmCheck();
            }
        }

        if (_armed && ShouldConsumeArm(input))
        {
            _armed = false;
            Coroutines.Start(CoStutter(player, physics, input));
            return;
        }

        _lastInput = input;
    }

    private static bool IsSafeGameplayState(PlayerControl player)
    {
        return ShipStatus.Instance != null &&
               player.Data != null &&
               !player.Data.IsDead &&
               !player.Data.Disconnected &&
               player.moveable &&
               !player.inVent &&
               !player.onLadder &&
               !player.inMovingPlat &&
               !player.walkingToVent &&
               !player.IsInTargetingAnimState() &&
               MeetingHud.Instance == null &&
               ExileController.Instance == null &&
               Minigame.Instance == null &&
               !TimeLordRewindSystem.IsRewinding;
    }

    private static bool ShouldConsumeArm(Vector2 input)
    {
        if (input == Vector2.zero)
        {
            return false;
        }

        if (_lastInput == Vector2.zero)
        {
            return true;
        }

        return Vector2.Dot(_lastInput.normalized, input.normalized) <= SharpTurnDotThreshold;
    }

    private static IEnumerator CoStutter(PlayerControl player, PlayerPhysics physics, Vector2 input)
    {
        if (_stuttering || player == null || player.Data == null || player.Data.IsDead)
        {
            yield break;
        }

        var options = OptionGroupSingleton<LaggyOptions>.Instance;
        var duration = Mathf.Max(0f, options.StutterDuration.Value);
        var previousMoveable = player.moveable;

        _stuttering = true;
        _lastInput = input;

        if (options.ShowAfterimage)
        {
            CreateAfterimage(player, input);
        }

        player.moveable = false;
        physics.ResetMoveState();
        player.NetTransform?.Halt();

        var elapsed = 0f;
        while (elapsed < duration)
        {
            if (player == null || player.Data == null || player.Data.IsDead || player.Data.Disconnected ||
                MeetingHud.Instance != null || ExileController.Instance != null || TimeLordRewindSystem.IsRewinding)
            {
                break;
            }

            // Keep velocity at zero without bypassing PlayerPhysics.FixedUpdate itself.
            physics.ResetMoveState();
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (player != null && player.Data != null && !player.Data.IsDead && !player.Data.Disconnected)
        {
            player.moveable = previousMoveable;
            physics.ResetMoveState();
        }

        _stuttering = false;
        _lastInput = Vector2.zero;
        ScheduleNextArmCheck();
    }

    private static void ScheduleNextArmCheck()
    {
        var options = OptionGroupSingleton<LaggyOptions>.Instance;
        var min = Mathf.Min(options.MinimumInterval.Value, options.MaximumInterval.Value);
        var max = Mathf.Max(options.MinimumInterval.Value, options.MaximumInterval.Value);
        _nextArmCheckTime = Time.time + Random.Range(min, max);
        _armed = false;
    }

    private static void CreateAfterimage(PlayerControl player, Vector2 input)
    {
        try
        {
            var source = player.cosmetics.currentBodySprite.BodySprite;
            if (source == null || !source.enabled || !source.gameObject.activeInHierarchy)
            {
                return;
            }

            var ghost = Object.Instantiate(source);
            ghost.gameObject.name = "TownOfNDev_LaggyAfterimage";
            ghost.transform.position = source.transform.position - (Vector3)(input.normalized * AfterimageOffset);
            ghost.transform.rotation = source.transform.rotation;
            ghost.transform.localScale = source.transform.lossyScale;
            ghost.flipX = source.flipX;
            ghost.sortingLayerID = source.sortingLayerID;
            ghost.sortingOrder = source.sortingOrder - 1;

            var color = source.color;
            color.a = Mathf.Min(color.a, 0.35f);
            ghost.color = color;

            Coroutines.Start(CoFadeAfterimage(ghost, color));
        }
        catch
        {
            // Visual feedback is optional; never let it interfere with movement logic.
        }
    }

    private static IEnumerator CoFadeAfterimage(SpriteRenderer ghost, Color startColor)
    {
        var elapsed = 0f;
        while (ghost != null && elapsed < AfterimageDuration)
        {
            var t = Mathf.Clamp01(elapsed / AfterimageDuration);
            var color = startColor;
            color.a = Mathf.Lerp(startColor.a, 0f, t);
            ghost.color = color;
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (ghost != null)
        {
            Object.Destroy(ghost.gameObject);
        }
    }
}
