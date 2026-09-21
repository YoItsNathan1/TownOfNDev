using System;
using System.Collections.Generic;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.Roles;
using MiraAPI.Translation;
using MiraAPI.Utilities.Assets;
using TownOfNDev.Assets;
using TownOfUs;
using TownOfUs.Modules.Wiki;
using TownOfUs.Roles;
using TownOfUs.Roles.Impostor;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfNDev.Roles.Impostor;

public sealed class ReverserRole(IntPtr cppPtr) : ImpostorRole(cppPtr), ITownOfUsRole, IWikiDiscoverable
{
    public string IdPart => "Reverser";
    public string RoleName => MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}", "Reverser");
    public string RoleDescription => "Reverse direct attacks while Alert is active.";
    public string RoleMedDescription =>
        "Activate Alert to temporarily reverse ordinary direct kill attempts back onto the attacker.";
    public string RoleLongDescription =>
        "Activate Alert to enter Reverse mode for a limited time.\n" +
        "While Alert is active, ordinary direct kill attempts against you are cancelled and reflected onto the attacker.\n" +
        "Alert stays active for its full duration, so more than one attacker can be reversed during the same activation.\n" +
        "Indirect attacks, defense-bypassing attacks, meeting deaths and ejections are not reversed.\n" +
        "Meetings immediately end an active Alert.";

    public Color RoleColor => TownOfUsColors.Impostor;
    public ModdedRoleTeams Team => ModdedRoleTeams.Impostor;
    public RoleAlignment RoleAlignment => RoleAlignment.ImpostorPower;

    public CustomRoleConfiguration Configuration => new(this)
    {
        MaxRoleCount = 1,
        DefaultRoleCount = 0,
        DefaultChance = 0,
        UseVanillaKillButton = true,
        CanUseVent = true,
        CanUseSabotage = true,
        Icon = TownOfNDevAssets.ReverserRoleIcon,
        IconTmp = TmpSpriteUtils.CreateSpriteAsset(
            TownOfNDevAssets.ReverserRoleIcon.LoadAsset(),
            "TownOfNDev.Role.Impostor.Reverser",
            1.35f)
    };

    [HideFromIl2Cpp]
    public List<CustomButtonWikiDescription> Abilities { get; } =
    [
        new(
            "Alert",
            "Enter Reverse mode for the configured duration. Ordinary direct kill attempts are cancelled and reflected back onto the attacker while Alert remains active.",
            TownOfNDevAssets.ReverserAlertButton)
    ];

    public string GetAdvancedDescription() =>
        RoleLongDescription +
        " Reflected kills use the normal Town of Us murder pipeline so compatible protection mechanics on the attacker can still resolve normally." +
        MiscUtils.AppendOptionsText(GetType());
}
