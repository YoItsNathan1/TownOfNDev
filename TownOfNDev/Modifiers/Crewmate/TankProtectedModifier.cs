using MiraAPI.Modifiers;
using MiraAPI.Utilities.Assets;
using TownOfNDev.Assets;
using TownOfUs.Modules.Wiki;
using UnityEngine;

namespace TownOfNDev.Modifiers.Crewmate;

public sealed class TankProtectedModifier : BaseModifier, IWikiDiscoverable
{
    public override string ModifierName => "Tank Protection";
    public override LoadableAsset<Sprite>? ModifierIcon => TownOfNDevAssets.TankProtectedStatusIcon;
    public override bool HideOnUi => !Player || !Player.AmOwner;
    public override bool ShowInFreeplay => false;
    public bool IsHiddenFromList => true;

    public override string GetDescription() =>
        "Your tasks are complete. Ordinary direct kill attempts against you are blocked.";
}
