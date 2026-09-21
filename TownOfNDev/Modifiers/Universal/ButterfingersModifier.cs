using MiraAPI.GameOptions;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using TownOfNDev.Assets;
using TownOfNDev.Options;
using TownOfUs.Modifiers;
using TownOfUs.Modifiers.Game;
using TownOfUs.Modules.Wiki;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfNDev.Modifiers.Universal;

public sealed class ButterfingersModifier : UniversalGameModifier, IWikiDiscoverable
{
    public override string IdPart => "NDevButterfingers";
    public override string ModifierName => "Butterfingers";
    public override string IntroInfo => "Sometimes you fumble an action.";

    public override ModifierUiConfiguration Configuration => new(
        TownOfNDevColors.Butterfingers,
        TmpSpriteUtils.CreateSpriteAsset(
            TownOfNDevAssets.ButterfingersModifierIcon.LoadAsset(),
            "TownOfNDev.Modifier.Universal.Butterfingers",
            1.45f));

    public override LoadableAsset<Sprite> ModifierIcon => TownOfNDevAssets.ButterfingersModifierIcon;
    public override Color FreeplayFileColor => TownOfNDevColors.Butterfingers;
    public override ModifierFaction FactionType => ModifierFaction.UniversalPassive;

    public List<CustomButtonWikiDescription> Abilities { get; } = [];

    public override string GetDescription() => "Sometimes an action slips at the worst moment, briefly delaying what you were trying to do.";

    public string GetAdvancedDescription() =>
        "Sometimes an action slips at the worst moment. When you fumble, the action is briefly delayed before it can go through, which can throw off your timing when you need it most." +
        MiscUtils.AppendOptionsText(GetType());

    public override int GetAssignmentChance() =>
        (int)OptionGroupSingleton<ButterfingersOptions>.Instance.AssignmentChance.Value;

    public override int GetAmountPerGame() =>
        (int)OptionGroupSingleton<ButterfingersOptions>.Instance.Amount;
}
