using MiraAPI.Modifiers;

namespace TownOfNDev.Modifiers.Crewmate;

public sealed class TracerTraceModifier(byte tracerId) : BaseModifier
{
    public override string ModifierName => "Tracer Evidence";
    public override bool HideOnUi => true;
    public override bool ShowInFreeplay => false;
    public override bool Unique => false;

    public byte TracerId { get; } = tracerId;
}
