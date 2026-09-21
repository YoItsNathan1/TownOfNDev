using MiraAPI.GameOptions;
using MiraAPI.Modifiers.Types;
using TownOfNDev.Options;

namespace TownOfNDev.Modifiers.Impostor;

public sealed class ReverserAlertModifier : TimedModifier
{
    public override float Duration => OptionGroupSingleton<ReverserOptions>.Instance.ReverseDuration.Value;
    public override string ModifierName => "Reversing Attacks";
    public override bool HideOnUi => true;
    public override bool ShowInFreeplay => false;

    public override void OnDeath(DeathReason reason)
    {
        ModifierComponent?.RemoveModifier(this);
    }

    public override void OnMeetingStart()
    {
        ModifierComponent?.RemoveModifier(this);
    }
}
