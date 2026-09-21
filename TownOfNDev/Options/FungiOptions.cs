using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using TownOfNDev.Roles.Neutral;

namespace TownOfNDev.Options;

public enum FungiWinMode
{
    IndependentMustSurvive,
    AdditionalWinnerDeathAllowed
}

public sealed class FungiOptions : AbstractRoleOptionGroup<FungiRole>
{
    public override string GroupName => "Fungi (TownOfNDev)";

    public ModdedNumberOption InfectCooldown { get; } =
        new("Infect Cooldown", 20f, 10f, 45f, 2.5f, MiraNumberSuffixes.Seconds, "0.0");

    public ModdedNumberOption RequiredInfectionPercentage { get; } =
        new("Required Infection Percentage", 75f, 50f, 100f, 5f, MiraNumberSuffixes.Percent);

    public ModdedToggleOption RestoreInfectionAfterRevival { get; } =
        new("Restore Infection After Revival", true);

    public ModdedEnumOption<FungiWinMode> WinMode { get; } =
        new(
            "Fungi Win Mode",
            FungiWinMode.IndependentMustSurvive,
            ["Independent - Must Survive", "Additional Winner - Death Allowed"]);
}
