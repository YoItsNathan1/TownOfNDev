using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using TownOfNDev.Assets;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfNDev.Options;

public sealed class EyeMaskOptions : AbstractOptionGroup
{
    public override string GroupName => "Eye Mask (TownOfNDev)";
    public override MenuCategory ParentMenu => MenuCategory.Modifiers;
    public override uint GroupPriority => 20;

    public override Color GroupColor => TownOfNDevColors.EyeMask;

    public override OptionNotifConfiguration Configuration => new(
        TownOfNDevColors.EyeMask,
        TmpSpriteUtils.CreateSpriteAsset(
            TownOfNDevAssets.EyeMaskModifierIcon.LoadAsset(),
            "TownOfNDev.Modifier.Crewmate.EyeMask",
            1.45f));

    [ModdedNumberOption("Eye Mask Amount", 0, 15)]
    public float Amount { get; set; } = 0f;

    public ModdedNumberOption AssignmentChance { get; } =
        new("Eye Mask Assignment Chance", 50f, 0f, 100f, 10f, MiraNumberSuffixes.Percent);

    public ModdedNumberOption TriggerChance { get; } =
        new("Sleep Chance After Task", 100f, 50f, 100f, 10f, MiraNumberSuffixes.Percent);

    public ModdedNumberOption MinimumDelay { get; } =
        new("Minimum Sleep Delay", 1f, 1f, 10f, 1f, MiraNumberSuffixes.Seconds);

    public ModdedNumberOption MaximumDelay { get; } =
        new("Maximum Sleep Delay", 10f, 1f, 10f, 1f, MiraNumberSuffixes.Seconds);

    public ModdedNumberOption SleepDuration { get; } =
        new("Sleep Duration", 5f, 1f, 10f, 0.5f, MiraNumberSuffixes.Seconds);
}
