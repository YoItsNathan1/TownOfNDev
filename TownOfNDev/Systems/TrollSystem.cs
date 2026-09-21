using TownOfNDev.Roles.Neutral;
using TownOfUs.Modules;

namespace TownOfNDev.Systems;

public static class TrollSystem
{
    private const byte NoWinner = byte.MaxValue;
    private static readonly HashSet<byte> SecuredWinnerPlayerIds = [];

    // The first Troll to secure a win is retained separately for the Ends Game mode,
    // where the match should finish immediately with that Troll as the sole winner.
    public static byte FirstWinnerPlayerId { get; private set; } = NoWinner;
    public static bool HasWinner => SecuredWinnerPlayerIds.Count > 0;

    public static void BeginGame()
    {
        FirstWinnerPlayerId = NoWinner;
        SecuredWinnerPlayerIds.Clear();
    }

    public static bool IsWinner(byte playerId) => SecuredWinnerPlayerIds.Contains(playerId);

    public static void AcceptWinner(byte playerId)
    {
        if (FirstWinnerPlayerId == NoWinner)
        {
            FirstWinnerPlayerId = playerId;
        }

        SecuredWinnerPlayerIds.Add(playerId);
    }

    public static bool HandleSuccessfulMurder(PlayerControl source, PlayerControl target)
    {
        if (source is null || !source || target is null || !target || source == target)
        {
            return false;
        }

        if (target.GetRoleWhenAlive() is not TrollRole)
        {
            return false;
        }

        // AfterMurderEvent is emitted only for a murder that actually succeeded.
        // Direct-vs-indirect eligibility is filtered by TrollEvents before this
        // method is called. Requiring a different source excludes self-kill paths;
        // exile, disconnects and non-murder deaths never enter this method at all.
        if (IsWinner(target.PlayerId))
        {
            return false;
        }

        AcceptWinner(target.PlayerId);
        return true;
    }

    public static NetworkedPlayerInfo[] GetEndGameWinnerData()
    {
        if (FirstWinnerPlayerId == NoWinner)
        {
            return [];
        }

        foreach (var player in PlayerControl.AllPlayerControls)
        {
            if (player && player.PlayerId == FirstWinnerPlayerId && player.Data is not null)
            {
                return [player.Data];
            }
        }

        return [];
    }
}
