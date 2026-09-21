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

public sealed class SoulhandlerModifier : UniversalGameModifier, IWikiDiscoverable
{
    public override string IdPart => "NDevSoulhandler";
    public override string ModifierName => "Soulhandler";
    public override string IntroInfo => "The dead reveal which side they belonged to.";

    public override ModifierUiConfiguration Configuration => new(
        TownOfNDevColors.Soulhandler,
        TmpSpriteUtils.CreateSpriteAsset(
            TownOfNDevAssets.SoulhandlerModifierIcon.LoadAsset(),
            "TownOfNDev.Modifier.Universal.Soulhandler",
            1.45f));
    public override LoadableAsset<Sprite> ModifierIcon => TownOfNDevAssets.SoulhandlerModifierIcon;
    public override Color FreeplayFileColor => TownOfNDevColors.Soulhandler;
    public override ModifierFaction FactionType => ModifierFaction.UniversalVisibility;

    public List<CustomButtonWikiDescription> Abilities { get; } = [];

    public override string GetDescription() =>
        "During meetings, you can see the faction of players who have died.";

    public string GetAdvancedDescription() =>
        "Dead players are privately labelled as Crewmate, Impostor, or Neutral on your meeting screen. The label reveals their faction, not their exact role." +
        MiscUtils.AppendOptionsText(GetType());

    public override int GetAssignmentChance() =>
        (int)OptionGroupSingleton<SoulhandlerOptions>.Instance.AssignmentChance.Value;

    public override int GetAmountPerGame() =>
        (int)OptionGroupSingleton<SoulhandlerOptions>.Instance.Amount;
}
