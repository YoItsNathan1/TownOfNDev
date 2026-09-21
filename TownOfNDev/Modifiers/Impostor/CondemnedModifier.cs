using MiraAPI.Modifiers;

namespace TownOfNDev.Modifiers.Impostor;

public sealed class CondemnedModifier(byte condemnerId) : BaseModifier
{
    public override string ModifierName => "Death Row";
    public override bool HideOnUi => true;
    public override bool ShowInFreeplay => false;

    public byte CondemnerId { get; } = condemnerId;
}
