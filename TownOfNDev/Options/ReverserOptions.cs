using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using TownOfNDev.Roles.Impostor;

namespace TownOfNDev.Options;

public sealed class ReverserOptions : AbstractRoleOptionGroup<ReverserRole>
{
    public override string GroupName => "Reverser (TownOfNDev)";

    public ModdedNumberOption NumberOfReverses { get; } =
        new("Number of Reverses", 10f, 1f, 15f, 1f, MiraNumberSuffixes.None, "0");

    public ModdedNumberOption ReverseCooldown { get; } =
        new("Reverse Cooldown", 30f, 2.5f, 120f, 2.5f, MiraNumberSuffixes.Seconds, "0.0");

    public ModdedNumberOption ReverseDuration { get; } =
        new("Reverse Duration", 15f, 2.5f, 120f, 2.5f, MiraNumberSuffixes.Seconds, "0.0");
}
