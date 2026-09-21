using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using TownOfNDev.Assets;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfNDev.Options;

public sealed class ButterfingersOptions : AbstractOptionGroup
{
    public override string GroupName => "Butterfingers (TownOfNDev)";
    public override MenuCategory ParentMenu => MenuCategory.Modifiers;
    public override uint GroupPriority => 21;

    public override Color GroupColor => TownOfNDevColors.Butterfingers;

    public override OptionNotifConfiguration Configuration => new(
        TownOfNDevColors.Butterfingers,
        TmpSpriteUtils.CreateSpriteAsset(
            TownOfNDevAssets.ButterfingersModifierIcon.LoadAsset(),
            "TownOfNDev.Modifier.Universal.Butterfingers",
            1.45f));

    [ModdedNumberOption("Butterfingers Amount", 0, 15)]
    public float Amount { get; set; } = 0f;

    public ModdedNumberOption AssignmentChance { get; } =
        new("Butterfingers Assignment Chance", 50f, 0f, 100f, 10f, MiraNumberSuffixes.Percent);

    public ModdedNumberOption FumbleChance { get; } =
        new("Fumble Chance", 25f, 10f, 50f, 5f, MiraNumberSuffixes.Percent);

    public ModdedNumberOption MinimumDelay { get; } =
        new("Minimum Fumble Delay", 0.5f, 0.25f, 1f, 0.25f, MiraNumberSuffixes.Seconds);

    public ModdedNumberOption MaximumDelay { get; } =
        new("Maximum Fumble Delay", 1.25f, 0.75f, 2f, 0.25f, MiraNumberSuffixes.Seconds);

    public ModdedNumberOption FumbleCooldown { get; } =
        new("Fumble Internal Cooldown", 5f, 3f, 15f, 1f, MiraNumberSuffixes.Seconds);

    [ModdedToggleOption("Can Fumble Kills")]
    public bool FumbleKills { get; set; } = true;

    [ModdedToggleOption("Can Fumble Abilities")]
    public bool FumbleAbilities { get; set; } = true;

    [ModdedToggleOption("Can Fumble Reports")]
    public bool FumbleReports { get; set; } = true;

    [ModdedToggleOption("Can Fumble Vents")]
    public bool FumbleVents { get; set; } = true;

    [ModdedToggleOption("Can Fumble Use / Tasks")]
    public bool FumbleUse { get; set; } = true;
}
