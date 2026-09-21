using MiraAPI.Modifiers;
using TMPro;
using TownOfNDev.Modifiers.Universal;
using TownOfUs.Modules;
using TownOfUs.Utilities;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TownOfNDev.Systems;

public static class SoulhandlerMeetingRenderer
{
    private const string LabelPrefix = "TownOfNDev_SoulhandlerFaction_";

    public static void Apply(MeetingHud meeting)
    {
        var local = PlayerControl.LocalPlayer;
        var enabled = local && local.HasModifier<SoulhandlerModifier>();

        foreach (var voteArea in meeting.playerStates)
        {
            if (!voteArea || voteArea.NameText == null)
            {
                continue;
            }

            var labelName = $"{LabelPrefix}{voteArea.PlayerId}";
            var player = FindPlayer(voteArea.PlayerId);
            if (!enabled || player is null || !player || player.Data is not { } playerData ||
                !playerData.IsDead || playerData.Disconnected)
            {
                DestroyLabel(voteArea, labelName);
                continue;
            }

            var (faction, color) = GetFaction(player);
            var label = EnsureLabel(voteArea, labelName);
            label.text = $"<color={color}>{faction}</color>";
            label.transform.localPosition = voteArea.NameText.transform.localPosition + new Vector3(0f, -0.27f, -0.15f);
            label.ForceMeshUpdate();
        }
    }

    private static TextMeshPro EnsureLabel(PlayerVoteArea voteArea, string labelName)
    {
        var existing = voteArea.transform.Find(labelName);
        if (existing && existing.TryGetComponent<TextMeshPro>(out var existingText))
        {
            return existingText;
        }

        var label = Object.Instantiate(voteArea.NameText, voteArea.transform);
        label.name = labelName;
        label.text = string.Empty;
        label.fontSize = voteArea.NameText.fontSize * 0.56f;
        label.alignment = TextAlignmentOptions.Center;
        label.enableWordWrapping = false;
        label.gameObject.layer = voteArea.gameObject.layer;
        return label;
    }

    private static (string Name, string Color) GetFaction(PlayerControl player)
    {
        var roleWhenAlive = player.GetRoleWhenAlive();

        if (player.IsImpostorAligned() || (roleWhenAlive != null && roleWhenAlive.IsImpostor()))
        {
            return ("Impostor", "#FF3030");
        }

        if (roleWhenAlive != null && roleWhenAlive.IsCrewmate())
        {
            return ("Crewmate", "#66D9FF");
        }

        return ("Neutral", "#B8BDC7");
    }

    private static void DestroyLabel(PlayerVoteArea voteArea, string labelName)
    {
        var existing = voteArea.transform.Find(labelName);
        if (existing)
        {
            Object.Destroy(existing.gameObject);
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
