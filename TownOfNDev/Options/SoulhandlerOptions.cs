using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using TownOfNDev.Assets;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfNDev.Options;

public sealed class SoulhandlerOptions : AbstractOptionGroup
{
    public override string GroupName => "Soulhandler (TownOfNDev)";
    public override MenuCategory ParentMenu => MenuCategory.Modifiers;
    public override uint GroupPriority => 24;
    public override Color GroupColor => TownOfNDevColors.Soulhandler;

    public override OptionNotifConfiguration Configuration => new(
        TownOfNDevColors.Soulhandler,
        TmpSpriteUtils.CreateSpriteAsset(
            TownOfNDevAssets.SoulhandlerModifierIcon.LoadAsset(),
            "TownOfNDev.Modifier.Universal.Soulhandler",
            1.45f));

    [ModdedNumberOption("Soulhandler Amount", 0, 15)]
    public float Amount { get; set; } = 0f;

    public ModdedNumberOption AssignmentChance { get; } =
        new("Soulhandler Assignment Chance", 50f, 0f, 100f, 10f, MiraNumberSuffixes.Percent);
}
