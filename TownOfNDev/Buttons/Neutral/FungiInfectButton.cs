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
    private bool _awaitingHost;

    public override string Name => "Infect";
    public override BaseKeybind Keybind => Keybinds.SecondaryAction;
    public override Color TextOutlineColor => TownOfNDevColors.Fungi;
    public override float InitialCooldown => Cooldown;
    public override float Cooldown => Mathf.Clamp(
        OptionGroupSingleton<FungiOptions>.Instance.InfectCooldown.Value + MapCooldown,
        10f,
        45f);
    public override LoadableAsset<Sprite> Sprite => TownOfNDevAssets.FungiInfectButton;

    public override bool Enabled(RoleBehaviour? role)
    {
        var local = PlayerControl.LocalPlayer;
        return !Disabled && local &&
               (role is FungiRole || FungiInfectionSystem.IsFungi(local));
    }

    public override void CreateButton(Transform parent)
    {
        // Do not carry a stale host-result wait into a recreated HUD button.
        _awaitingHost = false;

        base.CreateButton(parent);

        // The approved TownOfNDev sprite already contains the INFECT label. Keep the
        // native cooldown text, but hide Mira's duplicate label.
        if (Button?.buttonLabelText != null)
        {
            Button.buttonLabelText.gameObject.SetActive(false);
        }
    }

    public override PlayerControl? GetTarget()
    {
        return PlayerControl.LocalPlayer.GetClosestLivingPlayer(
            true,
            Distance,
            predicate: player =>
                player != null &&
                !FungiInfectionSystem.IsFungi(player) &&
                !FungiInfectionSystem.IsInfected(player));
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
        // Mira resets custom buttons at both meeting boundaries, but the
        // _awaitingHost flag is extension-owned state. Clear it with the native
        // button reset so a missed/late result cannot brick Infect next round.
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

        // Fungi is still a role ability, so Butterfingers may delay it when the
        // universal modifier's role-ability option is enabled. The replay calls
        // back into this method and revalidates range/target/cooldown before
        // requesting the infection from the host.
        if (ButterfingersController.TryFumble(FumbleActionKind.Ability, ClickHandler))
        {
            return;
        }

        _awaitingHost = true;
        FungiInfectionSystem.RpcRequestInfect(PlayerControl.LocalPlayer, Target);
    }

    protected override void OnClick()
    {
        // ClickHandler is host-authoritative for Fungi; cooldown is only consumed
        // after a successful result is returned by the host.
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
            CustomButtonSingleton<FungiInfectButton>.Instance.RecoverAfterMeeting();
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
            CustomButtonSingleton<FungiInfectButton>.Instance.ApplyHostResult(success);
        }
        catch
        {
            // The HUD may already be closing when a result arrives. Infection state
            // is still authoritative; only the local visual cooldown update is skipped.
        }
    }
}
