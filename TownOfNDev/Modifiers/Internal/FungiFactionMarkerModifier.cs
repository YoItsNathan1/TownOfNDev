using MiraAPI.GameEnd;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers.Types;
using TownOfNDev.GameOver;
using TownOfNDev.Options;
using TownOfNDev.Roles.Neutral;
using TownOfNDev.Systems;
using TownOfUs.Modules.Wiki;

namespace TownOfNDev.Modifiers.Internal;

public sealed class FungiFactionMarkerModifier : GameModifier, IWikiDiscoverable
{
    public override string ModifierName => "Fungi Faction Marker";
    public override bool HideOnUi => true;
    public override bool ShowInFreeplay => false;
    public bool IsHiddenFromList => true;

    public override int GetAssignmentChance() => 0;
    public override int GetAmountPerGame() => 0;
    public override int Priority() => 1000;
    public override bool IsModifierValidOn(RoleBehaviour role) => role is FungiRole;

    public override bool? DidWin(GameOverReason reason)
    {
        if (reason == CustomGameOver.GameOverReason<FungiGameOver>())
        {
            return true;
        }

        var mode = OptionGroupSingleton<FungiOptions>.Instance.WinMode.Value;
        if (mode == FungiWinMode.AdditionalWinnerDeathAllowed)
        {
            return FungiInfectionSystem.ObjectiveAchieved;
        }

        return false;
    }
}
