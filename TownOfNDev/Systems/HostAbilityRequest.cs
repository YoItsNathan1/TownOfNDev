using BepInEx.Logging;
using UnityEngine;

namespace TownOfNDev.Systems;

/// <summary>
/// Local-only request correlation for a host-validated custom ability. Mira owns
/// the actual button/HUD lifecycle; this tracks only a single outstanding RPC.
/// </summary>
internal sealed class HostAbilityRequest
{
    // A lost response must not permanently disable an ability. An old response
    // cannot affect a newer request because each request has its own identifier.
    private static readonly ManualLogSource Log =
        BepInEx.Logging.Logger.CreateLogSource("TownOfNDev.AbilityRequests");
    private const float TimeoutSeconds = 15f;
    private readonly string _abilityName;
    private int _serial;
    private int _pendingId;
    private float _sentAt;

    public HostAbilityRequest(string abilityName)
    {
        _abilityName = abilityName;
    }

    public bool IsPending
    {
        get
        {
            if (_pendingId != 0 && Time.realtimeSinceStartup - _sentAt >= TimeoutSeconds)
            {
                Log.LogWarning($"{_abilityName} host result timed out for request {_pendingId}; local button unlocked.");
                Cancel();
            }

            return _pendingId != 0;
        }
    }

    public int Begin()
    {
        // The caller guards against concurrent requests with IsPending.
        _serial = _serial == int.MaxValue ? 1 : _serial + 1;
        _pendingId = _serial;
        _sentAt = Time.realtimeSinceStartup;
        return _pendingId;
    }

    public bool TryComplete(int requestId)
    {
        if (requestId == 0 || !IsPending || _pendingId != requestId)
        {
            return false;
        }

        Cancel();
        return true;
    }

    public void Cancel()
    {
        _pendingId = 0;
    }

    public static void LogResultFailure(string abilityName, System.Exception exception)
    {
        Log.LogWarning($"Could not apply {abilityName} host result locally: {exception}");
    }
}
