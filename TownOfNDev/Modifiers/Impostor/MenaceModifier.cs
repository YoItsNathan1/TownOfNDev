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

namespace TownOfNDev.Modifiers.Impostor;

public sealed class MenaceModifier : TouGameModifier, IWikiDiscoverable
{
    public override string IdPart => "NDevMenace";
    public override string ModifierName => "Menace";
    public override string IntroInfo => "Your kill cooldown is reduced.";

    public override ModifierUiConfiguration Configuration => new(
        TownOfNDevColors.Menace,
        TmpSpriteUtils.CreateSpriteAsset(
            TownOfNDevAssets.MenaceModifierIcon.LoadAsset(),
            "TownOfNDev.Modifier.Impostor.Menace",
            1.45f));
    public override LoadableAsset<Sprite> ModifierIcon => TownOfNDevAssets.MenaceModifierIcon;
    public override Color FreeplayFileColor => TownOfNDevColors.Menace;
    public override ModifierFaction FactionType => ModifierFaction.ImpostorPassive;

    public List<CustomButtonWikiDescription> Abilities { get; } = [];

    public override string GetDescription()
    {
        var reduction = OptionGroupSingleton<MenaceOptions>.Instance.KillCooldownReduction.Value;
        return $"Your normal kill cooldown is reduced by {reduction:0}%.";
    }

    public string GetAdvancedDescription() =>
        "Your kill cooldown is shortened by a percentage of the cooldown you would otherwise receive, so it scales cleanly with the lobby's current kill cooldown and compatible Town of Us cooldown modifiers." +
        MiscUtils.AppendOptionsText(GetType());

    public override int GetAssignmentChance() =>
        (int)OptionGroupSingleton<MenaceOptions>.Instance.AssignmentChance.Value;

    public override int GetAmountPerGame() =>
        (int)OptionGroupSingleton<MenaceOptions>.Instance.Amount;

    public override bool IsModifierValidOn(RoleBehaviour role)
    {
        return base.IsModifierValidOn(role) && role.IsImpostor();
    }
}
