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

public sealed class CondemnerRole(IntPtr cppPtr) : ImpostorRole(cppPtr), ITownOfUsRole, IWikiDiscoverable
{
    public string IdPart => "Condemner";
    public string RoleName => MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}", "Condemner");
    public string RoleDescription => "Pass sentence in silence.";
    public string RoleMedDescription =>
        "Secretly place players on Death Row. Their sentence is revealed at the next meeting and carried out when voting ends.";
    public string RoleLongDescription =>
        "Use Death Note on a nearby non-Impostor to secretly place them on Death Row.\n" +
        "At the next meeting, condemned players are publicly marked with a red skull.\n" +
        "When voting is processed, every valid Death Row sentence is executed at once.\n" +
        "Using Death Note resets your normal Kill cooldown.";

    public Color RoleColor => TownOfUsColors.Impostor;
    public ModdedRoleTeams Team => ModdedRoleTeams.Impostor;
    public RoleAlignment RoleAlignment => RoleAlignment.ImpostorKilling;

    public CustomRoleConfiguration Configuration => new(this)
    {
        MaxRoleCount = 1,
        DefaultRoleCount = 0,
        DefaultChance = 0,
        UseVanillaKillButton = true,
        CanUseVent = true,
        CanUseSabotage = true,
        Icon = TownOfNDevAssets.CondemnerRoleIcon,
        IconTmp = TmpSpriteUtils.CreateSpriteAsset(
            TownOfNDevAssets.CondemnerRoleIcon.LoadAsset(),
            "TownOfNDev.Role.Impostor.Condemner",
            1.45f)
    };

    [HideFromIl2Cpp]
    public List<CustomButtonWikiDescription> Abilities { get; } =
    [
        new(
            "Death Note",
            "Secretly condemn the nearest valid non-Impostor. You privately track condemned players with a Death Note marker; the target learns about the sentence only when the next meeting reveals the public Death Row skull.",
            TownOfNDevAssets.CondemnerDeathNoteButton)
    ];

    public string GetAdvancedDescription() =>
        RoleLongDescription +
        " Death Row targets remain alive and can still vote, guess, and use meeting abilities until their sentence is resolved. Interaction protection can prevent Death Note from taking effect, while ordinary kill protection does not stop a sentence." +
        MiscUtils.AppendOptionsText(GetType());
}
