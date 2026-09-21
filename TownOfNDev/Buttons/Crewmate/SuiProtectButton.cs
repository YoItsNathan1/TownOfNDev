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
    private bool _awaitingHost;

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
        // A button instance can survive HUD/lobby transitions even though an
        // owner-result RPC was lost while the previous HUD was closing. Never
        // carry that transient request lock into a newly-created HUD button.
        _awaitingHost = false;

        base.CreateButton(parent);

        // The approved SUI artwork already contains the PROTECT label.
        // Keep Mira's native keybind/cooldown UI but hide duplicate button text.
        if (Button?.buttonLabelText != null)
        {
            Button.buttonLabelText.gameObject.SetActive(false);
        }

        // The approved sprite reads a little small inside Mira's native ability-button frame.
        // Scale only the artwork so the keybind and cooldown overlays keep their normal size.
        if (Button?.graphic != null)
        {
            Button.graphic.transform.localScale = new Vector3(1.12f, 1.12f, 1f);
        }
    }

    public override bool Enabled(RoleBehaviour? role)
    {
        // Use the role object Mira supplies directly. TownOfUsRoleButton also
        // checks GetRole<T>(), which is unnecessary here and can be stale during
        // HUD/meeting transitions for extension roles.
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

    public override bool CanUse()
    {
        var local = PlayerControl.LocalPlayer;
        return !_awaitingHost && local && SuiSystem.HasProtectionCapacity(local) && base.CanUse();
    }

    public override bool CanClick()
    {
        return !_awaitingHost && base.CanClick();
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

    public override void ResetCooldownAndOrEffect()
    {
        // Mira resets custom buttons at MeetingHud.Start and again when
        // ExileController re-enables gameplay. _awaitingHost is TownOfNDev
        // state, so Mira cannot clear it for us. If a result RPC arrived while
        // the HUD was closing (or was otherwise missed locally), keeping this
        // flag set would permanently disable Protect after the meeting.
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
        SuiSystem.RpcRequestProtect(PlayerControl.LocalPlayer, Target);
    }

    protected override void OnClick()
    {
        // Host-authoritative. Cooldown begins only after the host confirms that
        // the target is valid, in range, not already protected, and that SUI has
        // an available protection slot.
    }

    public void ApplyHostResult(bool success)
    {
        // Ignore a delayed result from a request that was abandoned by a
        // meeting/HUD transition. Otherwise an old round can consume the new
        // round's cooldown/state after recovery has already completed.
        if (!_awaitingHost)
        {
            return;
        }

        _awaitingHost = false;
        if (!success)
        {
            ResetTarget();
            return;
        }

        SetTimer(Cooldown);
        ResetTarget();
    }


    public void RecoverAfterMeeting()
    {
        Disabled = false;
        _awaitingHost = false;
        ResetTarget();
        EffectActive = false;
        SetTimerPaused(false);
        SetTimer(Cooldown);
    }

    public static void RecoverLocalAfterMeeting()
    {
        try
        {
            CustomButtonSingleton<SuiProtectButton>.Instance.RecoverAfterMeeting();
        }
        catch
        {
            // The lifecycle fallback creates/re-shows the button when the HUD is ready.
        }
    }


    public static void ResetLocalState()
    {
        try
        {
            var button = CustomButtonSingleton<SuiProtectButton>.Instance;
            button._awaitingHost = false;
            button.ResetTarget();

            // A new game always starts with the configured Protect cooldown, even
            // if the singleton survived a previous lobby/game transition.
            button.SetTimer(button.Cooldown);
        }
        catch
        {
            // The button may not have been constructed yet. Its InitialCooldown
            // will provide the same value when Mira creates it later.
        }
    }

    public static void HandleHostResult(bool success)
    {
        try
        {
            CustomButtonSingleton<SuiProtectButton>.Instance.ApplyHostResult(success);
        }
        catch
        {
            // The HUD may already be closing when the authoritative result arrives.
            // Protection state itself is synchronized independently via modifiers.
        }
    }
}
