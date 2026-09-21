using System.Collections.Generic;
using BepInEx.Logging;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Keybinds;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using TownOfNDev.Assets;
using TownOfNDev.Options;
using TownOfNDev.Roles.Impostor;
using TownOfNDev.Systems;
using TownOfUs;
using TownOfUs.Buttons;
using TownOfUs.Options;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfNDev.Buttons.Impostor;

public sealed class CondemnerDeathNoteButton : TownOfUsRoleButton<CondemnerRole, PlayerControl>
{
    private static readonly ManualLogSource TimerLog = BepInEx.Logging.Logger.CreateLogSource("TownOfNDev.CondemnerTimer");

    private bool _awaitingHost;

    // The TOU/Mira custom-button update can be invoked more than once per real
    // second in some locally-owned-player setups. Store an absolute real-time
    // cooldown state so duplicate update calls cannot make Death Note run fast.
    private bool _realTimeCooldownActive;
    private float _realTimeCooldownRemaining;
    private float _lastRealtimeSample;

    // One short diagnostic sample per cooldown. This will tell us whether the
    // handler is being called twice and which PlayerControl IDs are driving it.
    private float _diagnosticStartRealtime;
    private int _diagnosticHandlerCalls;
    private readonly HashSet<byte> _diagnosticPlayerIds = [];
    private bool _diagnosticLogged;

    public override string Name => "Death Note";
    public override BaseKeybind Keybind => Keybinds.SecondaryAction;
    public override Color TextOutlineColor => TownOfUsColors.Impostor;
    public override float InitialCooldown => Cooldown;
    public override float Cooldown => OptionGroupSingleton<CondemnerOptions>.Instance.DeathNoteCooldown.Value;
    public override int MaxUses => OptionGroupSingleton<CondemnerOptions>.Instance.LimitUsesPerRound.Value
        ? Mathf.Clamp((int)OptionGroupSingleton<CondemnerOptions>.Instance.MaxUsesPerRound.Value, 1, 5)
        : 0;
    public override ButtonUsesMode UsesMode => ButtonUsesMode.PerRound;
    public override LoadableAsset<Sprite> Sprite => TownOfNDevAssets.CondemnerDeathNoteButton;

    public override void CreateButton(Transform parent)
    {
        base.CreateButton(parent);

        // Track the initial cooldown too. Mira sets Timer before this override returns.
        BeginRealTimeCooldown(Timer);

        // The approved TownOfNDev button artwork already contains the DEATH NOTE
        // label in the desired Among Us-style treatment, so avoid a duplicate label.
        if (Button?.buttonLabelText != null)
        {
            Button.buttonLabelText.gameObject.SetActive(false);
        }
    }

    public override void ResetCooldownAndOrEffect()
    {
        base.ResetCooldownAndOrEffect();
        BeginRealTimeCooldown(Timer);
    }

    public override PlayerControl? GetTarget()
    {
        var local = PlayerControl.LocalPlayer;
        if (!local || local.Data == null)
        {
            return null;
        }

        return local.GetClosestLivingPlayer(
            true,
            Distance,
            predicate: player =>
                player != null &&
                player != local &&
                !player.IsImpostorAligned() &&
                !CondemnerSystem.IsCondemned(player));
    }

    public override bool CanUse()
    {
        return !_awaitingHost && base.CanUse();
    }

    public override bool CanClick()
    {
        return !_awaitingHost && base.CanClick();
    }

    public override void FixedUpdateHandler(PlayerControl playerControl)
    {
        RecordTimerDiagnostic(playerControl);

        if (!_realTimeCooldownActive)
        {
            base.FixedUpdateHandler(playerControl);
            return;
        }

        var now = Time.realtimeSinceStartup;
        var realElapsed = Mathf.Max(0f, now - _lastRealtimeSample);
        _lastRealtimeSample = now;

        var local = PlayerControl.LocalPlayer;
        var shouldPauseInVent = local && ShouldPauseInVent && local.inVent && !EffectActive;
        var shouldTick = !TimerPaused &&
                         !OptionGroupSingleton<VanillaTweakOptions>.Instance.CanPauseCooldown &&
                         (!shouldPauseInVent || EffectActive);

        if (shouldTick)
        {
            _realTimeCooldownRemaining = Mathf.Max(0f, _realTimeCooldownRemaining - realElapsed);
        }

        Timer = _realTimeCooldownRemaining;

        // Prevent TownOfUsTargetButton.FixedUpdateHandler from subtracting a second
        // time. It still performs normal target acquisition, enable/disable state,
        // HUD work and its protected FixedUpdate call.
        var previousTimerPaused = TimerPaused;
        try
        {
            TimerPaused = true;
            base.FixedUpdateHandler(playerControl);
        }
        finally
        {
            TimerPaused = previousTimerPaused;
        }

        // Base rendered the current value while paused; enforce our real-time value
        // again in case another implementation changed Timer while updating.
        Timer = _realTimeCooldownRemaining;
        Button?.SetCooldownFormat(Timer, Cooldown, CooldownTimerFormatString);

        if (_realTimeCooldownRemaining <= 0f)
        {
            _realTimeCooldownActive = false;
            SetTimer(0f);
        }
    }

    public override void ClickHandler()
    {
        if (!CanClick() || Target == null)
        {
            return;
        }

        if (ButterfingersController.TryFumble(FumbleActionKind.Ability, ClickHandler))
        {
            return;
        }

        _awaitingHost = true;
        CondemnerSystem.RpcRequestDeathNote(PlayerControl.LocalPlayer, Target);
    }

    protected override void OnClick()
    {
        // Host-authoritative. Cooldown/use consumption and the normal Kill reset
        // happen only after the host confirms the interaction actually succeeded.
    }

    public void ApplyHostResult(bool success)
    {
        _awaitingHost = false;
        if (!success)
        {
            ResetTarget();
            return;
        }

        if (OptionGroupSingleton<CondemnerOptions>.Instance.LimitUsesPerRound.Value)
        {
            SetUses(Mathf.Max(0, UsesLeft - 1));
        }

        BeginRealTimeCooldown(Cooldown);

        var local = PlayerControl.LocalPlayer;
        if (local && local.Data != null && !local.Data.IsDead)
        {
            local.SetKillTimer(local.GetKillCooldown());
        }

        ResetTarget();
    }

    private void BeginRealTimeCooldown(float duration)
    {
        _realTimeCooldownRemaining = Mathf.Max(0f, duration);
        _realTimeCooldownActive = _realTimeCooldownRemaining > 0f;
        _lastRealtimeSample = Time.realtimeSinceStartup;
        SetTimer(_realTimeCooldownRemaining);

        _diagnosticStartRealtime = _lastRealtimeSample;
        _diagnosticHandlerCalls = 0;
        _diagnosticPlayerIds.Clear();
        _diagnosticLogged = false;
    }

    private void RecordTimerDiagnostic(PlayerControl playerControl)
    {
        if (!_realTimeCooldownActive || _diagnosticLogged)
        {
            return;
        }

        _diagnosticHandlerCalls++;
        if (playerControl)
        {
            _diagnosticPlayerIds.Add(playerControl.PlayerId);
        }

        var now = Time.realtimeSinceStartup;
        var elapsed = now - _diagnosticStartRealtime;
        if (elapsed < 2f)
        {
            return;
        }

        _diagnosticLogged = true;
        var rate = elapsed > 0f ? _diagnosticHandlerCalls / elapsed : 0f;
        var ids = _diagnosticPlayerIds.Count == 0
            ? "none"
            : string.Join(",", _diagnosticPlayerIds);

        TimerLog.LogInfo(
            $"Death Note timer diagnostic: handlers={_diagnosticHandlerCalls} over {elapsed:F2}s " +
            $"({rate:F1}/s), updatePlayerIds=[{ids}], localPlayerId=" +
            $"{(PlayerControl.LocalPlayer ? PlayerControl.LocalPlayer.PlayerId : byte.MaxValue)}, " +
            $"deltaTime={Time.deltaTime:F4}, fixedDeltaTime={Time.fixedDeltaTime:F4}, " +
            $"remaining={_realTimeCooldownRemaining:F2}s.");
    }

    public static void HandleHostResult(bool success)
    {
        try
        {
            CustomButtonSingleton<CondemnerDeathNoteButton>.Instance.ApplyHostResult(success);
        }
        catch
        {
            // HUD can already be closing when the network result arrives. The
            // authoritative Death Row state is synchronized independently.
        }
    }
}
