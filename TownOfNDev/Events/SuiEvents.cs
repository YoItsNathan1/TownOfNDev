using MiraAPI.Events;
using MiraAPI.Events.Mira;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Player;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using TownOfNDev.Buttons.Crewmate;
using TownOfNDev.Roles.Crewmate;
using TownOfNDev.Systems;
using TownOfUs.Buttons;
using TownOfUs.Options;
using TownOfUs.Utilities;

namespace TownOfNDev.Events;

public static class SuiEvents
{
    [RegisterEvent]
    public static void RoundStartHandler(RoundStartEvent @event)
    {
        if (@event.TriggeredByIntro)
        {
            SuiProtectButton.ResetLocalState();
            SuiSystem.BeginGame();
        }

        SuiOutlineRenderer.UpdateAll();
    }

    [RegisterEvent(-700)]
    public static void BeforeMurderHandler(BeforeMurderEvent @event)
    {
        if (@event.IsCancelled || MeetingHud.Instance || ExileController.Instance)
        {
            return;
        }

        var source = @event.Source;
        var target = @event.Target;
        if (source is null || !source || target is null || !target || source == target)
        {
            return;
        }

        var isSuiHuntKill = source.Data?.Role is SuiRole &&
                            (source.AmOwner
                                ? SuiSystem.IsLocalHuntTarget(target, source)
                                : SuiSystem.IsRevealedTo(target, source.PlayerId));
        if (source.Data?.Role is SuiRole && !isSuiHuntKill)
        {
            @event.Cancel();
            return;
        }

        if (!SuiSystem.IsProtected(target))
        {
            return;
        }

        if (isSuiHuntKill)
        {
            return;
        }

        @event.Cancel();
        SuiSystem.HostTryBlockMurder(source, target);
        ResetAttackTimer(source);
    }

    [RegisterEvent(-700)]
    public static void MiraButtonClickHandler(MiraButtonClickEvent @event)
    {
        if (@event.IsCancelled || MeetingHud.Instance || ExileController.Instance ||
            @event.Button is not CustomActionButton<PlayerControl> button ||
            button is not IKillButton)
        {
            return;
        }

        var source = PlayerControl.LocalPlayer;
        var target = button.Target;
        if (source is null || !source || target is null || !target || source == target ||
            !button.CanClick() || !SuiSystem.IsProtected(target))
        {
            return;
        }

        @event.Cancel();
        SuiSystem.RpcRequestProtectionTrigger(source, target);
        ResetAttackTimer(source, button);
    }

    [RegisterEvent]
    public static void PlayerDeathHandler(PlayerDeathEvent @event)
    {
        SuiSystem.HostHandlePlayerDeath(@event.Player);
        SuiOutlineRenderer.UpdateAll();
    }

    [RegisterEvent]
    public static void PlayerLeaveHandler(PlayerLeaveEvent @event)
    {
        var player = @event.ClientData.Character;
        if (!player)
        {
            return;
        }

        SuiSystem.HostHandlePlayerLeave(player);
        SuiOutlineRenderer.UpdateAll();
    }

    private static void ResetAttackTimer(PlayerControl source, CustomActionButton<PlayerControl>? button = null)
    {
        if (!source.AmOwner)
        {
            return;
        }

        var reset = OptionGroupSingleton<GeneralOptions>.Instance.TempSaveCdReset;
        button?.SetTimer(reset);
        source.SetKillTimer(reset);
    }
}
