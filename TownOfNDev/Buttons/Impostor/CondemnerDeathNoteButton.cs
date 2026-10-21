using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Keybinds;
using MiraAPI.Modifiers;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using TownOfNDev.Assets;
using TownOfNDev.Options;
using TownOfNDev.Roles.Impostor;
using TownOfNDev.Systems;
using TownOfUs;
using TownOfUs.Buttons;
using TownOfUs.Modifiers.Crewmate;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfNDev.Buttons.Impostor;

public sealed class CondemnerDeathNoteButton : TownOfUsRoleButton<CondemnerRole, PlayerControl>
{
    private readonly HostAbilityRequest _request = new(nameof(CondemnerDeathNoteButton));

    public override string Name => "Death Note";
    public override BaseKeybind Keybind => Keybinds.SecondaryAction;
    public override Color TextOutlineColor => TownOfUsColors.Impostor;
    public override float InitialCooldown => Cooldown;
    public override float Cooldown => OptionGroupSingleton<CondemnerOptions>.Instance.DeathNoteCooldown.Value;
    public override bool ZeroIsInfinite { get; set; } = true;
    public override int MaxUses => OptionGroupSingleton<CondemnerOptions>.Instance.LimitUsesPerRound.Value
        ? Mathf.Clamp((int)OptionGroupSingleton<CondemnerOptions>.Instance.MaxUsesPerRound.Value, 1, 5)
        : 0;
    public override ButtonUsesMode UsesMode => ButtonUsesMode.PerRound;
    public override LoadableAsset<Sprite> Sprite => TownOfNDevAssets.CondemnerDeathNoteButton;

    public override void CreateButton(Transform parent)
    {
        _request.Cancel();
        base.CreateButton(parent);

        // The current artwork already contains the DEATH NOTE label.
        if (Button?.buttonLabelText != null)
        {
            Button.buttonLabelText.gameObject.SetActive(false);
        }
    }

    public override bool Enabled(RoleBehaviour? role)
    {
        // Use Mira's supplied role instead of a second GetRole<T>() lookup
        // during the HUD transition back from a meeting.
        return !Disabled && role is CondemnerRole;
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
                !CondemnerSystem.IsCondemned(player) &&
                !player.HasModifier<WardenFortifiedModifier>());
    }

    public override bool CanUse()
    {
        return !_request.IsPending && base.CanUse();
    }

    public override bool CanClick()
    {
        return !_request.IsPending && base.CanClick();
    }

    public override void ResetCooldownAndOrEffect()
    {
        // Mira invokes this on meeting start and after exile. Invalidate any
        // outstanding response so it cannot consume the next round's ability.
        _request.Cancel();
        ResetTarget();
        base.ResetCooldownAndOrEffect();
        SetTimerPaused(false);
    }

    public override void ClickHandler()
    {
        if (!CanClick() || Target == null)
        {
            return;
        }

        var local = PlayerControl.LocalPlayer;
        if (!local || local.Data == null || local.Data.IsDead || local.Data.Disconnected)
        {
            return;
        }

        // The Butterfingers MiraButtonClickEvent intercepts the click pipeline
        // before this handler; do not run TryFumble a second time here.
        var requestId = _request.Begin();
        CondemnerSystem.RpcRequestDeathNote(local, Target, requestId);
    }

    protected override void OnClick()
    {
        // Intentionally unused: the inherited TOU handler would consume a use
        // before the host validates the request.
    }

    private void ApplyHostResult(int requestId, bool success)
    {
        if (!_request.TryComplete(requestId))
        {
            return;
        }

        if (!success)
        {
            ResetTarget();
            return;
        }

        if (OptionGroupSingleton<CondemnerOptions>.Instance.LimitUsesPerRound.Value)
        {
            SetUses(Mathf.Max(0, UsesLeft - 1));
        }

        SetTimer(Cooldown);
        var local = PlayerControl.LocalPlayer;
        if (local && local.Data != null && !local.Data.IsDead && !local.Data.Disconnected &&
            local.Data.Role is CondemnerRole && !MeetingHud.Instance && !ExileController.Instance)
        {
            local.SetKillTimer(local.GetKillCooldown());
        }

        ResetTarget();
    }

    public static void HandleHostResult(int requestId, bool success)
    {
        // A result may arrive while the HUD is being destroyed or a role changes.
        // The authoritative mechanic itself is separately synced by its system.
        try
        {
            CustomButtonSingleton<CondemnerDeathNoteButton>.Instance.ApplyHostResult(requestId, success);
        }
        catch (System.Exception ex)
        {
            HostAbilityRequest.LogResultFailure(nameof(CondemnerDeathNoteButton), ex);
        }
    }
}
