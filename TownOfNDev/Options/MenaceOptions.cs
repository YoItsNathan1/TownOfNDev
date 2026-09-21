using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using TownOfNDev.Assets;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfNDev.Options;

public sealed class MenaceOptions : AbstractOptionGroup
{
    public override string GroupName => "Menace (TownOfNDev)";
    public override MenuCategory ParentMenu => MenuCategory.Modifiers;
    public override uint GroupPriority => 23;
    public override Color GroupColor => TownOfNDevColors.Menace;

    public override OptionNotifConfiguration Configuration => new(
        TownOfNDevColors.Menace,
        TmpSpriteUtils.CreateSpriteAsset(
            TownOfNDevAssets.MenaceModifierIcon.LoadAsset(),
            "TownOfNDev.Modifier.Impostor.Menace",
            1.45f));

    [ModdedNumberOption("Menace Amount", 0, 15)]
    public float Amount { get; set; } = 0f;

    public ModdedNumberOption AssignmentChance { get; } =
        new("Menace Assignment Chance", 50f, 0f, 100f, 10f, MiraNumberSuffixes.Percent);

    public ModdedNumberOption KillCooldownReduction { get; } =
        new("Kill Cooldown Reduction", 25f, 15f, 50f, 5f, MiraNumberSuffixes.Percent);
}
