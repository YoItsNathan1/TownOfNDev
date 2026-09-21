using MiraAPI.GameOptions;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using TownOfNDev.Assets;
using TownOfNDev.Options;
using TownOfUs.Modifiers;
using TownOfUs.Modifiers.Game;
using TownOfUs.Modules.Wiki;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfNDev.Modifiers.Crewmate;

public sealed class EyeMaskModifier : TouGameModifier, IWikiDiscoverable
{
    public override string IdPart => "NDevEyeMask";
    public override string ModifierName => "Eye Mask";
    public override string IntroInfo => "Tasks can make you unexpectedly fall asleep.";

    public override ModifierUiConfiguration Configuration => new(
        TownOfNDevColors.EyeMask,
        TmpSpriteUtils.CreateSpriteAsset(
            TownOfNDevAssets.EyeMaskModifierIcon.LoadAsset(),
            "TownOfNDev.Modifier.Crewmate.EyeMask",
            1.45f));

    public override LoadableAsset<Sprite> ModifierIcon => TownOfNDevAssets.EyeMaskModifierIcon;
    public override Color FreeplayFileColor => TownOfNDevColors.EyeMask;
    public override ModifierFaction FactionType => ModifierFaction.CrewmatePassive;

    public List<CustomButtonWikiDescription> Abilities { get; } = [];

    public override string GetDescription() => "After completing a task, you may fall asleep after a random delay. While asleep, you are blind, unable to move, and unable to interact.";

    public string GetAdvancedDescription() =>
        "Completing a task can make you unexpectedly drowsy. After a short, unpredictable delay, you fall asleep: your screen goes dark and you cannot move or interact until you wake up." +
        MiscUtils.AppendOptionsText(GetType());

    public override int GetAssignmentChance() =>
        (int)OptionGroupSingleton<EyeMaskOptions>.Instance.AssignmentChance.Value;

    public override int GetAmountPerGame() =>
        (int)OptionGroupSingleton<EyeMaskOptions>.Instance.Amount;

    public override bool IsModifierValidOn(RoleBehaviour role)
    {
        return base.IsModifierValidOn(role) && role.IsCrewmate();
    }
}
