using MiraAPI.GameOptions;
using TownOfNDev.Options;
using UnityEngine;
using Random = UnityEngine.Random;

namespace TownOfNDev.Systems;

public static class MiracleSystem
{
    private static byte? PendingProtectedTargetId;
    private static float PendingProtectedUntil;

    public static bool RollSurvival()
    {
        var chance = Mathf.Clamp(
            OptionGroupSingleton<MiracleOptions>.Instance.SurvivalChance.Value,
            0f,
            100f);

        if (chance >= 100f)
        {
            return true;
        }

        if (chance <= 0f)
        {
            return false;
        }

        return Random.Range(0f, 100f) < chance;
    }

    public static void MarkPendingProtectedResult(PlayerControl target)
    {
        PendingProtectedTargetId = target.PlayerId;
        PendingProtectedUntil = Time.realtimeSinceStartup + 0.5f;
    }

    public static bool ConsumePendingProtectedResult(PlayerControl target)
    {
        if (!PendingProtectedTargetId.HasValue)
        {
            return false;
        }

        if (Time.realtimeSinceStartup > PendingProtectedUntil)
        {
            ClearPendingProtectedResult();
            return false;
        }

        if (PendingProtectedTargetId.Value != target.PlayerId)
        {
            return false;
        }

        ClearPendingProtectedResult();
        return true;
    }

    public static void ClearPendingProtectedResult()
    {
        PendingProtectedTargetId = null;
        PendingProtectedUntil = 0f;
    }
}
