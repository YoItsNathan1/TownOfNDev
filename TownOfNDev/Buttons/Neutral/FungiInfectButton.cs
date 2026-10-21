using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Keybinds;
using MiraAPI.Utilities.Assets;
using TownOfNDev.Assets;
using TownOfNDev.Options;
using TownOfNDev.Roles.Neutral;
using TownOfNDev.Systems;
using TownOfUs.Buttons;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfNDev.Buttons.Neutral;

public sealed class FungiInfectButton : TownOfUsRoleButton<FungiRole, PlayerControl>
{
    private readonly HostAbilityRequest _request = new(nameof(FungiInfectButton));

    public override string Name => "Infect";
    public override BaseKeybind Keybind => Keybinds.SecondaryAction;
    public override Color TextOutlineColor => TownOfNDevColors.Fungi;
    public override float InitialCooldown => Cooldown;
    public override float Cooldown => Mathf.Clamp(
        OptionGroupSingleton<FungiOptions>.Instance.InfectCooldown.Value + MapCooldown,
        10f,
        45f);
    public override LoadableAsset<Sprite> Sprite => TownOfNDevAssets.FungiInfectButton;

    public override void CreateButton(Transform parent)
    {
        _request.Cancel();
        base.CreateButton(parent);

        // The current artwork already contains the INFECT label.
        if (Button?.buttonLabelText != null)
        {
            Button.buttonLabelText.gameObject.SetActive(false);
        }
    }

    public override bool Enabled(RoleBehaviour? role)
    {
        var local = PlayerControl.LocalPlayer;
        return !Disabled && local &&
               (role is FungiRole || FungiInfectionSystem.IsFungi(local));
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
                !FungiInfectionSystem.IsFungi(player) &&
                !FungiInfectionSystem.IsInfected(player));
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
        FungiInfectionSystem.RpcRequestInfect(local, Target, requestId);
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
            CustomButtonSingleton<FungiInfectButton>.Instance.ApplyHostResult(requestId, success);
        }
        catch (System.Exception ex)
        {
            HostAbilityRequest.LogResultFailure(nameof(FungiInfectButton), ex);
        }
    }
}
