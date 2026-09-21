using AmongUs.GameOptions;
using System;
using MiraAPI.GameEnd;
using MiraAPI.GameOptions;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using MiraAPI.Translation;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using TownOfNDev.Assets;
using TownOfNDev.GameOver;
using TownOfNDev.Options;
using TownOfNDev.Systems;
using TownOfUs;
using TownOfUs.Extensions;
using TownOfUs.Modules.Wiki;
using TownOfUs.Roles;
using TownOfUs.Roles.Neutral;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfNDev.Roles.Neutral;

public sealed class TrollRole(IntPtr cppPtr) : NeutralRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IUnguessableBasic
{
    public override void SpawnTaskHeader(PlayerControl playerControl)
    {
        if (!playerControl.AmOwner)
        {
            return;
        }

        ImportantTextTask task = PlayerTask.GetOrCreateTask<ImportantTextTask>(playerControl, 0);
        task.Text = $"{TownOfUsColors.Neutral.ToTextColor()}{MiraLocaleManager.Get("NeutralEvilTaskHeader")}</color>";
        task.name = "NeutralRoleText";
    }

    public string IdPart => "Troll";
    public string RoleName => MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}", "Troll");
    public string RoleDescription => "Make someone kill you.";
    public string RoleMedDescription =>
        "Convince another player to kill you and secure your personal win.";
    public string RoleLongDescription =>
        "Convince another player to kill you and secure your personal win.\n" +
        "Being guessed, executed in a meeting, ejected, disconnected, or causing your own death does not count.\n" +
        "A successful qualifying out-of-meeting kill from another player completes your objective.";

    public Color RoleColor => TownOfNDevColors.Troll;
    public ModdedRoleTeams Team => ModdedRoleTeams.Custom;
    public RoleAlignment RoleAlignment => RoleAlignment.NeutralEvil;

    public CustomRoleConfiguration Configuration => new(this)
    {
        MaxRoleCount = 3,
        DefaultRoleCount = 0,
        DefaultChance = 0,
        CanUseVent = false,
        CanUseSabotage = false,
        UseVanillaKillButton = false,
        Icon = TownOfNDevAssets.TrollRoleIcon,
        IconTmp = TmpSpriteUtils.CreateSpriteAsset(
            TownOfNDevAssets.TrollRoleIcon.LoadAsset(),
            "TownOfNDev.Role.Neutral.Troll",
            1.45f),
        GhostRole = (RoleTypes)RoleId.Get<NeutralGhostRole>()
    };

    public string GetAdvancedDescription() =>
        RoleLongDescription +
        MiscUtils.AppendOptionsText(GetType());

    public bool HasImpostorVision => OptionGroupSingleton<TrollOptions>.Instance.ImpostorVision.Value;
    public bool IsGuessable => OptionGroupSingleton<TrollOptions>.Instance.Guessable.Value;

    public bool WinConditionMet()
    {
        return OptionGroupSingleton<TrollOptions>.Instance.AfterWinType.Value == TrollAfterWinType.EndsGame &&
               TrollSystem.IsWinner(Player.PlayerId);
    }

    public override bool DidWin(GameOverReason gameOverReason)
    {
        // In Continues Game mode, securing the Troll objective makes this player an
        // additional winner on whatever normal faction/neutral ending happens later.
        if (OptionGroupSingleton<TrollOptions>.Instance.AfterWinType.Value == TrollAfterWinType.ContinuesGame)
        {
            return TrollSystem.IsWinner(Player.PlayerId);
        }

        return gameOverReason == CustomGameOver.GameOverReason<TrollGameOver>() &&
               TrollSystem.IsWinner(Player.PlayerId);
    }

    public override void Initialize(PlayerControl player)
    {
        RoleBehaviourStubs.Initialize(this, player);

        if (!OptionGroupSingleton<TrollOptions>.Instance.CanUseButton.Value)
        {
            player.RemainingEmergencies = 0;
        }
    }

    public override void Deinitialize(PlayerControl targetPlayer)
    {
        RoleBehaviourStubs.Deinitialize(this, targetPlayer);
        TouRoleUtils.ClearTaskHeader(Player);
    }

    public override bool CanUse(IUsable usable)
    {
        if (!GameManager.Instance.LogicUsables.CanUse(usable, Player))
        {
            return false;
        }

        // Troll behaves like the other Neutral Evil roles: any displayed task list
        // is fake/cosmetic, so ordinary Crewmate task consoles cannot be completed.
        var console = usable.TryCast<Console>()!;
        return console == null || console.AllowImpostor;
    }
}
