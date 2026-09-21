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

public sealed class NullModifier : UniversalGameModifier, IWikiDiscoverable
{
    public override string IdPart => "NDevNull";
    public override string ModifierName => "Null";
    public override string IntroInfo => "Your vote secretly carries no voting power.";

    public override ModifierUiConfiguration Configuration => new(
        TownOfNDevColors.Null,
        TmpSpriteUtils.CreateSpriteAsset(
            TownOfNDevAssets.NullModifierIcon.LoadAsset(),
            "TownOfNDev.Modifier.Universal.Null",
            1.45f));
    public override LoadableAsset<Sprite> ModifierIcon => TownOfNDevAssets.NullModifierIcon;
    public override Color FreeplayFileColor => TownOfNDevColors.Null;
    public override ModifierFaction FactionType => ModifierFaction.UniversalPassive;

    public List<CustomButtonWikiDescription> Abilities { get; } = [];

    public override string GetDescription() =>
        "You can vote normally, but your vote secretly contributes zero to the final tally.";

    public string GetAdvancedDescription() =>
        "Your vote is submitted and displayed normally, including in the voting results, but it has a hidden weight of zero when the game decides who is ejected or whether the vote is tied." +
        MiscUtils.AppendOptionsText(GetType());

    public override int GetAssignmentChance() =>
        (int)OptionGroupSingleton<NullOptions>.Instance.AssignmentChance.Value;

    public override int GetAmountPerGame() =>
        (int)OptionGroupSingleton<NullOptions>.Instance.Amount;
}
