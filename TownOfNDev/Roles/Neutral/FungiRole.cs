using AmongUs.GameOptions;
using MiraAPI.GameOptions;
using System;
using System.Collections.Generic;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameEnd;
using MiraAPI.Modifiers;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using MiraAPI.Translation;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using TownOfNDev.Assets;
using TownOfNDev.GameOver;
using TownOfNDev.Modifiers.Internal;
using TownOfNDev.Options;
using TownOfNDev.Systems;
using TownOfUs;
using TownOfUs.Modules.Wiki;
using TownOfUs.Roles;
using TownOfUs.Roles.Neutral;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfNDev.Roles.Neutral;

public sealed class FungiRole(IntPtr cppPtr) : NeutralRole(cppPtr), ITownOfUsRole, IWikiDiscoverable
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

    public string IdPart => "Fungi";
    public string RoleName => MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}", "Fungi");
    public string RoleDescription => "Spread a hidden fungal infection through the crew.";
    public string RoleMedDescription => "Infect players at close range. Infected players silently spread the fungus through successful interactions.";
    public string RoleLongDescription =>
        "Infect nearby players and let the fungus spread through successful interactions.\n" +
        "New infections begin hidden and become more visible after meetings.\n" +
        "Fungi can recognize infected players and each other.\n" +
        "Spread the infection far enough to complete your objective.";

    public Color RoleColor => TownOfNDevColors.Fungi;
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
        Icon = TownOfNDevAssets.FungiRoleIcon,
        IconTmp = TmpSpriteUtils.CreateSpriteAsset(
            TownOfNDevAssets.FungiRoleIcon.LoadAsset(),
            "TownOfNDev.Role.Neutral.Fungi",
            1.45f),
        GhostRole = (RoleTypes)RoleId.Get<NeutralGhostRole>()
    };

    [HideFromIl2Cpp]
    public List<CustomButtonWikiDescription> Abilities { get; } =
    [
        new(
            "Infect",
            "Infect the nearest valid player. The infection starts hidden and can spread to others through successful player-to-player interactions.",
            TownOfNDevAssets.FungiInfectButton)
    ];

    public string GetAdvancedDescription() =>
        RoleLongDescription +
        " Multiple Fungi work toward the same infection objective, while infected non-Fungi keep their original role." +
        MiscUtils.AppendOptionsText(GetType());

    public bool WinConditionMet()
    {
        if (OptionGroupSingleton<FungiOptions>.Instance.WinMode.Value != FungiWinMode.IndependentMustSurvive)
        {
            return false;
        }

        return FungiInfectionSystem.AnyFungiAlive() && FungiInfectionSystem.CurrentThresholdMet;
    }

    public override bool DidWin(GameOverReason gameOverReason)
    {
        if (gameOverReason == CustomGameOver.GameOverReason<FungiGameOver>())
        {
            return true;
        }

        if (OptionGroupSingleton<FungiOptions>.Instance.WinMode.Value == FungiWinMode.AdditionalWinnerDeathAllowed)
        {
            return FungiInfectionSystem.ObjectiveAchieved;
        }

        return false;
    }

    public override void Initialize(PlayerControl player)
    {
        RoleBehaviourStubs.Initialize(this, player);
        FungiInfectionSystem.RegisterFungi(player);

        if (!player.HasModifier<FungiFactionMarkerModifier>())
        {
            player.AddModifier<FungiFactionMarkerModifier>();
        }
    }


    public override bool CanUse(IUsable usable)
    {
        if (!GameManager.Instance.LogicUsables.CanUse(usable, Player))
        {
            return false;
        }

        // Match TOU's Neutral Evil behaviour (e.g. Jester/Doomsayer):
        // Fungi may use normal non-task interactables, but ordinary task consoles
        // are only usable when the console explicitly allows Impostor-style access.
        // This keeps any displayed task list cosmetic/fake and prevents Fungi from
        // completing Crewmate task progress.
        var console = usable.TryCast<Console>()!;
        return console == null || console.AllowImpostor;
    }

    public override void Deinitialize(PlayerControl targetPlayer)
    {
        // Match TOU's NeutralRole cleanup path so a Fungi role change does not
        // leave the Neutral Evil task header behind. Keep the faction marker
        // and member id intact so an already-earned/shared Fungi objective is
        // still evaluated correctly after role changes.
        RoleBehaviourStubs.Deinitialize(this, targetPlayer);
        TouRoleUtils.ClearTaskHeader(Player);
    }

    public bool CanLocalPlayerSeeRole(PlayerControl player)
    {
        if (PlayerControl.LocalPlayer &&
            FungiInfectionSystem.IsFungi(PlayerControl.LocalPlayer) &&
            FungiInfectionSystem.IsFungi(player))
        {
            return true;
        }

        return PlayerControl.LocalPlayer?.Data?.IsDead == true;
    }

    public bool SetupIntroTeam(
        IntroCutscene instance,
        ref Il2CppSystem.Collections.Generic.List<PlayerControl> yourTeam)
    {
        var fungiTeam = new Il2CppSystem.Collections.Generic.List<PlayerControl>();
        foreach (var player in PlayerControl.AllPlayerControls)
        {
            if (player && FungiInfectionSystem.IsFungi(player))
            {
                fungiTeam.Add(player);
            }
        }

        if (fungiTeam.Count == 0 && PlayerControl.LocalPlayer)
        {
            fungiTeam.Add(PlayerControl.LocalPlayer);
        }

        yourTeam = fungiTeam;
        return true;
    }
}
