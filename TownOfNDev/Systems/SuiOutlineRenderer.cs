using System.Collections.Generic;
using System.Linq;
using MiraAPI.Modifiers;
using Reactor.Utilities.Extensions;
using TownOfNDev.Modifiers.Internal;
using TownOfNDev.Roles.Crewmate;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfNDev.Systems;

public static class SuiOutlineRenderer
{
    private static readonly HashSet<byte> OutlinedPlayers = [];

    public static void UpdateAll()
    {
        var local = PlayerControl.LocalPlayer;
        if (!local || local.Data == null || local.Data.Disconnected || local.HasDied() ||
            local.Data.Role is not SuiRole || MeetingHud.Instance || ExileController.Instance)
        {
            ClearAll();
            return;
        }

        var shouldOutline = new HashSet<byte>();
        foreach (var player in PlayerControl.AllPlayerControls.ToArray())
        {
            if (!player || player.Data == null || player.Data.Disconnected || player.HasDied() ||
                (!SuiSystem.IsLocalHuntTarget(player, local) &&
                 !player.GetModifiers<SuiRevealedAttackerModifier>().Any(x => x.SuiId == local.PlayerId)))
            {
                continue;
            }

            var body = player.cosmetics?.currentBodySprite?.BodySprite;
            if (body is null || !body || !body.enabled || !body.gameObject.activeInHierarchy || body.color.a <= 0.05f)
            {
                continue;
            }

            body.SetOutline(Color.red);
            shouldOutline.Add(player.PlayerId);
        }

        foreach (var oldId in OutlinedPlayers.ToArray())
        {
            if (shouldOutline.Contains(oldId))
            {
                continue;
            }

            ClearOutline(oldId);
        }

        foreach (var playerId in shouldOutline)
        {
            OutlinedPlayers.Add(playerId);
        }
    }

    public static void ClearAll()
    {
        foreach (var playerId in OutlinedPlayers.ToArray())
        {
            ClearOutline(playerId);
        }
    }

    private static void ClearOutline(byte playerId)
    {
        var player = FindPlayer(playerId);
        var body = player?.cosmetics?.currentBodySprite?.BodySprite;
        if (body is not null && body)
        {
            body.SetOutline(null);
        }

        OutlinedPlayers.Remove(playerId);
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
