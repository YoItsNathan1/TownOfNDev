using MiraAPI.GameOptions;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using TownOfNDev.Assets;
using TownOfNDev.Options;
using TownOfNDev.Systems;
using TownOfUs.Modifiers;
using TownOfUs.Modifiers.Game;
using TownOfUs.Modules.Wiki;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfNDev.Modifiers.Universal;

public sealed class LaggyModifier : UniversalGameModifier, IWikiDiscoverable
{
    public override string IdPart => "NDevLaggy";
    public override string ModifierName => "Laggy";
    public override string IntroInfo => "Your movement occasionally stutters.";

    public override ModifierUiConfiguration Configuration => new(
        TownOfNDevColors.Laggy,
        TmpSpriteUtils.CreateSpriteAsset(
            TownOfNDevAssets.LaggyModifierIcon.LoadAsset(),
            "TownOfNDev.Modifier.Universal.Laggy",
            1.45f));

    public override LoadableAsset<Sprite> ModifierIcon => TownOfNDevAssets.LaggyModifierIcon;
    public override Color FreeplayFileColor => TownOfNDevColors.Laggy;
    public override ModifierFaction FactionType => ModifierFaction.UniversalPassive;

    public List<CustomButtonWikiDescription> Abilities { get; } = [];

    public override string GetDescription() => "Your movement occasionally stutters, briefly freezing you before you carry on.";

    public string GetAdvancedDescription() =>
        "Your movement occasionally stutters. Starting to move or making a sharp change of direction can briefly freeze you before movement resumes, making your character feel like it suddenly lagged." +
        MiscUtils.AppendOptionsText(GetType());

    public override int GetAssignmentChance() =>
        (int)OptionGroupSingleton<LaggyOptions>.Instance.AssignmentChance.Value;

    public override int GetAmountPerGame() =>
        (int)OptionGroupSingleton<LaggyOptions>.Instance.Amount;

    public override void OnActivate()
    {
        // TOU 1.7.2 records active game modifiers from the base activation path.
        // Keep that bookkeeping while still resetting the local stutter state.
        base.OnActivate();
        if (Player && Player.AmOwner)
        {
            LaggyController.Reset();
        }
    }

    public override void OnDeactivate()
    {
        if (Player && Player.AmOwner)
        {
            LaggyController.Reset();
        }
        base.OnDeactivate();
    }
}
