using MiraAPI.GameEnd;
using MiraAPI.GameOptions;
using TownOfNDev.GameOver;
using TownOfNDev.Options;
using TownOfNDev.Systems;
using TownOfUs.Interfaces;

namespace TownOfNDev.WinConditions;

public sealed class FungiWinCondition : IWinCondition
{
    // Runs immediately before Town of Us' generic neutral win condition (priority 5)
    // so a multi-Fungi faction can pass every Fungi member to its own game-over type.
    public int Priority => 4;

    public bool IsMet(LogicGameFlowNormal gameFlow)
    {
        if (AmongUsClient.Instance == null || !AmongUsClient.Instance.AmHost)
        {
            return false;
        }

        if (OptionGroupSingleton<FungiOptions>.Instance.WinMode.Value != FungiWinMode.IndependentMustSurvive)
        {
            return false;
        }

        return FungiInfectionSystem.AnyFungiAlive() && FungiInfectionSystem.CurrentThresholdMet;
    }

    public void TriggerGameOver(LogicGameFlowNormal gameFlow)
    {
        if (AmongUsClient.Instance == null || !AmongUsClient.Instance.AmHost)
        {
            return;
        }

        var winners = FungiInfectionSystem.GetFungiWinnerData();
        if (winners.Length > 0)
        {
            CustomGameOver.Trigger<FungiGameOver>(winners);
        }
    }
}
