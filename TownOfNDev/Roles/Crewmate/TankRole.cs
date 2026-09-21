using System;
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

public sealed class TankRole(IntPtr cppPtr) : CrewmateRole(cppPtr), ITownOfUsRole, IWikiDiscoverable
{
    public string IdPart => "Tank";
    public string RoleName => MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}", "Tank");
    public string RoleDescription => "Finish your tasks to resist direct kills.";
    public string RoleMedDescription =>
        "Complete all of your tasks to gain permanent protection from ordinary direct kill attempts.";
    public string RoleLongDescription =>
        "Complete all of your assigned tasks to activate Tank protection.\n" +
        "Once active, ordinary direct kill attempts against you are blocked.\n" +
        "Indirect attacks, defense-bypassing attacks, meeting kills and ejections are not blocked.\n" +
        "If enabled, you are told who tried to kill you when Tank protection blocks an attack.";

    public Color RoleColor => TownOfNDevColors.Tank;
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;
    public RoleAlignment RoleAlignment => RoleAlignment.CrewmateProtective;

    public CustomRoleConfiguration Configuration => new(this)
    {
        MaxRoleCount = 1,
        DefaultRoleCount = 0,
        DefaultChance = 0,
        Icon = TownOfNDevAssets.TankRoleIcon,
        IconTmp = TmpSpriteUtils.CreateSpriteAsset(
            TownOfNDevAssets.TankRoleIcon.LoadAsset(),
            "TownOfNDev.Role.Crewmate.Tank",
            1.45f)
    };

    public string GetAdvancedDescription() =>
        RoleLongDescription + MiscUtils.AppendOptionsText(GetType());
}
