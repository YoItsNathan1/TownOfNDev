using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Keybinds;
using MiraAPI.Modifiers;
using MiraAPI.Utilities.Assets;
using TownOfNDev.Assets;
using TownOfNDev.Modifiers.Impostor;
using TownOfNDev.Options;
using TownOfNDev.Roles.Impostor;
using TownOfUs;
using TownOfUs.Buttons;
using UnityEngine;

namespace TownOfNDev.Buttons.Impostor;

public sealed class ReverserAlertButton : TownOfUsRoleButton<ReverserRole>
{
    public override string Name => "Alert";
    public override BaseKeybind Keybind => Keybinds.SecondaryAction;
    public override Color TextOutlineColor => TownOfUsColors.Impostor;
    public override float InitialCooldown => Cooldown;
    public override float Cooldown =>
        OptionGroupSingleton<ReverserOptions>.Instance.ReverseCooldown.Value;
    public override float EffectDuration =>
        OptionGroupSingleton<ReverserOptions>.Instance.ReverseDuration.Value;
    public override int MaxUses =>
        Mathf.Clamp((int)OptionGroupSingleton<ReverserOptions>.Instance.NumberOfReverses.Value, 1, 15);
    public override ButtonUsesMode UsesMode => ButtonUsesMode.PerGame;
    public override LoadableAsset<Sprite> Sprite => TownOfNDevAssets.ReverserAlertButton;

    public override void CreateButton(Transform parent)
    {
        base.CreateButton(parent);

        if (Button?.graphic != null)
        {
            Button.graphic.transform.localScale = new Vector3(0.92f, 0.92f, 1f);
        }
    }

    protected override void OnClick()
    {
        var local = PlayerControl.LocalPlayer;
        if (!local || local.Data == null || local.Data.IsDead || local.Data.Disconnected ||
            local.Data.Role is not ReverserRole)
        {
            return;
        }

        local.RpcAddModifier<ReverserAlertModifier>();
        OverrideName("REVERSING");
    }

    public override void OnEffectEnd()
    {
        OverrideName("ALERT");
    }
}
