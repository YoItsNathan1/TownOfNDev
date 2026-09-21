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

public sealed class TracerRole(IntPtr cppPtr) : CrewmateRole(cppPtr), ITownOfUsRole, IWikiDiscoverable
{
    public string IdPart => "Tracer";
    public string RoleName => MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}", "Tracer");
    public string RoleDescription => "Follow the evidence.";
    public string RoleMedDescription =>
        "Dust a player with invisible trace powder. Anyone who directly interacts with them leaves evidence only you can see.";
    public string RoleLongDescription =>
        "Dust a player with invisible trace powder.\n" +
        "Anyone who directly interacts with your Dusted target leaves private evidence only you can see.\n" +
        "Trace does not spread beyond that first contact.\n" +
        "Use the evidence to work out who has been near your target and why.";

    public Color RoleColor => TownOfNDevColors.Tracer;
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;
    public RoleAlignment RoleAlignment => RoleAlignment.CrewmateInvestigative;

    public CustomRoleConfiguration Configuration => new(this)
    {
        MaxRoleCount = 2,
        DefaultRoleCount = 0,
        DefaultChance = 0,
        Icon = TownOfNDevAssets.TracerRoleIcon,
        IconTmp = TmpSpriteUtils.CreateSpriteAsset(
            TownOfNDevAssets.TracerRoleIcon.LoadAsset(),
            "TownOfNDev.Role.Crewmate.Tracer",
            1.45f)
    };

    [HideFromIl2Cpp]
    public List<CustomButtonWikiDescription> Abilities { get; } =
    [
        new(
            "Dust",
            "Secretly dust the nearest valid player. For a short time, successful direct interactions with that player leave private Trace evidence on the other participant.",
            TownOfNDevAssets.TracerDustButton)
    ];

    public string GetAdvancedDescription() =>
        RoleLongDescription +
        " The Dusted player is not notified. Trace evidence can reveal direct contact, including a killer who murders your target, and remains until after the next meeting." +
        MiscUtils.AppendOptionsText(GetType());

}
