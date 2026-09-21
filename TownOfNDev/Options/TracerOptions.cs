using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using TownOfNDev.Roles.Crewmate;

namespace TownOfNDev.Options;

public sealed class TracerOptions : AbstractRoleOptionGroup<TracerRole>
{
    public override string GroupName => "Tracer (TownOfNDev)";

    public ModdedNumberOption DustDuration { get; } =
        new("Dust Duration", 25f, 15f, 45f, 5f, MiraNumberSuffixes.Seconds, "0");

    public ModdedToggleOption TraceProximityOutline { get; } =
        new("Trace Proximity Outline", true);

    public ModdedToggleOption ShowTraceInMeetings { get; } =
        new("Show Trace In Meetings", true);
}
