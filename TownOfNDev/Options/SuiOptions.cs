using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using TownOfNDev.Roles.Crewmate;

namespace TownOfNDev.Options;

public sealed class SuiOptions : AbstractRoleOptionGroup<SuiRole>
{
    public override string GroupName => "SUI (TownOfNDev)";

    public ModdedNumberOption ProtectCooldown { get; } =
        new("Protect Cooldown", 25f, 20f, 45f, 5f, MiraNumberSuffixes.Seconds, "0");

    public ModdedToggleOption LimitProtectedPlayers { get; } =
        new("Limit Protected Players", true);

    public ModdedNumberOption MaximumProtectedPlayers { get; } =
        new("Maximum Protected Players", 3f, 1f, 5f, 1f, MiraNumberSuffixes.None, "0")
        {
            Visible = () => OptionGroupSingleton<SuiOptions>.Instance.LimitProtectedPlayers.Value
        };
}
