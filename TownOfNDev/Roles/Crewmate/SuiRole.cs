using System;
using System.Collections.Generic;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.Roles;
using MiraAPI.Translation;
using MiraAPI.Utilities.Assets;
using TownOfNDev.Assets;
using TownOfUs.Modules.Wiki;
using TownOfUs.Roles;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfNDev.Roles.Crewmate;

public sealed class SuiRole(IntPtr cppPtr) : CrewmateRole(cppPtr), ITownOfUsRole, IWikiDiscoverable
{
    public string IdPart => "SUI";
    public string RoleName => MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}", "SUI");
    public string RoleDescription => "Protect the crew from lethal attacks.";
    public string RoleMedDescription =>
        "Protect multiple players from lethal attacks. The first attacker to trigger a protection is marked red for you to hunt.";
    public string RoleLongDescription =>
        "Protect living players from lethal attacks.\n" +
        "Protections last while you are alive.\n" +
        "The first attacker to trigger protection\n" +
        "is marked RED and unlocks your Kill button.\n" +
        "Only that marked attacker can be killed.\n" +
        "Meeting guesses and ejections bypass protection.";

    public Color RoleColor => TownOfNDevColors.Sui;
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;
    public RoleAlignment RoleAlignment => RoleAlignment.CrewmateProtective;

    public CustomRoleConfiguration Configuration => new(this)
    {
        MaxRoleCount = 1,
        DefaultRoleCount = 0,
        DefaultChance = 0,
        UseVanillaKillButton = true,
        KillButtonOutlineColor = Color.red,
        Icon = TownOfNDevAssets.SuiRoleIcon,
        IconTmp = TmpSpriteUtils.CreateSpriteAsset(
            TownOfNDevAssets.SuiRoleIcon.LoadAsset(),
            "TownOfNDev.Role.Crewmate.SUI",
            1.45f)
    };

    [HideFromIl2Cpp]
    public List<CustomButtonWikiDescription> Abilities { get; } =
    [
        new(
            "Protect",
            "Protect the nearest valid player. Protection lasts while you are alive; the first attacker to trigger it is marked red and becomes your hunt target.",
            TownOfNDevAssets.SuiProtectButton)
    ];

    public string GetAdvancedDescription() =>
        RoleLongDescription +
        " A protected player knows they are protected. You cannot protect yourself or manually remove a protection once it has been applied. While a hunt is active, your Kill button can only target the marked attacker." +
        MiscUtils.AppendOptionsText(GetType());
}
