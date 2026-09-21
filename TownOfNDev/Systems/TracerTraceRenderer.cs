using System.Collections.Generic;
using System.Linq;
using MiraAPI.GameOptions;
using MiraAPI.Utilities;
using Reactor.Utilities.Extensions;
using TownOfNDev.Assets;
using TownOfNDev.Options;
using TownOfNDev.Roles.Crewmate;
using TownOfUs.Utilities;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TownOfNDev.Systems;

public static class TracerTraceRenderer
{
    private sealed class TraceVisual
    {
        public GameObject Object = null!;
        public SpriteRenderer Renderer = null!;
    }

    private static readonly Dictionary<byte, TraceVisual> Visuals = [];
    private static readonly HashSet<byte> OutlinedPlayers = [];

    private static readonly Vector2 HandAnchor = new(-0.48f, 0.08f);
    private static readonly Vector3 HandScale = new(0.18f, 0.18f, 1f);

    public static void UpdateAll()
    {
        var local = PlayerControl.LocalPlayer;
        if (!local || local.Data == null || local.Data.Disconnected || local.HasDied() ||
            local.Data.Role is not TracerRole || MeetingHud.Instance)
        {
            ClearWorldVisuals();
            return;
        }

        var tracerId = local.PlayerId;
        var activeTraceIds = new HashSet<byte>();

        foreach (var player in PlayerControl.AllPlayerControls.ToArray())
        {
            if (!player || player.Data == null || player.Data.Disconnected ||
                player.HasDied() || !TracerSystem.HasTraceFrom(player, tracerId))
            {
                continue;
            }

            activeTraceIds.Add(player.PlayerId);
            UpdateHandprint(player);
        }

        foreach (var playerId in Visuals.Keys.ToArray())
        {
            if (!activeTraceIds.Contains(playerId))
            {
                RemoveVisual(playerId);
            }
        }

        UpdateOutlines(local, activeTraceIds);
    }

    public static void ClearWorldVisuals()
    {
        foreach (var playerId in Visuals.Keys.ToArray())
        {
            RemoveVisual(playerId);
        }

        ClearOutlines();
    }

    public static void ClearAll() => ClearWorldVisuals();

    private static void UpdateHandprint(PlayerControl player)
    {
        var body = player.cosmetics?.currentBodySprite?.BodySprite;
        if (body is null || !body)
        {
            RemoveVisual(player.PlayerId);
            return;
        }

        var visible = IsBodyVisible(player, body);
        if (!Visuals.TryGetValue(player.PlayerId, out var visual))
        {
            visual = CreateVisual(player.PlayerId);
            Visuals[player.PlayerId] = visual;
        }

        visual.Object.SetActive(visible);
        if (!visible)
        {
            return;
        }

        if (visual.Object.transform.parent != body.transform)
        {
            visual.Object.transform.SetParent(body.transform, false);
            visual.Object.transform.localScale = HandScale;
        }

        var bounds = body.sprite
            ? body.sprite.bounds
            : new Bounds(Vector3.zero, new Vector3(1f, 1.5f, 0f));
        var center = bounds.center;
        var extents = bounds.extents;
        var x = center.x + extents.x * HandAnchor.x;
        var y = center.y + extents.y * HandAnchor.y;
        if (body.flipX)
        {
            x = center.x - (x - center.x);
        }

        visual.Object.transform.localPosition = new Vector3(x, y, -0.025f);
        visual.Object.transform.localEulerAngles = new Vector3(0f, 0f, body.flipX ? 8f : -8f);
        visual.Renderer.flipX = body.flipX;
        visual.Renderer.sortingLayerID = body.sortingLayerID;
        visual.Renderer.sortingOrder = body.sortingOrder + 2;
        visual.Renderer.gameObject.layer = body.gameObject.layer;

        var color = Color.white;
        color.a = body.color.a;
        visual.Renderer.color = color;
    }

    private static TraceVisual CreateVisual(byte playerId)
    {
        var obj = new GameObject($"TownOfNDev_TracerTrace_{playerId}");
        obj.transform.localScale = HandScale;
        var renderer = obj.AddComponent<SpriteRenderer>();
        renderer.sprite = TownOfNDevAssets.TracerTraceHandprint.LoadAsset();
        return new TraceVisual
        {
            Object = obj,
            Renderer = renderer
        };
    }

    private static void UpdateOutlines(PlayerControl local, HashSet<byte> activeTraceIds)
    {
        if (!OptionGroupSingleton<TracerOptions>.Instance.TraceProximityOutline.Value)
        {
            ClearOutlines();
            return;
        }

        var range = local.Data.Role.GetAbilityDistance();
        var shouldOutline = new HashSet<byte>();

        foreach (var player in PlayerControl.AllPlayerControls.ToArray())
        {
            if (!player || player.Data == null || player.Data.Disconnected || player.HasDied() ||
                !activeTraceIds.Contains(player.PlayerId))
            {
                continue;
            }

            var body = player.cosmetics?.currentBodySprite?.BodySprite;
            if (body is null || !body || !IsBodyVisible(player, body))
            {
                continue;
            }

            // Reuse TOU's standard closest-player helper for the specific Trace
            // carrier. That keeps the proximity cue within normal role range and
            // respects the same line-of-sight rules used by target abilities.
            var visibleTarget = local.GetClosestLivingPlayer(
                true,
                range,
                predicate: candidate => candidate != null && candidate.PlayerId == player.PlayerId);

            if (visibleTarget is null || !visibleTarget || visibleTarget.PlayerId != player.PlayerId)
            {
                continue;
            }

            body.SetOutline(TownOfNDevColors.Tracer);
            shouldOutline.Add(player.PlayerId);
        }

        foreach (var oldId in OutlinedPlayers.ToArray())
        {
            if (shouldOutline.Contains(oldId))
            {
                continue;
            }

            var oldPlayer = FindPlayer(oldId);
            var oldBody = oldPlayer?.cosmetics?.currentBodySprite?.BodySprite;
            if (oldBody is not null && oldBody)
            {
                oldBody.SetOutline(null);
            }
            OutlinedPlayers.Remove(oldId);
        }

        foreach (var playerId in shouldOutline)
        {
            OutlinedPlayers.Add(playerId);
        }
    }

    private static bool IsBodyVisible(PlayerControl player, SpriteRenderer body)
    {
        return !player.inVent && body.enabled && body.gameObject.activeInHierarchy && body.color.a > 0.05f;
    }

    private static void ClearOutlines()
    {
        foreach (var playerId in OutlinedPlayers.ToArray())
        {
            var player = FindPlayer(playerId);
            var body = player?.cosmetics?.currentBodySprite?.BodySprite;
            if (body is not null && body)
            {
                body.SetOutline(null);
            }
        }

        OutlinedPlayers.Clear();
    }

    private static void RemoveVisual(byte playerId)
    {
        if (!Visuals.Remove(playerId, out var visual))
        {
            return;
        }

        if (visual.Object)
        {
            Object.Destroy(visual.Object);
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
