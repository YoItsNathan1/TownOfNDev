using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Keybinds;
using MiraAPI.Utilities.Assets;
using Reactor.Utilities.Extensions;
using TownOfNDev.Assets;
using TownOfNDev.Options;
using TownOfNDev.Roles.Crewmate;
using TownOfNDev.Systems;
using TownOfUs.Buttons;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfNDev.Buttons.Crewmate;

public sealed class SuiProtectButton : TownOfUsRoleButton<SuiRole, PlayerControl>
{
    private readonly HostAbilityRequest _request = new(nameof(SuiProtectButton));

    public override string Name => "Protect";
    public override BaseKeybind Keybind => Keybinds.SecondaryAction;
    public override Color TextOutlineColor => TownOfNDevColors.Sui;
    public override float InitialCooldown => Cooldown;
    public override float Cooldown => Mathf.Clamp(
        OptionGroupSingleton<SuiOptions>.Instance.ProtectCooldown.Value,
        20f,
        45f);
    public override LoadableAsset<Sprite> Sprite => TownOfNDevAssets.SuiProtectButton;

    public override void CreateButton(Transform parent)
    {
        _request.Cancel();
        base.CreateButton(parent);

        // The current artwork already contains the PROTECT label. Keep Mira's
        // keybind/cooldown UI while avoiding a second text label over the icon.
        if (Button?.buttonLabelText != null)
        {
            Button.buttonLabelText.gameObject.SetActive(false);
        }

        if (Button?.graphic != null)
        {
            Button.graphic.transform.localScale = new Vector3(1.12f, 1.12f, 1f);
        }
    }

    public override bool Enabled(RoleBehaviour? role)
    {
        // Use Mira's supplied role instead of a second GetRole<T>() lookup
        // during the HUD transition back from a meeting.
        return !Disabled && role is SuiRole;
    }

    public override PlayerControl? GetTarget()
    {
        var local = PlayerControl.LocalPlayer;
        if (!local || local.Data == null || !SuiSystem.HasProtectionCapacity(local))
        {
            return null;
        }

        return local.GetClosestLivingPlayer(
            true,
            Distance,
            predicate: player =>
                player != null &&
                player != local &&
                !SuiSystem.IsProtected(player));
    }

    public override void SetOutline(bool active)
    {
        if (Target == null || PlayerControl.LocalPlayer.HasDied())
        {
            return;
        }

        var body = Target.cosmetics?.currentBodySprite?.BodySprite;
        if (body is null || !body)
        {
            return;
        }

        var local = PlayerControl.LocalPlayer;
        var outline = active
            ? (SuiSystem.IsRevealedTo(Target, local.PlayerId) ? Color.red : TownOfNDevColors.Sui)
            : (Color?)null;
        body.SetOutline(outline);
    }

    public override bool CanUse()
    {
        var local = PlayerControl.LocalPlayer;
        return !_request.IsPending && local &&
               SuiSystem.HasProtectionCapacity(local) && base.CanUse();
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
        SuiSystem.RpcRequestProtect(local, Target, requestId);
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

        SetTimer(Cooldown);
        ResetTarget();
    }

    public static void HandleHostResult(int requestId, bool success)
    {
        // A result may arrive while the HUD is being destroyed or a role changes.
        // The authoritative mechanic itself is separately synced by its system.
        try
        {
            CustomButtonSingleton<SuiProtectButton>.Instance.ApplyHostResult(requestId, success);
        }
        catch (System.Exception ex)
        {
            HostAbilityRequest.LogResultFailure(nameof(SuiProtectButton), ex);
        }
    }
}
