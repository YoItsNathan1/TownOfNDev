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
    private bool _awaitingHost;

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
        // Do not let a transient owner-result wait survive a HUD recreation.
        _awaitingHost = false;

        base.CreateButton(parent);

        // The approved TownOfNDev artwork already contains the DUST label.
        // Keep Mira's native keybind/uses UI but hide the duplicate text label.
        if (Button?.buttonLabelText != null)
        {
            Button.buttonLabelText.gameObject.SetActive(false);
        }
    }

    public override bool Enabled(RoleBehaviour? role)
    {
        // Use the role object Mira supplies directly. TownOfUsRoleButton also
        // checks GetRole<T>(), which is unnecessary here and can be stale during
        // HUD/meeting transitions for extension roles.
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
        return !_awaitingHost && base.CanUse();
    }

    public override bool CanClick()
    {
        return !_awaitingHost && base.CanClick();
    }

    public override void ResetCooldownAndOrEffect()
    {
        // Mira invokes this on meeting start and when gameplay resumes. The
        // framework resets Timer/Effect/PerRound uses, but it has no knowledge
        // of TownOfNDev's host-result lock. Clear it at the same lifecycle
        // boundary so Dust cannot remain disabled after a meeting.
        _awaitingHost = false;
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

        if (ButterfingersController.TryFumble(FumbleActionKind.Ability, ClickHandler))
        {
            return;
        }

        _awaitingHost = true;
        TracerSystem.RpcRequestDust(PlayerControl.LocalPlayer, Target);
    }

    protected override void OnClick()
    {
        // Host-authoritative. The per-round use is consumed only after the host
        // confirms the target/range/state and applies synchronized Dust.
    }

    public void ApplyHostResult(bool success)
    {
        if (!_awaitingHost)
        {
            return;
        }

        _awaitingHost = false;
        if (!success)
        {
            return;
        }

        SetUses(Mathf.Max(0, UsesLeft - 1));
        SetTimer(0f);
        ResetTarget();
    }

    public void RecoverAfterMeeting()
    {
        Disabled = false;
        _awaitingHost = false;
        ResetTarget();
        EffectActive = false;
        SetTimerPaused(false);
        SetTimer(0f);
        SetUses(MaxUses);
    }

    public static void RecoverLocalAfterMeeting()
    {
        try
        {
            CustomButtonSingleton<TracerDustButton>.Instance.RecoverAfterMeeting();
        }
        catch
        {
            // The lifecycle fallback creates/re-shows the button when the HUD is ready.
        }
    }


    public static void HandleHostResult(bool success)
    {
        try
        {
            CustomButtonSingleton<TracerDustButton>.Instance.ApplyHostResult(success);
        }
        catch
        {
            // The HUD can already be closing when a result arrives. Dust state is
            // still network-synchronized independently of the local button UI.
        }
    }
}
