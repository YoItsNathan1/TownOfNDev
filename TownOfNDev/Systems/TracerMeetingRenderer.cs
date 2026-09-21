using MiraAPI.GameOptions;
using TownOfNDev.Assets;
using TownOfNDev.Options;
using TownOfNDev.Roles.Crewmate;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfNDev.Systems;

public static class TracerMeetingRenderer
{
    public static void Apply(MeetingHud meeting)
    {
        var local = PlayerControl.LocalPlayer;
        if (!local || local.Data == null || local.Data.Role is not TracerRole || local.HasDied() ||
            !OptionGroupSingleton<TracerOptions>.Instance.ShowTraceInMeetings.Value)
        {
            return;
        }

        foreach (var voteArea in meeting.playerStates)
        {
            if (!voteArea || voteArea.AmDead)
            {
                continue;
            }

            var player = FindPlayer(voteArea.PlayerId);
            if (!player || !TracerSystem.HasTraceFrom(player, local.PlayerId))
            {
                continue;
            }

            var markerName = $"TownOfNDev_TracerMeetingTrace_{voteArea.PlayerId}";
            if (voteArea.transform.Find(markerName))
            {
                continue;
            }

            var marker = new GameObject(markerName);
            marker.transform.SetParent(voteArea.transform, false);
            marker.transform.localPosition = new Vector3(1.38f, 0.11f, -8f);
            marker.transform.localScale = new Vector3(0.28f, 0.28f, 1f);
            marker.layer = voteArea.gameObject.layer;

            var renderer = marker.AddComponent<SpriteRenderer>();
            renderer.sprite = TownOfNDevAssets.TracerTraceHandprint.LoadAsset();
            renderer.sortingOrder = 30;
        }
    }

    private static PlayerControl? FindPlayer(byte playerId)
    {
        foreach (var player in PlayerControl.AllPlayerControls)
        {
            if (player && player.PlayerId == playerId)
            {
                return player;
            }
        }

        return null;
    }
}
