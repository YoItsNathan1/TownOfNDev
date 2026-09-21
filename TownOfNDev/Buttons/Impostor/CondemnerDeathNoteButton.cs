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
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfNDev.Buttons.Impostor;

public sealed class CondemnerDeathNoteButton : TownOfUsRoleButton<CondemnerRole, PlayerControl>
{
    private bool _awaitingHost;

    public override string Name => "Death Note";
    public override BaseKeybind Keybind => Keybinds.SecondaryAction;
    public override Color TextOutlineColor => TownOfUsColors.Impostor;
    public override float InitialCooldown => Cooldown;
    public override float Cooldown => OptionGroupSingleton<CondemnerOptions>.Instance.DeathNoteCooldown.Value;
    // TownOfUsButton defaults ZeroIsInfinite to false. Death Note uses 0 to
    // represent unlimited uses when the host disables the per-round limit, so
    // explicitly opt into Mira's zero-is-infinite semantics.
    public override bool ZeroIsInfinite { get; set; } = true;
    public override int MaxUses => OptionGroupSingleton<CondemnerOptions>.Instance.LimitUsesPerRound.Value
        ? Mathf.Clamp((int)OptionGroupSingleton<CondemnerOptions>.Instance.MaxUsesPerRound.Value, 1, 5)
        : 0;
    public override ButtonUsesMode UsesMode => ButtonUsesMode.PerRound;
    public override LoadableAsset<Sprite> Sprite => TownOfNDevAssets.CondemnerDeathNoteButton;

    public override void CreateButton(Transform parent)
    {
        // Do not carry a stale host-result wait into a recreated HUD button.
        _awaitingHost = false;

        base.CreateButton(parent);

        // The approved TownOfNDev button artwork already contains the DEATH NOTE
        // label in the desired Among Us-style treatment, so avoid a duplicate label.
        if (Button?.buttonLabelText != null)
        {
            Button.buttonLabelText.gameObject.SetActive(false);
        }
    }

    public override void ResetCooldownAndOrEffect()
    {
        // Mira calls this at MeetingHud.Start and again when gameplay resumes.
        // Clear TownOfNDev's transient request lock at the same boundary.
        _awaitingHost = false;
        ResetTarget();

        base.ResetCooldownAndOrEffect();
        SetTimerPaused(false);
    }

    public override bool Enabled(RoleBehaviour? role)
    {
        // Use the role object Mira supplies directly. TownOfUsRoleButton also
        // checks GetRole<T>(), which is unnecessary here and can be stale during
        // HUD/meeting transitions for extension roles.
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

        if (OptionGroupSingleton<CondemnerOptions>.Instance.LimitUsesPerRound.Value)
        {
            SetUses(Mathf.Max(0, UsesLeft - 1));
        }

        SetTimer(Cooldown);

        var local = PlayerControl.LocalPlayer;
        if (local && local.Data != null && !local.Data.IsDead)
        {
            local.SetKillTimer(local.GetKillCooldown());
        }

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

        if (LimitedUses)
        {
            SetUses(MaxUses);
        }
    }

    public static void RecoverLocalAfterMeeting()
    {
        try
        {
            CustomButtonSingleton<CondemnerDeathNoteButton>.Instance.RecoverAfterMeeting();
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
            CustomButtonSingleton<CondemnerDeathNoteButton>.Instance.ApplyHostResult(success);
        }
        catch
        {
            // HUD can already be closing when the network result arrives. The
            // authoritative Death Row state is synchronized independently.
        }
    }
}
