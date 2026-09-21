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
        _awaitingHost = false;
        if (!success)
        {
            return;
        }

        SetTimer(Cooldown);
        ResetTarget();
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
