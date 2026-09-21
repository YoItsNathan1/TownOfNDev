using MiraAPI.GameOptions;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using TownOfNDev.Assets;
using TownOfNDev.Options;
using TownOfNDev.Roles.Neutral;
using TownOfUs.Modifiers;
using TownOfUs.Modifiers.Game;
using TownOfUs.Modules.Wiki;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfNDev.Modifiers.Universal;

public sealed class MiracleModifier : UniversalGameModifier, IWikiDiscoverable
{
    private static readonly HashSet<string> IncompatibleRoleTypeNames =
    [
        "TankRole",
        "UnstoppableRole",
        "TerroristRole"
    ];

    public override string IdPart => "NDevMiracle";
    public override string ModifierName => "Miracle";
    public override string IntroInfo => "Incoming kills may fail to kill you.";

    public override ModifierUiConfiguration Configuration => new(
        TownOfNDevColors.Miracle,
        TmpSpriteUtils.CreateSpriteAsset(
            TownOfNDevAssets.MiracleModifierIcon.LoadAsset(),
            "TownOfNDev.Modifier.Universal.Miracle",
            1.45f));
    public override LoadableAsset<Sprite> ModifierIcon => TownOfNDevAssets.MiracleModifierIcon;
    public override Color FreeplayFileColor => TownOfNDevColors.Miracle;
    public override ModifierFaction FactionType => ModifierFaction.UniversalPassive;

    public List<CustomButtonWikiDescription> Abilities { get; } = [];

    public override string GetDescription() =>
        "When another player tries to kill you, there is a configurable chance that the attack is blocked and you survive.";

    public string GetAdvancedDescription() =>
        "Every eligible incoming murder attempt makes an independent survival roll. A successful roll blocks the kill and gives the attacker the normal protected-kill feedback. The next attack rolls again from scratch." +
        MiscUtils.AppendOptionsText(GetType());

    public override int GetAssignmentChance() =>
        (int)OptionGroupSingleton<MiracleOptions>.Instance.AssignmentChance.Value;

    public override int GetAmountPerGame() =>
        (int)OptionGroupSingleton<MiracleOptions>.Instance.Amount;

    public override bool IsModifierValidOn(RoleBehaviour role)
    {
        if (!base.IsModifierValidOn(role) || role is TrollRole)
        {
            return false;
        }

        // Preserve the compatibility intent of the original modifier without hard-linking
        // TownOfNDev to roles that may not exist in every supported TOU build.
        return !IncompatibleRoleTypeNames.Contains(role.GetType().Name);
    }
}
