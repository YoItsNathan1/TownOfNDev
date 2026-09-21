using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
using TownOfNDev.Roles.Crewmate;

namespace TownOfNDev.Options;

public sealed class TankOptions : AbstractRoleOptionGroup<TankRole>
{
    public override string GroupName => "Tank (TownOfNDev)";

    public ModdedToggleOption CanSeeWhoTriedToKill { get; } =
        new("Can See Who Tried To Kill", true);
}
