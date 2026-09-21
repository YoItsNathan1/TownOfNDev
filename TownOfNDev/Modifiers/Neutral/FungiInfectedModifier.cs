using System;
using MiraAPI.Modifiers;
using TownOfNDev.Systems;

namespace TownOfNDev.Modifiers.Neutral;

public sealed class FungiInfectedModifier(byte stage) : BaseModifier
{
    public override string ModifierName => "Fungi Infection";
    public override bool HideOnUi => true;
    public override bool ShowInFreeplay => false;

    public byte Stage { get; private set; } = (byte)Math.Clamp(stage, (byte)0, (byte)3);

    public void SetStage(byte stage)
    {
        Stage = (byte)Math.Clamp(stage, (byte)0, (byte)3);
        if (Player)
        {
            FungiGrowthRenderer.Refresh(Player, Stage);
        }
    }

    public override void OnActivate()
    {
        base.OnActivate();
        if (Player)
        {
            FungiGrowthRenderer.Refresh(Player, Stage);
        }
    }

    public override void OnDeactivate()
    {
        if (Player)
        {
            FungiGrowthRenderer.Remove(Player.PlayerId);
        }
        base.OnDeactivate();
    }
}
