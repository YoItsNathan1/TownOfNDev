using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using TownOfNDev.Roles.Impostor;

namespace TownOfNDev.Options;

public sealed class CondemnerOptions : AbstractRoleOptionGroup<CondemnerRole>
{
    public override string GroupName => "Condemner (TownOfNDev)";

    public ModdedNumberOption DeathNoteCooldown { get; } =
        new("Death Note Cooldown", 45f, 30f, 60f, 5f, MiraNumberSuffixes.Seconds, "0");

    public ModdedToggleOption LimitUsesPerRound { get; } =
        new("Limit Death Note Uses Per Round", true);

    public ModdedNumberOption MaxUsesPerRound { get; } =
        new("Maximum Death Note Uses Per Round", 2f, 1f, 5f, 1f, MiraNumberSuffixes.None, "0")
        {
            Visible = () => OptionGroupSingleton<CondemnerOptions>.Instance.LimitUsesPerRound.Value
        };

    public ModdedToggleOption PersistAfterCondemnerDeath { get; } =
        new("Death Row Persists After Condemner Dies", false);
}
