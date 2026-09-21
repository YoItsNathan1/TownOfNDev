using MiraAPI.Modifiers;
using MiraAPI.Utilities.Assets;
using TownOfNDev.Assets;
using TownOfUs.Modules.Wiki;
using UnityEngine;

namespace TownOfNDev.Modifiers.Crewmate;

public sealed class SuiProtectedModifier(byte suiId) : BaseModifier, IWikiDiscoverable
{
    public override string ModifierName => "SUI Protected";
    public override LoadableAsset<Sprite>? ModifierIcon => TownOfNDevAssets.SuiRoleIcon;
    public override bool HideOnUi => !Player || !Player.AmOwner;
    public override bool ShowInFreeplay => false;
    public bool IsHiddenFromList => true;

    public byte SuiId { get; } = suiId;

    public override string GetDescription() =>
        "SUI is protecting you from lethal attacks while they remain alive.";
}
