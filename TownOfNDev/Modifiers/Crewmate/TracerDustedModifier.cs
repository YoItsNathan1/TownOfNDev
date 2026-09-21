using MiraAPI.Modifiers;
using TownOfNDev.Systems;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfNDev.Modifiers.Crewmate;

public sealed class TracerDustedModifier(byte tracerId, float duration) : BaseModifier
{
    private bool _removalRequested;

    public override string ModifierName => "Tracer Dust";
    public override bool HideOnUi => true;
    public override bool ShowInFreeplay => false;
    public override bool Unique => false;

    public byte TracerId { get; } = tracerId;
    public float Remaining { get; private set; } = Mathf.Max(0f, duration);

    public override void FixedUpdate()
    {
        base.FixedUpdate();

        if (_removalRequested || AmongUsClient.Instance == null || !AmongUsClient.Instance.AmHost || !Player)
        {
            return;
        }

        if (Player.Data == null || Player.Data.Disconnected || Player.HasDied() || !TracerSystem.IsActiveTracer(TracerId))
        {
            RequestRemoval();
            return;
        }

        Remaining -= Time.fixedDeltaTime;
        if (Remaining <= 0f)
        {
            RequestRemoval();
        }
    }

    private void RequestRemoval()
    {
        _removalRequested = true;
        if (Player)
        {
            Player.RpcRemoveModifier(UniqueId);
        }
    }
}
