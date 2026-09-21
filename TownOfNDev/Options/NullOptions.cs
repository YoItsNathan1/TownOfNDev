using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using TownOfNDev.Assets;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfNDev.Options;

public sealed class NullOptions : AbstractOptionGroup
{
    public override string GroupName => "Null (TownOfNDev)";
    public override MenuCategory ParentMenu => MenuCategory.Modifiers;
    public override uint GroupPriority => 26;
    public override Color GroupColor => TownOfNDevColors.Null;

    public override OptionNotifConfiguration Configuration => new(
        TownOfNDevColors.Null,
        TmpSpriteUtils.CreateSpriteAsset(
            TownOfNDevAssets.NullModifierIcon.LoadAsset(),
            "TownOfNDev.Modifier.Universal.Null",
            1.45f));

    [ModdedNumberOption("Null Amount", 0, 15)]
    public float Amount { get; set; } = 0f;

    public ModdedNumberOption AssignmentChance { get; } =
        new("Null Assignment Chance", 50f, 0f, 100f, 10f, MiraNumberSuffixes.Percent);
}
