using MiraAPI.GameEnd;
using MiraAPI.GameOptions;
using TownOfNDev.GameOver;
using TownOfNDev.Options;
using TownOfNDev.Systems;
using TownOfUs.Interfaces;

namespace TownOfNDev.WinConditions;

public sealed class TrollWinCondition : IWinCondition
{
    // Only Ends Game mode uses a custom end condition. Continues Game mode marks
    // successful Trolls as additional winners through TrollRole.DidWin().
    public int Priority => 3;

    public bool IsMet(LogicGameFlowNormal gameFlow)
    {
        return AmongUsClient.Instance != null &&
               AmongUsClient.Instance.AmHost &&
               OptionGroupSingleton<TrollOptions>.Instance.AfterWinType.Value == TrollAfterWinType.EndsGame &&
               TrollSystem.HasWinner;
    }

    public void TriggerGameOver(LogicGameFlowNormal gameFlow)
    {
        if (AmongUsClient.Instance == null || !AmongUsClient.Instance.AmHost ||
            OptionGroupSingleton<TrollOptions>.Instance.AfterWinType.Value != TrollAfterWinType.EndsGame)
        {
            return;
        }

        var winners = TrollSystem.GetEndGameWinnerData();
        if (winners.Length == 1)
        {
            CustomGameOver.Trigger<TrollGameOver>(winners);
        }
    }
}
