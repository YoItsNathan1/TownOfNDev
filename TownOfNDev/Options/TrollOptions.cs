using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
using TownOfNDev.Roles.Neutral;

namespace TownOfNDev.Options;

public enum TrollAfterWinType
{
    ContinuesGame,
    EndsGame
}

public sealed class TrollOptions : AbstractRoleOptionGroup<TrollRole>
{
    public override string GroupName => "Troll (TownOfNDev)";

    public ModdedToggleOption CanUseButton { get; } =
        new("Can Use Button", true);

    public ModdedToggleOption ImpostorVision { get; } =
        new("Has Impostor Vision", true);

    public ModdedToggleOption IndirectKillsCount { get; } =
        new("Indirect Kills Count as Win", false);

    public ModdedToggleOption Guessable { get; } =
        new("Guessable", false);

    public ModdedEnumOption<TrollAfterWinType> AfterWinType { get; } =
        new(
            "After Win Type",
            TrollAfterWinType.ContinuesGame,
            ["Continues Game", "Ends Game"]);

    public ModdedToggleOption AnnounceWin { get; } =
        new("Announce Troll Win", true)
        {
            Visible = () => OptionGroupSingleton<TrollOptions>.Instance.AfterWinType.Value == TrollAfterWinType.ContinuesGame
        };
}
