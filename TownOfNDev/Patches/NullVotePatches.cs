using HarmonyLib;
using MiraAPI.Modifiers;
using MiraAPI.Voting;
using TownOfNDev.Modifiers.Universal;
using TownOfUs.Utilities;

namespace TownOfNDev.Patches;

[HarmonyPatch(typeof(VotingUtils), nameof(VotingUtils.CalculateNumVotes))]
public static class NullVotePatches
{
    [HarmonyPrefix]
    public static void CalculateNumVotesPrefix(ref IEnumerable<CustomVote> votes)
    {
        // Only the tally input is filtered. The underlying CustomVote records stay
        // untouched, so Null still visibly casts a normal vote and its vote icon is
        // shown in the results even though it contributes zero voting power.
        votes = votes.Where(vote => !HasNull(vote.Voter));
    }

    private static bool HasNull(byte playerId)
    {
        var player = MiscUtils.PlayerById(playerId);
        return player != null && player.HasModifier<NullModifier>();
    }
}
