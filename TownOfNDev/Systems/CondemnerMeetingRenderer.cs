using System.Collections.Generic;
using System.Linq;
using TownOfNDev.Assets;
using TownOfNDev.Roles.Impostor;
using TownOfUs.Utilities;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TownOfNDev.Systems;

public static class CondemnerMeetingRenderer
{
    private static readonly HashSet<byte> DeathRowThisMeeting = [];

    public static void Reset()
    {
        DeathRowThisMeeting.Clear();
    }

    public static void BeginMeeting(MeetingHud meeting)
    {
        Reset();
        CaptureCurrentDeathRow();
        Apply(meeting);
    }

    public static void HandlePlayerUnavailable(byte playerId)
    {
        DeathRowThisMeeting.Remove(playerId);

        var meeting = MeetingHud.Instance;
        if (!meeting)
        {
            return;
        }

        foreach (var voteArea in meeting.playerStates)
        {
            if (!voteArea || voteArea.PlayerId != playerId)
            {
                continue;
            }

            DestroyMarker(voteArea, $"TownOfNDev_DeathRow_{playerId}");
            DestroyMarker(voteArea, $"TownOfNDev_CondemnerTracker_{playerId}");
            break;
        }
    }

    public static void Apply(MeetingHud meeting)
    {
        if (!meeting)
        {
            return;
        }

        CaptureCurrentDeathRow();


        foreach (var voteArea in meeting.playerStates)
        {
            if (!voteArea || voteArea.NameText == null)
            {
                continue;
            }

            var player = FindPlayer(voteArea.PlayerId);
            var alive = player != null && player && player.Data != null && !player.Data.Disconnected && !player.HasDied();
            var showPublicSkull = alive && DeathRowThisMeeting.Contains(voteArea.PlayerId);
            var showPrivateTracker = false;

            var skullName = $"TownOfNDev_DeathRow_{voteArea.PlayerId}";
            var trackerName = $"TownOfNDev_CondemnerTracker_{voteArea.PlayerId}";

            if (!showPublicSkull)
            {
                DestroyMarker(voteArea, skullName);
            }

            if (!showPrivateTracker)
            {
                DestroyMarker(voteArea, trackerName);
            }

            if (!showPublicSkull && !showPrivateTracker)
            {
                continue;
            }

            voteArea.NameText.ForceMeshUpdate();

            var halfNameWidth = 0f;
            if (voteArea.NameText.textBounds.size.x > 0f)
            {
                halfNameWidth = voteArea.NameText.textBounds.size.x / 2f;
            }
            else if (voteArea.NameText.preferredWidth > 0f)
            {
                halfNameWidth = voteArea.NameText.preferredWidth / 2f;
            }

            var namePosition = voteArea.NameText.transform.localPosition;

            if (showPublicSkull)
            {
                var skull = EnsureMarker(
                    voteArea,
                    skullName,
                    TownOfNDevAssets.CondemnerDeathRowSkull.LoadAsset(),
                    0.20f,
                    35);
                var skullX = Mathf.Min(namePosition.x + halfNameWidth + 0.14f, 1.35f);
                skull.transform.localPosition = new Vector3(skullX, namePosition.y, -8f);
            }

            if (showPrivateTracker)
            {
                var tracker = EnsureMarker(
                    voteArea,
                    trackerName,
                    TownOfNDevAssets.CondemnerDeathNoteTracker.LoadAsset(),
                    0.16f,
                    36);
                var trackerX = Mathf.Max(namePosition.x - halfNameWidth - 0.14f, -1.35f);
                tracker.transform.localPosition = new Vector3(trackerX, namePosition.y, -8.1f);
            }
        }
    }

    private static GameObject EnsureMarker(
        PlayerVoteArea voteArea,
        string markerName,
        Sprite sprite,
        float scale,
        int sortingOrder)
    {
        var markerTransform = voteArea.transform.Find(markerName);
        if (markerTransform)
        {
            return markerTransform.gameObject;
        }

        var marker = new GameObject(markerName);
        marker.transform.SetParent(voteArea.transform, false);
        marker.transform.localScale = new Vector3(scale, scale, 1f);
        marker.layer = voteArea.gameObject.layer;

        var renderer = marker.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.sortingOrder = sortingOrder;
        return marker;
    }

    private static void DestroyMarker(PlayerVoteArea voteArea, string markerName)
    {
        var markerTransform = voteArea.transform.Find(markerName);
        if (markerTransform)
        {
            Object.Destroy(markerTransform.gameObject);
        }
    }

    private static void CaptureCurrentDeathRow()
    {
        foreach (var player in PlayerControl.AllPlayerControls)
        {
            if (player && player.Data != null && !player.Data.Disconnected && !player.HasDied() &&
                CondemnerSystem.IsCondemned(player))
            {
                DeathRowThisMeeting.Add(player.PlayerId);
            }
        }
    }

    private static PlayerControl? FindPlayer(byte playerId) =>
        PlayerControl.AllPlayerControls.ToArray().FirstOrDefault(player => player && player.PlayerId == playerId);
}
