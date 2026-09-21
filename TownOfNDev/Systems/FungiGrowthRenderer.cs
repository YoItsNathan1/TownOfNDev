using System;
using System.Collections.Generic;
using System.Linq;
using TownOfNDev.Assets;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TownOfNDev.Systems;

public static class FungiGrowthRenderer
{
    private sealed class GrowthSet
    {
        public readonly GameObject[] Objects = new GameObject[3];
        public readonly SpriteRenderer[] Renderers = new SpriteRenderer[3];
        public byte Stage;
    }

    private static readonly Dictionary<byte, GrowthSet> GrowthByPlayer = [];

    // All three growths use the clean mushroom strand art. Each copy has its own
    // body-relative anchor, size, orientation and rotation so it reads as three
    // separate colonies without carrying any reference-crewmate pixels.
    private static readonly Vector2[] AnchorFractions =
    [
        new(-0.72f, 0.18f),
        new(0.72f, 0.56f),
        new(-0.65f, -0.44f)
    ];

    private static readonly Vector3[] Scales =
    [
        new(0.36f, 0.36f, 1f),
        new(0.31f, 0.31f, 1f),
        new(0.33f, 0.33f, 1f)
    ];

    private static readonly bool[] BaseFlipX = [false, true, false];
    private static readonly float[] BaseRotation = [0f, -16f, 12f];

    public static void Refresh(PlayerControl player, byte stage)
    {
        if (!player || player.Data == null)
        {
            return;
        }

        stage = (byte)Math.Clamp(stage, (byte)0, (byte)3);
        if (stage == 0)
        {
            Remove(player.PlayerId);
            return;
        }

        if (!GrowthByPlayer.TryGetValue(player.PlayerId, out var set))
        {
            set = Create(player);
            GrowthByPlayer[player.PlayerId] = set;
        }

        set.Stage = stage;
        UpdateSet(player, set);
    }

    public static void UpdateAll()
    {
        foreach (var entry in GrowthByPlayer.ToArray())
        {
            var player = PlayerControl.AllPlayerControls.ToArray()
                .FirstOrDefault(x => x && x.PlayerId == entry.Key);
            if (player is null || !player || player.Data is null)
            {
                Remove(entry.Key);
                continue;
            }

            var data = player.Data;
            if (data.Disconnected || data.IsDead)
            {
                Remove(entry.Key);
                continue;
            }

            UpdateSet(player, entry.Value);
        }
    }

    public static void Remove(byte playerId)
    {
        if (!GrowthByPlayer.Remove(playerId, out var set))
        {
            return;
        }

        foreach (var obj in set.Objects)
        {
            if (obj)
            {
                Object.Destroy(obj);
            }
        }
    }

    public static void ClearAll()
    {
        foreach (var playerId in GrowthByPlayer.Keys.ToArray())
        {
            Remove(playerId);
        }
    }

    private static GrowthSet Create(PlayerControl player)
    {
        var set = new GrowthSet();
        var cleanStrand = TownOfNDevAssets.FungiStrand1.LoadAsset();

        for (var i = 0; i < 3; i++)
        {
            var obj = new GameObject($"TownOfNDev_FungiStrand_{i + 1}_{player.PlayerId}");
            obj.transform.localScale = Scales[i];

            var renderer = obj.AddComponent<SpriteRenderer>();
            renderer.sprite = cleanStrand;
            set.Objects[i] = obj;
            set.Renderers[i] = renderer;
        }

        return set;
    }

    private static void UpdateSet(PlayerControl player, GrowthSet set)
    {
        var body = player.cosmetics?.currentBodySprite?.BodySprite;
        if (body is null || !body)
        {
            return;
        }

        var bodyTransform = body.transform;
        if (!bodyTransform)
        {
            return;
        }

        var flipped = body.flipX;
        var alpha = body.color.a;
        var bounds = body.sprite ? body.sprite.bounds : new Bounds(Vector3.zero, new Vector3(1f, 1.5f, 0f));
        var extents = bounds.extents;
        var center = bounds.center;

        for (var i = 0; i < 3; i++)
        {
            var active = set.Stage >= i + 1;
            var obj = set.Objects[i];
            var renderer = set.Renderers[i];
            if (!obj || !renderer)
            {
                continue;
            }

            obj.SetActive(active);
            if (!active)
            {
                continue;
            }

            // Parent directly to the live body renderer rather than PlayerControl.
            // This keeps every strand attached through ordinary movement and the
            // cosmetic/body animation offsets that can move independently of the
            // PlayerControl transform.
            if (obj.transform.parent != bodyTransform)
            {
                obj.transform.SetParent(bodyTransform, false);
                obj.transform.localScale = Scales[i];
            }

            var anchor = AnchorFractions[i];
            var x = center.x + extents.x * anchor.x;
            var y = center.y + extents.y * anchor.y;
            if (flipped)
            {
                x = center.x - (x - center.x);
            }

            obj.transform.localPosition = new Vector3(x, y, -0.02f);
            obj.transform.localEulerAngles = new Vector3(
                0f,
                0f,
                flipped ? -BaseRotation[i] : BaseRotation[i]);

            renderer.flipX = flipped ^ BaseFlipX[i];
            renderer.sortingLayerID = body.sortingLayerID;
            renderer.sortingOrder = body.sortingOrder + 1;
            renderer.gameObject.layer = body.gameObject.layer;

            var color = Color.white;
            color.a = alpha;
            renderer.color = color;
        }
    }
}
