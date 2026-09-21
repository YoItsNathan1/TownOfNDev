using System.Linq;
using MiraAPI.GameEnd;
using MiraAPI.Utilities;
using Reactor.Utilities.Extensions;
using TownOfNDev.Systems;
using TownOfUs.Modules;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TownOfNDev.GameOver;

public sealed class FungiGameOver : CustomGameOver
{
    public override bool VerifyCondition(PlayerControl playerControl, NetworkedPlayerInfo[] winners)
    {
        if (winners == null || winners.Length == 0)
        {
            return false;
        }

        var memberIds = FungiInfectionSystem.Members;
        return FungiInfectionSystem.AnyFungiAlive() &&
               FungiInfectionSystem.CurrentThresholdMet &&
               winners.All(x => x != null && memberIds.Contains(x.PlayerId));
    }

    public override void AfterEndGameSetup(EndGameManager endGameManager)
    {
        endGameManager.BackgroundBar.material.SetColor(ShaderID.Color, TownOfNDevColors.Fungi);

        var text = Object.Instantiate(endGameManager.WinText);
        text.text = "Fungi Win!";
        text.color = TownOfNDevColors.Fungi;
        GameHistory.WinningFaction =
            $"<color=#{TownOfNDevColors.Fungi.ToHtmlStringRGBA()}>Fungi Win</color>";
        text.transform.localScale = Vector3.one;

        var pos = endGameManager.WinText.transform.localPosition;
        pos.y = 1.5f;
        pos += Vector3.down * 0.15f;
        text.transform.localPosition = pos;
        text.text = $"<size=4>{text.text}</size>";
    }
}
