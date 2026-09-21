using MiraAPI.GameEnd;
using MiraAPI.Utilities;
using Reactor.Utilities.Extensions;
using TownOfNDev.Roles.Neutral;
using TownOfNDev.Systems;
using TownOfUs.Modules;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TownOfNDev.GameOver;

public sealed class TrollGameOver : CustomGameOver
{
    public override bool VerifyCondition(PlayerControl playerControl, NetworkedPlayerInfo[] winners)
    {
        if (winners == null || winners.Length != 1 || winners[0] == null)
        {
            return false;
        }

        var winnerId = winners[0].PlayerId;
        if (TrollSystem.HasWinner)
        {
            return TrollSystem.IsWinner(winnerId);
        }

        // Fallback for a client that receives the end-game packet before its local
        // AfterMurderEvent has populated TrollSystem. Seed the winner id from the
        // authoritative winner payload so DidWin can resolve correctly client-side.
        if (winners[0].Object?.GetRoleWhenAlive() is not TrollRole)
        {
            return false;
        }

        TrollSystem.AcceptWinner(winnerId);
        return true;
    }

    public override void AfterEndGameSetup(EndGameManager endGameManager)
    {
        endGameManager.BackgroundBar.material.SetColor(ShaderID.Color, TownOfNDevColors.Troll);

        var text = Object.Instantiate(endGameManager.WinText);
        text.text = "Troll Wins!";
        text.color = TownOfNDevColors.Troll;
        GameHistory.WinningFaction =
            $"<color=#{TownOfNDevColors.Troll.ToHtmlStringRGBA()}>Troll Win</color>";
        text.transform.localScale = Vector3.one;

        var pos = endGameManager.WinText.transform.localPosition;
        pos.y = 1.5f;
        pos += Vector3.down * 0.15f;
        text.transform.localPosition = pos;
        text.text = $"<size=4>{text.text}</size>";
    }
}
