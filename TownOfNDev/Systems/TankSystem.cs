using MiraAPI.Modifiers;
using MiraAPI.Utilities;
using TownOfNDev.Assets;
using TownOfNDev.Modifiers.Crewmate;
using TownOfNDev.Options;
using TownOfNDev.Roles.Crewmate;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfNDev.Systems;

public static class TankSystem
{
    public static bool HasCompletedAllTasks(PlayerControl? player)
    {
        if (player is null || !player)
        {
            return false;
        }

        var data = player.Data;
        if (data == null || data.Disconnected || player.HasDied() ||
            data.Role is not TankRole || data.Tasks == null || data.Tasks.Count <= 0)
        {
            return false;
        }

        var tasks = data.Tasks;
        for (var i = 0; i < tasks.Count; i++)
        {
            if (!tasks[i].Complete)
            {
                return false;
            }
        }

        return true;
    }

    public static bool IsProtected(PlayerControl? player) => HasCompletedAllTasks(player);

    public static void HostRefreshProtection(PlayerControl player)
    {
        if (AmongUsClient.Instance == null || !AmongUsClient.Instance.AmHost || !player ||
            player.Data?.Role is not TankRole || !HasCompletedAllTasks(player) ||
            player.HasModifier<TankProtectedModifier>())
        {
            return;
        }

        player.RpcAddModifier<TankProtectedModifier>();
    }

    public static void NotifyProtectionActivated(PlayerControl player)
    {
        if (!player || !player.AmOwner || player.Data?.Role is not TankRole)
        {
            return;
        }

        var notification = Helpers.CreateAndShowNotification(
            "<b>Tank protection is now <color=#86BCEB>ACTIVE</color></b>",
            Color.white,
            new Vector3(0f, 1f, -20f),
            spr: TownOfNDevAssets.TankProtectedStatusIcon.LoadAsset());
        notification.AdjustNotification();
        notification.alphaTimer = 5f;
    }

    public static void NotifyBlockedAttacker(PlayerControl tank, PlayerControl attacker)
    {
        if (!tank || !tank.AmOwner || !attacker ||
            !MiraAPI.GameOptions.OptionGroupSingleton<TankOptions>.Instance.CanSeeWhoTriedToKill.Value)
        {
            return;
        }

        var attackerName = attacker.Data?.PlayerName ?? "Unknown";
        var notification = Helpers.CreateAndShowNotification(
            $"<b><color=#FF5050>{attackerName}</color> tried to kill you</b>",
            Color.white,
            new Vector3(0f, 1f, -20f),
            spr: TownOfNDevAssets.TankProtectedStatusIcon.LoadAsset());
        notification.AdjustNotification();
        notification.alphaTimer = 5f;
    }
}
