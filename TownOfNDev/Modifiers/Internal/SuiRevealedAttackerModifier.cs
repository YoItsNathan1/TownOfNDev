using MiraAPI.Modifiers;
using TownOfUs.Modules.Wiki;

namespace TownOfNDev.Modifiers.Internal;

public sealed class SuiRevealedAttackerModifier(byte suiId) : BaseModifier, IWikiDiscoverable
{
    public override string ModifierName => "SUI Revealed Attacker";
    public override bool HideOnUi => true;
    public override bool ShowInFreeplay => false;
    public bool IsHiddenFromList => true;

    public byte SuiId { get; } = suiId;
}
