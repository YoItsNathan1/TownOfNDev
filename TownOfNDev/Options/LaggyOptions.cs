using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using TownOfNDev.Assets;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfNDev.Options;

public sealed class LaggyOptions : AbstractOptionGroup
{
    public override string GroupName => "Laggy (TownOfNDev)";
    public override MenuCategory ParentMenu => MenuCategory.Modifiers;
    public override uint GroupPriority => 22;

    public override Color GroupColor => TownOfNDevColors.Laggy;

    public override OptionNotifConfiguration Configuration => new(
        TownOfNDevColors.Laggy,
        TmpSpriteUtils.CreateSpriteAsset(
            TownOfNDevAssets.LaggyModifierIcon.LoadAsset(),
            "TownOfNDev.Modifier.Universal.Laggy",
            1.45f));

    [ModdedNumberOption("Laggy Amount", 0, 15)]
    public float Amount { get; set; } = 0f;

    public ModdedNumberOption AssignmentChance { get; } =
        new("Laggy Assignment Chance", 50f, 0f, 100f, 10f, MiraNumberSuffixes.Percent);

    public ModdedNumberOption TriggerChance { get; } =
        new("Stutter Trigger Chance", 60f, 10f, 100f, 10f, MiraNumberSuffixes.Percent);

    public ModdedNumberOption MinimumInterval { get; } =
        new("Minimum Stutter Check Interval", 8f, 3f, 30f, 1f, MiraNumberSuffixes.Seconds);

    public ModdedNumberOption MaximumInterval { get; } =
        new("Maximum Stutter Check Interval", 15f, 5f, 45f, 1f, MiraNumberSuffixes.Seconds);

    public ModdedNumberOption StutterDuration { get; } =
        new("Movement Stutter Duration", 0.4f, 0.2f, 0.6f, 0.05f, MiraNumberSuffixes.Seconds);

    [ModdedToggleOption("Show Lag Afterimage")]
    public bool ShowAfterimage { get; set; } = true;
}
