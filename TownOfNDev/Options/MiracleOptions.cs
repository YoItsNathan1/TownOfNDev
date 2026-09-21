using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using TownOfNDev.Assets;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfNDev.Options;

public sealed class MiracleOptions : AbstractOptionGroup
{
    public override string GroupName => "Miracle (TownOfNDev)";
    public override MenuCategory ParentMenu => MenuCategory.Modifiers;
    public override uint GroupPriority => 25;
    public override Color GroupColor => TownOfNDevColors.Miracle;

    public override OptionNotifConfiguration Configuration => new(
        TownOfNDevColors.Miracle,
        TmpSpriteUtils.CreateSpriteAsset(
            TownOfNDevAssets.MiracleModifierIcon.LoadAsset(),
            "TownOfNDev.Modifier.Universal.Miracle",
            1.45f));

    [ModdedNumberOption("Miracle Amount", 0, 15)]
    public float Amount { get; set; } = 0f;

    public ModdedNumberOption AssignmentChance { get; } =
        new("Miracle Assignment Chance", 50f, 0f, 100f, 10f, MiraNumberSuffixes.Percent);

    public ModdedNumberOption SurvivalChance { get; } =
        new("Kill Survival Chance", 50f, 5f, 100f, 5f, MiraNumberSuffixes.Percent);
}
