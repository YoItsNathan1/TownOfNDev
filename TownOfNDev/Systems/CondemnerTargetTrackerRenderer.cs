using System.Collections.Generic;
using System.Linq;
using TownOfNDev.Assets;
using TownOfNDev.Roles.Impostor;
using TownOfUs.Utilities;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TownOfNDev.Systems;

public static class CondemnerTargetTrackerRenderer
{
    private static readonly HashSet<byte> TrackedPlayers = [];

    public static void Reset()
    {
        ClearAll();
        TrackedPlayers.Clear();
    }

    public static void UpdateAll()
    {
        var local = PlayerControl.LocalPlayer;
        var canShow = local && local.Data != null && !local.Data.Disconnected && !local.HasDied() &&
                      local.Data.Role is CondemnerRole && MeetingHud.Instance == null && ExileController.Instance == null;

        if (!canShow)
        {
            ClearAll();
            return;
        }

        var active = new HashSet<byte>();
        foreach (var player in PlayerControl.AllPlayerControls)
        {
            if (!player || player == local || player.Data == null || player.Data.Disconnected || player.HasDied() ||
                !CondemnerSystem.IsCondemnedBy(player, local.PlayerId))
            {
                continue;
            }

            active.Add(player.PlayerId);
            EnsureWorldMarker(local, player);
        }

        foreach (var playerId in TrackedPlayers.ToArray())
        {
            if (!active.Contains(playerId))
            {
                DestroyWorldMarker(playerId);
            }
        }

        TrackedPlayers.Clear();
        foreach (var playerId in active)
        {
            TrackedPlayers.Add(playerId);
        }
    }

    public static void HandlePlayerUnavailable(byte playerId)
    {
        DestroyWorldMarker(playerId);
        TrackedPlayers.Remove(playerId);
    }

    private static void EnsureWorldMarker(PlayerControl local, PlayerControl player)
    {
        var nameText = player.cosmetics?.nameText;
        var body = player.cosmetics?.currentBodySprite?.BodySprite;
        if (nameText is null || !nameText || body is null || !body)
        {
            return;
        }

        var parent = nameText.transform.parent;
        if (!parent)
        {
            return;
        }

        var markerName = $"TownOfNDev_CondemnerWorldTracker_{player.PlayerId}";
        var markerTransform = parent.Find(markerName);
        GameObject marker;

        if (markerTransform)
        {
            marker = markerTransform.gameObject;
        }
        else
        {
            marker = new GameObject(markerName);
            marker.transform.SetParent(parent, false);
            marker.transform.localScale = new Vector3(0.18f, 0.18f, 1f);
            marker.layer = player.gameObject.layer;

            var renderer = marker.AddComponent<SpriteRenderer>();
            renderer.sprite = TownOfNDevAssets.CondemnerDeathNoteTracker.LoadAsset();
        }

        var markerRenderer = marker.GetComponent<SpriteRenderer>();
        markerRenderer.sortingLayerID = body.sortingLayerID;
        markerRenderer.sortingOrder = body.sortingOrder + 5;
        markerRenderer.maskInteraction = body.maskInteraction;
        marker.layer = body.gameObject.layer;

        var visionAlpha = GetVisionAlpha(local, player);
        var bodyAlpha = body.enabled && body.gameObject.activeInHierarchy && !player.inVent
            ? body.color.a
            : 0f;
        var finalAlpha = Mathf.Min(visionAlpha, bodyAlpha);
        marker.SetActive(finalAlpha > 0.01f);
        if (!marker.activeSelf)
        {
            return;
        }

        markerRenderer.color = new Color(1f, 1f, 1f, finalAlpha);

        nameText.ForceMeshUpdate();
        var halfNameWidth = 0f;
        if (nameText.textBounds.size.x > 0f)
        {
            halfNameWidth = nameText.textBounds.size.x / 2f;
        }
        else if (nameText.preferredWidth > 0f)
        {
            halfNameWidth = nameText.preferredWidth / 2f;
        }

        var namePosition = nameText.transform.localPosition;
        marker.transform.localPosition = new Vector3(
            namePosition.x + halfNameWidth + 0.16f,
            namePosition.y + 0.01f,
            namePosition.z - 0.2f);
    }

    private static float GetVisionAlpha(PlayerControl local, PlayerControl target)
    {
        if (!local || !target || local.lightSource == null)
        {
            return 1f;
        }

        var origin = local.GetTruePosition();
        var offset = target.GetTruePosition() - origin;
        var distance = offset.magnitude;
        var viewDistance = local.lightSource.viewDistance;

        if (viewDistance <= 0f || distance >= viewDistance)
        {
            return 0f;
        }

        if (distance > 0.001f &&
            PhysicsHelpers.AnyNonTriggersBetween(origin, offset.normalized, distance, Constants.ShipAndObjectsMask))
        {
            return 0f;
        }

        // Blend the marker out over the outer edge of the player's real vision
        // instead of letting it remain as a bright UI-like sprite in darkness.
        var fadeWidth = Mathf.Min(0.75f, viewDistance * 0.15f);
        if (fadeWidth <= 0.001f)
        {
            return 1f;
        }

        return Mathf.Clamp01((viewDistance - distance) / fadeWidth);
    }

    private static void DestroyWorldMarker(byte playerId)
    {
        var player = PlayerControl.AllPlayerControls.ToArray().FirstOrDefault(p => p && p.PlayerId == playerId);
        if (player is null || !player)
        {
            return;
        }

        var nameText = player.cosmetics?.nameText;
        if (nameText is null || !nameText)
        {
            return;
        }

        var parent = nameText.transform.parent;
        if (!parent)
        {
            return;
        }

        var markerTransform = parent.Find($"TownOfNDev_CondemnerWorldTracker_{playerId}");
        if (markerTransform)
        {
            Object.Destroy(markerTransform.gameObject);
        }
    }

    private static void ClearAll()
    {
        foreach (var playerId in TrackedPlayers.ToArray())
        {
            DestroyWorldMarker(playerId);
        }
    }
}
