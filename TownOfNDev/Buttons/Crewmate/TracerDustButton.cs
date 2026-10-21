using MiraAPI.Hud;
using MiraAPI.Keybinds;
using MiraAPI.Utilities.Assets;
using TownOfNDev.Assets;
using TownOfNDev.Roles.Crewmate;
using TownOfNDev.Systems;
using TownOfUs.Buttons;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfNDev.Buttons.Crewmate;

public sealed class TracerDustButton : TownOfUsRoleButton<TracerRole, PlayerControl>
{
    private readonly HostAbilityRequest _request = new(nameof(TracerDustButton));

    public override string Name => "Dust";
    public override BaseKeybind Keybind => Keybinds.SecondaryAction;
    public override Color TextOutlineColor => TownOfNDevColors.Tracer;
    public override float InitialCooldown => 0f;
    public override float Cooldown => 0f;
    public override int MaxUses => 1;
    public override ButtonUsesMode UsesMode => ButtonUsesMode.PerRound;
    public override LoadableAsset<Sprite> Sprite => TownOfNDevAssets.TracerDustButton;

    public override void CreateButton(Transform parent)
    {
        _request.Cancel();
        base.CreateButton(parent);

        // The current artwork already contains the DUST label.
        if (Button?.buttonLabelText != null)
        {
            Button.buttonLabelText.gameObject.SetActive(false);
        }
    }

    public override bool Enabled(RoleBehaviour? role)
    {
        // Use Mira's supplied role instead of a second GetRole<T>() lookup
        // during the HUD transition back from a meeting.
        return !Disabled && role is TracerRole;
    }

    public override PlayerControl? GetTarget()
    {
        var local = PlayerControl.LocalPlayer;
        if (!local)
        {
            return null;
        }

        return local.GetClosestLivingPlayer(
            true,
            Distance,
            predicate: player =>
                player != null &&
                player != local &&
                !TracerSystem.IsDustedBy(player, local.PlayerId));
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
        TracerSystem.RpcRequestDust(local, Target, requestId);
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

        SetUses(Mathf.Max(0, UsesLeft - 1));
        ResetTarget();
    }

    public static void HandleHostResult(int requestId, bool success)
    {
        // A result may arrive while the HUD is being destroyed or a role changes.
        // The authoritative mechanic itself is separately synced by its system.
        try
        {
            CustomButtonSingleton<TracerDustButton>.Instance.ApplyHostResult(requestId, success);
        }
        catch (System.Exception ex)
        {
            HostAbilityRequest.LogResultFailure(nameof(TracerDustButton), ex);
        }
    }
}
