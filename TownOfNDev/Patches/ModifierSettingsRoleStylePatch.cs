using System;
using System.Collections.Generic;
using System.Linq;
using AmongUs.GameOptions;
using HarmonyLib;
using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Patches.Options;
using MiraAPI.Roles;
using MiraAPI.Translation;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using Reactor.Utilities.Extensions;
using TMPro;
using TownOfNDev.Assets;
using TownOfNDev.Options;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace TownOfNDev.Patches;

/// <summary>
/// Renders TownOfNDev modifiers inside the actual RolesSettingsMenu instead of trying to
/// imitate role rows inside GameOptionsMenu. This keeps the modifier screen on the exact
/// same coordinate system, templates, mask and scrollbar as the normal role settings page.
/// </summary>
public static class ModifierSettingsRoleStylePatch
{
    private const string PluginGuid = "ndevstudios.townofndev";
    private const string ContainerName = "TownOfNDev.ModifierRoleSettings";

    private static GameObject? _modifierContainer;
    private static Transform? _containerParent;
    private static readonly List<ModifierBinding> Bindings = [];
    private static readonly Dictionary<string, bool> GroupCollapsed = new(StringComparer.Ordinal);
    private static readonly List<OptionBehaviour> AdvancedBehaviours = [];
    private static readonly List<RoleOptionSetting> ModifierRows = [];
    private static float _listBottomY = 0.522f;
    private static bool _isVisible;

    public static bool IsVisible => _isVisible || (_modifierContainer is not null && _modifierContainer && _modifierContainer.activeSelf);

    public static bool IsTownOfNDevCurrentMod()
    {
        var menuState = MenuState.Instance;
        if (menuState is null || !menuState || menuState.CurrentModIdx <= 0)
        {
            return false;
        }

        try
        {
            return string.Equals(menuState.CurrentMod.PluginId, PluginGuid, StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }

    public static bool IsTownOfNDevModifierMenu()
    {
        var menuState = MenuState.Instance;
        return menuState is not null && menuState && IsTownOfNDevCurrentMod() &&
               menuState.CurrentMenu == MenuCategory.Modifiers;
    }

    public static void Show(GameSettingMenu menu, bool previewOnly)
    {
        if (!IsTownOfNDevModifierMenu())
        {
            return;
        }

        var roleMenu = menu.RoleSettingsTab;
        var originalRolesContainer = GetOriginalRolesContainer();

        // The modifier page borrows RolesSettingsMenu purely as a visual host. Keep the
        // component ENABLED: a fresh Game Settings screen can legitimately have the Roles
        // component disabled before the user has ever opened Roles. Capturing/restoring that
        // disabled state caused two linked bugs:
        //   1) Modifiers could be blank until Roles had been opened once.
        //   2) returning to Roles could leave RolesSettingsMenu.Update disabled, so role
        //      advanced options were created but never positioned/activated.
        //
        // Mira's normal role quota builder is already blocked by
        // TownOfNDevModifierRoleBuildGuardPatch while CurrentMenu == Modifiers, so there is no
        // need to disable RolesSettingsMenu itself.
        if (originalRolesContainer is not null && originalRolesContainer)
        {
            originalRolesContainer.SetActive(false);
        }

        menu.GameSettingsTab.gameObject.SetActive(false);
        StopMiraRoleBuild(roleMenu);
        ClearAdvancedOptionBehaviours(roleMenu);

        EnsureContainer(roleMenu, originalRolesContainer);
        var modifierContainer = _modifierContainer;
        if (modifierContainer is null || !modifierContainer)
        {
            return;
        }

        // Point the host at the modifier container before activating the Roles host. This lets a
        // first-ever Modifiers visit initialise RolesSettingsMenu normally without exposing or
        // building the real Roles list. OpenChancesTab is guarded while Modifiers is current.
        roleMenu.RoleChancesSettings = modifierContainer;
        roleMenu.scrollBar.Inner = modifierContainer.transform;
        roleMenu.AdvancedRolesSettings.SetActive(false);
        modifierContainer.SetActive(false);

        roleMenu.enabled = true;
        _isVisible = true;
        roleMenu.gameObject.SetActive(true);

        // OnEnable may touch host state, so clear any attempted role build and then construct the
        // modifier page against a fully initialised RolesSettingsMenu.
        StopMiraRoleBuild(roleMenu);
        ConfigureRoleMenuFrame(roleMenu);
        BuildBindings();
        BuildList(menu, roleMenu);

        // Reassert after activation/build so another patch cannot swap the shared host back to
        // the actual Roles container in the same frame.
        if (originalRolesContainer is not null && originalRolesContainer)
        {
            originalRolesContainer.SetActive(false);
        }
        roleMenu.AllButton.transform.parent.gameObject.SetActive(false);
        roleMenu.AllButton.gameObject.SetActive(false);
        roleMenu.RoleChancesSettings = modifierContainer;
        roleMenu.scrollBar.Inner = modifierContainer.transform;
        modifierContainer.SetActive(true);

        roleMenu.scrollBar.SetYBoundsMax(Mathf.Max(0f, -_listBottomY - 2f));
        roleMenu.scrollBar.ScrollToTop();

        if (!previewOnly)
        {
            roleMenu.ControllerSelectable.Clear();
            foreach (var row in ModifierRows)
            {
                roleMenu.ControllerSelectable.AddRange(
                    new Il2CppSystem.Collections.Generic.IEnumerable<UiElement>(row.ControllerSelectable.Pointer));
            }
        }
    }

    private static GameObject? GetOriginalRolesContainer()
    {
        var menuState = MenuState.Instance;
        if (menuState is null || !menuState || menuState.CurrentModIdx <= 0)
        {
            return null;
        }

        if (!menuState.Containers.TryGetValue(MenuCategory.Roles, out var containers) ||
            !containers.TryGetValue(menuState.CurrentModIdx, out var rolesContainer))
        {
            return null;
        }

        return rolesContainer;
    }

    public static void Hide(GameSettingMenu menu)
    {
        if (!IsVisible)
        {
            return;
        }

        _isVisible = false;

        var modifierContainer = _modifierContainer;
        if (modifierContainer is not null && modifierContainer)
        {
            modifierContainer.SetActive(false);
        }

        foreach (var behaviour in AdvancedBehaviours.ToArray())
        {
            if (behaviour is not null && behaviour && behaviour.gameObject)
            {
                Object.Destroy(behaviour.gameObject);
            }
        }
        AdvancedBehaviours.Clear();

        var roleMenu = menu.RoleSettingsTab;
        roleMenu.AdvancedRolesSettings.SetActive(false);
        ConfigureRoleReturnButton(roleMenu);

        var menuState = MenuState.Instance;
        if (menuState is null || !menuState)
        {
            roleMenu.gameObject.SetActive(false);
            roleMenu.enabled = true;
            return;
        }

        var originalRolesContainer = GetOriginalRolesContainer();

        // Always restore the REAL Roles container pointer, even when this method runs in the
        // prefix before MenuState has changed CurrentMenu away from Modifiers. That way Mira's
        // subsequent Roles OpenMenu/OpenChancesTab call can never start against our modifier
        // container.
        if (originalRolesContainer is not null && originalRolesContainer)
        {
            roleMenu.RoleChancesSettings = originalRolesContainer;
            roleMenu.scrollBar.Inner = originalRolesContainer.transform;
        }

        roleMenu.enabled = true;

        if (menuState.CurrentMenu == MenuCategory.Roles)
        {
            if (originalRolesContainer is not null && originalRolesContainer)
            {
                originalRolesContainer.SetActive(true);
            }
            roleMenu.gameObject.SetActive(true);
        }
        else
        {
            if (originalRolesContainer is not null && originalRolesContainer)
            {
                originalRolesContainer.SetActive(false);
            }
            roleMenu.gameObject.SetActive(false);
        }
    }

    private static void ClearAdvancedOptionBehaviours(RolesSettingsMenu roleMenu)
    {
        // RolesSettingsMenu keeps the previous role's advanced OptionBehaviour objects alive
        // when switching away from Roles. If we then add modifier options to the same
        // AdvancedRolesSettings container, both sets render on top of each other (for example
        // SUI's Protect Cooldown underneath Menace/Butterfingers settings). Mirror Mira's own
        // role ChangeTab cleanup and remove every existing option before modifier UI is built.
        foreach (var optionBehaviour in roleMenu.AdvancedRolesSettings.GetComponentsInChildren<OptionBehaviour>(true))
        {
            if (optionBehaviour is not null && optionBehaviour && optionBehaviour.gameObject)
            {
                Object.Destroy(optionBehaviour.gameObject);
            }
        }

        AdvancedBehaviours.Clear();
        roleMenu.advancedSettingChildren.Clear();
    }

    private static void StopMiraRoleBuild(RolesSettingsMenu roleMenu)
    {
        // If the user came here from a role's advanced settings, Mira keeps CurrentRole and
        // CurrentRoleOptions around and its Update patch would continue re-laying out that role.
        // Clear those private state values so the stock role patch leaves this modifier screen alone.
        AccessTools.Property(typeof(RoleSettingMenuPatches), "CurrentRole")?.SetValue(null, null);
        AccessTools.Property(typeof(RoleSettingMenuPatches), "CurrentRoleOptions")?.SetValue(null, null);

        var coroutineField = AccessTools.Field(typeof(RoleSettingMenuPatches), "_quotaTabCoroutine");
        var gameSettingMenu = GameSettingMenu.Instance;
        if (coroutineField?.GetValue(null) is Coroutine coroutine &&
            gameSettingMenu is not null && gameSettingMenu)
        {
            gameSettingMenu.StopCoroutine(coroutine);
            coroutineField.SetValue(null, null);
        }
    }

    private static void EnsureContainer(RolesSettingsMenu roleMenu, GameObject? originalRolesContainer)
    {
        var sourceContainer = originalRolesContainer is not null && originalRolesContainer
            ? originalRolesContainer
            : roleMenu.RoleChancesSettings;
        if (sourceContainer is null || !sourceContainer)
        {
            return;
        }

        var expectedParent = sourceContainer.transform.parent;
        if (expectedParent is null || !expectedParent)
        {
            return;
        }
        var existingModifierContainer = _modifierContainer;
        if (existingModifierContainer is not null && existingModifierContainer && _containerParent == expectedParent)
        {
            return;
        }

        if (existingModifierContainer is not null && existingModifierContainer)
        {
            Object.Destroy(existingModifierContainer);
        }

        _containerParent = expectedParent;
        _modifierContainer = new GameObject(ContainerName)
        {
            layer = sourceContainer.layer
        };
        _modifierContainer.transform.SetParent(expectedParent, false);
        _modifierContainer.transform.localPosition = sourceContainer.transform.localPosition;
        _modifierContainer.transform.localRotation = sourceContainer.transform.localRotation;
        _modifierContainer.transform.localScale = sourceContainer.transform.localScale;
        _modifierContainer.SetActive(false);
    }

    private static void ConfigureRoleMenuFrame(RolesSettingsMenu roleMenu)
    {
        roleMenu.selectedRoleTab = 0;
        roleMenu.QuotaTabSelectables ??= new();
        roleMenu.roleChances ??= new();
        roleMenu.advancedSettingChildren ??= new();
        roleMenu.QuotaTabSelectables.Clear();
        roleMenu.roleChances.Clear();
        roleMenu.advancedSettingChildren.Clear();

        // These values are copied from MiraAPI's custom-role branch in CoQuotaTabPatch.
        // This is the frame used by TownOfNDev's normal Roles page.
        roleMenu.AllButton.transform.parent.gameObject.SetActive(false);
        roleMenu.AllButton.gameObject.SetActive(false);
        roleMenu.scrollBar.transform.localPosition = new Vector3(-1.4957f, 1.5261f, -4f);

        var maskBg = roleMenu.scrollBar.transform.FindChild("MaskBg");
        if (maskBg)
        {
            maskBg.localPosition = new Vector3(1.5353f, -1.0607f, -.1f);
            maskBg.localScale = new Vector3(6.6811f, 4.1563f, 0.5598f);
        }

        var hitbox = roleMenu.scrollBar.transform.FindChild("Hitbox");
        if (hitbox)
        {
            hitbox.localPosition = new Vector3(0.3297f, -.6333f, 4f);
            hitbox.localScale = new Vector3(1f, 1.2f, 1f);
        }

        var dividerImage = roleMenu.transform.FindChild("HeaderButtons/DividerImage");
        if (dividerImage)
        {
            dividerImage.gameObject.SetActive(false);
        }
    }

    private static void BuildBindings()
    {
        Bindings.Clear();

        AddBinding("EyeMask", "Eye Mask", "Crewmate", TownOfNDevColors.EyeMask,
            TownOfNDevAssets.EyeMaskModifierIcon, OptionGroupSingleton<EyeMaskOptions>.Instance,
            OptionGroupSingleton<EyeMaskOptions>.Instance.AssignmentChance);

        AddBinding("Menace", "Menace", "Impostor", TownOfNDevColors.Menace,
            TownOfNDevAssets.MenaceModifierIcon, OptionGroupSingleton<MenaceOptions>.Instance,
            OptionGroupSingleton<MenaceOptions>.Instance.AssignmentChance);

        AddBinding("Butterfingers", "Butterfingers", "Universal", TownOfNDevColors.Butterfingers,
            TownOfNDevAssets.ButterfingersModifierIcon, OptionGroupSingleton<ButterfingersOptions>.Instance,
            OptionGroupSingleton<ButterfingersOptions>.Instance.AssignmentChance);

        AddBinding("Laggy", "Laggy", "Universal", TownOfNDevColors.Laggy,
            TownOfNDevAssets.LaggyModifierIcon, OptionGroupSingleton<LaggyOptions>.Instance,
            OptionGroupSingleton<LaggyOptions>.Instance.AssignmentChance);

        AddBinding("Soulhandler", "Soulhandler", "Universal", TownOfNDevColors.Soulhandler,
            TownOfNDevAssets.SoulhandlerModifierIcon, OptionGroupSingleton<SoulhandlerOptions>.Instance,
            OptionGroupSingleton<SoulhandlerOptions>.Instance.AssignmentChance);

        AddBinding("Miracle", "Miracle", "Universal", TownOfNDevColors.Miracle,
            TownOfNDevAssets.MiracleModifierIcon, OptionGroupSingleton<MiracleOptions>.Instance,
            OptionGroupSingleton<MiracleOptions>.Instance.AssignmentChance);

        AddBinding("Null", "Null", "Universal", TownOfNDevColors.Null,
            TownOfNDevAssets.NullModifierIcon, OptionGroupSingleton<NullOptions>.Instance,
            OptionGroupSingleton<NullOptions>.Instance.AssignmentChance);
    }

    private static void AddBinding(
        string key,
        string displayName,
        string group,
        Color color,
        LoadableAsset<Sprite> icon,
        AbstractOptionGroup optionGroup,
        ModdedNumberOption chance)
    {
        var amount = optionGroup.Children
            .OfType<ModdedNumberOption>()
            .FirstOrDefault(x => x.Title.Contains("Amount", StringComparison.OrdinalIgnoreCase));

        if (amount == null)
        {
            return;
        }

        Bindings.Add(new ModifierBinding(key, displayName, group, color, icon, optionGroup, amount, chance));
        GroupCollapsed.TryAdd(group, false);
    }

    private static void BuildList(GameSettingMenu menu, RolesSettingsMenu roleMenu)
    {
        var modifierContainer = _modifierContainer;
        if (modifierContainer is null || !modifierContainer)
        {
            return;
        }

        modifierContainer.transform.DestroyChildren();
        ModifierRows.Clear();
        roleMenu.roleChances.Clear();
        roleMenu.QuotaTabSelectables.Clear();

        var scrollerNum = 0.522f;
        var globalRowIndex = 0;
        var totalRows = Bindings.Count;
        var quotaTemplate = roleMenu.categoryHeaderEditRoleOrigin.transform.FindChild("QuotaHeader");
        var headerTemplate = menu.GameSettingsTab.categoryHeaderOrigin;

        // RoleOptionSetting's visual chrome (white +/- buttons, bordered black value boxes
        // and the centre divider) is initialised by SetRole(). We must not bind modifiers to a
        // dummy role, but we can safely initialise one hidden throwaway row using a real
        // TownOfNDev role, clone its fully-initialised visuals, and then strip all role state
        // from each modifier clone. This gives us the exact same UI as the Roles page without
        // making Mira/Among Us think a modifier row is an actual role.
        var visualTemplate = CreateInitialisedRoleVisualTemplate(roleMenu, modifierContainer.transform);

        foreach (var group in new[] { "Crewmate", "Impostor", "Universal" })
        {
            var members = Bindings.Where(x => x.Group == group).ToList();
            if (members.Count == 0)
            {
                continue;
            }

            var collapsed = GroupCollapsed.TryGetValue(group, out var hidden) && hidden;
            var header = Object.Instantiate(
                headerTemplate,
                Vector3.zero,
                Quaternion.identity,
                modifierContainer.transform);

            header.name = $"TownOfNDev.ModifierHeader.{group}";
            header.SetHeader(StringNames.None, 20);
            if (header.Title.TryGetComponent<TextTranslatorTMP>(out var titleTranslator))
            {
                Object.Destroy(titleTranslator);
            }
            header.Title.text = $"{group} Modifiers";

            var quota = Object.Instantiate(quotaTemplate, header.transform);
            quota.name = "TownOfNDev.ModifierQuotaHeader";
            quota.localScale = new Vector3(1.3f, 1.3f, 1.3f);
            quota.localPosition = new Vector3(0.7f, -0.82f, 0f);

            var chanceText = quota.FindChild("Chance Text");
            if (chanceText)
            {
                chanceText.localPosition = new Vector3(4.3f, 0.0993f, 0f);
            }

            var countText = quota.FindChild("# Text");
            if (countText)
            {
                countText.localPosition = new Vector3(1.9f, 0.0993f, 0f);
            }

            foreach (var childName in new[] { "BlankLabel", "Chance Label", "# Label" })
            {
                var child = quota.FindChild(childName);
                if (child)
                {
                    Object.Destroy(child.gameObject);
                }
            }

            header.Background.sprite = MiraAssets.CategoryHeader.LoadAsset();
            header.Background.sprite.texture.filterMode = FilterMode.Bilinear;
            header.Background.sprite.texture.wrapMode = TextureWrapMode.Clamp;
            header.Background.transform.localPosition = new Vector3(0.55f, -0.1833f, 0f);
            header.Background.size = new Vector2(header.Background.size.x + 1.5f, header.Background.size.y);

            ApplyHeaderColor(header, group);
            header.Title.fontStyle = roleMenu.categoryHeaderEditRoleOrigin.Title.fontStyle;
            header.Title.font = roleMenu.categoryHeaderEditRoleOrigin.Title.font;
            header.Title.fontMaterial = roleMenu.categoryHeaderEditRoleOrigin.Title.fontMaterial;
            header.transform.localScale = Vector3.one * 0.63f;
            header.transform.localPosition = new Vector3(-0.44f, scrollerNum, -2f);
            header.gameObject.SetActive(true);
            quota.gameObject.SetActive(!collapsed);

            var toggleText = Object.Instantiate(header.Title, header.transform);
            toggleText.name = "TownOfNDev.ModifierHeaderToggleText";
            if (toggleText.TryGetComponent<TextTranslatorTMP>(out var toggleTranslator))
            {
                Object.Destroy(toggleTranslator);
            }
            toggleText.text = $"<size=70%>({(collapsed ? "Click to open" : "Click to close")})</size>";
            toggleText.transform.localPosition = new Vector3(2.6249f, -0.165f, 0f);

            var boxCol = header.gameObject.AddComponent<BoxCollider2D>();
            boxCol.size = new Vector2(7f, 0.7f);
            boxCol.offset = new Vector2(1.5f, -0.3f);

            var headerBtn = header.gameObject.AddComponent<PassiveButton>();
            headerBtn.ClickSound = roleMenu.BackButton.GetComponent<PassiveButton>().ClickSound;
            headerBtn.OnMouseOver = new UnityEvent();
            headerBtn.OnMouseOut = new UnityEvent();
            headerBtn.OnClick = new Button.ButtonClickedEvent();
            var capturedGroup = group;
            headerBtn.OnClick.AddListener((UnityAction)(() =>
            {
                GroupCollapsed[capturedGroup] = !GroupCollapsed[capturedGroup];
                BuildList(menu, roleMenu);
                roleMenu.scrollBar.Inner = modifierContainer.transform;
                roleMenu.scrollBar.SetYBoundsMax(Mathf.Max(0f, -_listBottomY - 2f));
            }));
            headerBtn.SetButtonEnableState(true);

            scrollerNum -= 0.422f;

            if (!collapsed)
            {
                foreach (var binding in members)
                {
                    CreateModifierRow(menu, roleMenu, binding, scrollerNum, visualTemplate);
                    globalRowIndex++;
                    if (globalRowIndex < totalRows)
                    {
                        scrollerNum -= 0.43f;
                    }
                }

                // Same extra group gap used by Mira's custom Roles screen.
                scrollerNum -= 0.4f;
            }
        }

        _listBottomY = scrollerNum;

        if (visualTemplate is not null && visualTemplate)
        {
            Object.Destroy(visualTemplate.gameObject);
        }
    }

    private static RoleOptionSetting? CreateInitialisedRoleVisualTemplate(
        RolesSettingsMenu roleMenu,
        Transform parent)
    {
        try
        {
            // Prefer an already-built TownOfNDev role row because it is guaranteed to contain
            // the exact visual chrome Mira currently uses.
            var originalRolesContainer = GetOriginalRolesContainer();
            var existingRoleRow = originalRolesContainer is not null && originalRolesContainer
                ? originalRolesContainer
                    .GetComponentsInChildren<RoleOptionSetting>(true)
                    .FirstOrDefault(row => row is not null && row)
                : null;

            if (existingRoleRow is not null && existingRoleRow)
            {
                return PrepareVisualTemplate(Object.Instantiate(
                    existingRoleRow,
                    Vector3.zero,
                    Quaternion.identity,
                    parent));
            }

            // First-entry path: the Roles page may never have been opened, so no role row exists
            // to clone yet. Initialise a hidden copy of the stock row with a real TownOfNDev role
            // purely to obtain the same button/number-box/divider visuals. This avoids forcing
            // users to open Roles once before Modifiers looks correct.
            var donorRole = CustomRoleManager.CustomMiraRoles
                .FirstOrDefault(role =>
                {
                    try
                    {
                        return string.Equals(
                            CustomRoleManager.FindParentMod(role).PluginId,
                            PluginGuid,
                            StringComparison.OrdinalIgnoreCase);
                    }
                    catch
                    {
                        return false;
                    }
                }) as RoleBehaviour;

            if (donorRole is null || !donorRole)
            {
                return null;
            }

            var generated = Object.Instantiate(
                roleMenu.roleOptionSettingOrigin,
                Vector3.zero,
                Quaternion.identity,
                parent);
            generated.SetRole(GameOptionsManager.Instance.CurrentGameOptions.RoleOptions, donorRole, 20);
            return PrepareVisualTemplate(generated);
        }
        catch
        {
            return null;
        }
    }

    private static RoleOptionSetting PrepareVisualTemplate(RoleOptionSetting template)
    {
        template.name = "TownOfNDev.ModifierVisualTemplate";
        template.enabled = false;

        // Custom role rows may already contain their own role icon and config cog. Keep only the
        // stock quota chrome; modifier-specific icon/cog controls are added later.
        var inheritedRoleIcon = template.transform.FindChild("RoleIcon");
        if (inheritedRoleIcon)
        {
            Object.DestroyImmediate(inheritedRoleIcon.gameObject);
        }

        var inheritedConfigButton = template.transform.FindChild("ConfigButton");
        if (inheritedConfigButton)
        {
            Object.DestroyImmediate(inheritedConfigButton.gameObject);
        }

        template.gameObject.SetActive(false);
        return template;
    }

    private static void ApplyHeaderColor(CategoryHeaderMasked header, string group)
    {
        switch (group)
        {
            case "Crewmate":
                header.Title.color = Palette.CrewmateRoleHeaderTextBlue;
                header.Background.color = Palette.CrewmateRoleHeaderBlue;
                break;
            case "Impostor":
                header.Title.color = Palette.ImpostorRoleHeaderTextRed;
                header.Background.color = Palette.ImpostorRoleHeaderRed;
                break;
            default:
                var universal = new Color32(108, 112, 122, 255);
                header.Title.color = Color.white;
                header.Background.color = universal;
                break;
        }

        header.Divider.color = header.Background.color;
    }

    private static void CreateModifierRow(
        GameSettingMenu menu,
        RolesSettingsMenu roleMenu,
        ModifierBinding binding,
        float y,
        RoleOptionSetting? visualTemplate)
    {
        var modifierContainer = _modifierContainer;
        if (modifierContainer is null || !modifierContainer)
        {
            return;
        }

        var row = visualTemplate is not null && visualTemplate
            ? Object.Instantiate(
                visualTemplate,
                Vector3.zero,
                Quaternion.identity,
                modifierContainer.transform)
            : Object.Instantiate(
                roleMenu.roleOptionSettingOrigin,
                Vector3.zero,
                Quaternion.identity,
                modifierContainer.transform);

        row.gameObject.SetActive(true);
        row.name = $"TownOfNDev.ModifierRow.{binding.Key}";
        row.transform.localPosition = new Vector3(-0.1f, y, -2f);
        row.SetClickMask(roleMenu.ButtonClickMask);

        // This row uses RoleOptionSetting only as a visual prefab. A cloned role row retains
        // its read-only Role reference, but the component is disabled and the row is never added
        // to Mira's roleChances collection. All modifier value changes are handled by our child
        // GameOptionButtons, so the retained reference cannot drive stock role settings updates.
        row.OnValueChanged = null;
        row.enabled = false;
        UpdateRowTexts(row, binding);
        row.titleText.text = binding.DisplayName;

        // RoleOptionSetting's prefab is normally initialised by SetRole(), which enables the
        // coloured label panel. Modifier rows deliberately do not call SetRole() (otherwise
        // Among Us/Mira treats them as real roles), so explicitly initialise only the visual
        // label renderer here. This gives modifiers the same coloured background strip as
        // TownOfNDev role rows without binding a fake RoleBehaviour.
        var labelColor = binding.Color;
        labelColor.a = 1f;
        row.labelSprite.gameObject.SetActive(true);
        row.labelSprite.enabled = true;
        row.labelSprite.color = labelColor;
        row.labelSprite.material.SetInt(PlayerMaterial.MaskLayer, 20);

        row.titleText.transform.localPosition = new Vector3(-0.25f, -0.2923f, 0f);
        row.titleText.color = binding.Color.FindAlternateColor();
        row.titleText.horizontalAlignment = HorizontalAlignmentOptions.Left;

        CreateModifierIcon(row, binding);
        ConfigureButton(row.CountMinusBtn, roleMenu.roleOptionSettingOrigin.CountMinusBtn,
            () => ChangeNumber(binding.Amount, -1, row, binding));
        ConfigureButton(row.CountPlusBtn, roleMenu.roleOptionSettingOrigin.CountPlusBtn,
            () => ChangeNumber(binding.Amount, 1, row, binding));
        ConfigureButton(row.ChanceMinusBtn, roleMenu.roleOptionSettingOrigin.ChanceMinusBtn,
            () => ChangeNumber(binding.Chance, -1, row, binding));
        ConfigureButton(row.ChancePlusBtn, roleMenu.roleOptionSettingOrigin.ChancePlusBtn,
            () => ChangeNumber(binding.Chance, 1, row, binding));

        // The initialised role template supplies the same white button boxes, black bordered
        // numeric fields and vertical separator used by the Roles page. Re-assert the visible
        // state in case SetInteractable/clone initialisation changed any renderer state.
        RestoreRoleRowChrome(row);

        if (binding.AdvancedOptions.Count > 0)
        {
            CreateGearButton(menu, roleMenu, row, binding);
        }

        var host = !AmongUsClient.Instance || AmongUsClient.Instance.AmHost;
        row.CountMinusBtn.SetInteractable(host);
        row.CountPlusBtn.SetInteractable(host);
        row.ChanceMinusBtn.SetInteractable(host);
        row.ChancePlusBtn.SetInteractable(host);

        ModifierRows.Add(row);
        roleMenu.QuotaTabSelectables.AddRange(
            new Il2CppSystem.Collections.Generic.IEnumerable<UiElement>(row.ControllerSelectable.Pointer));
    }

    private static void UpdateRowTexts(RoleOptionSetting row, ModifierBinding binding)
    {
        row.roleMaxCount = Mathf.RoundToInt(binding.Amount.Value);
        row.roleChance = Mathf.RoundToInt(binding.Chance.Value);
        row.countText.text = row.roleMaxCount.ToString();
        row.chanceText.text = row.roleChance.ToString();
        row.titleText.text = binding.DisplayName;
        var labelColor = binding.Color;
        labelColor.a = 1f;
        row.labelSprite.gameObject.SetActive(true);
        row.labelSprite.enabled = true;
        row.labelSprite.color = labelColor;
    }

    private static void CreateModifierIcon(RoleOptionSetting row, ModifierBinding binding)
    {
        var iconObject = new GameObject("ModifierIcon")
        {
            layer = LayerMask.NameToLayer("UI")
        };
        iconObject.transform.SetParent(row.transform, false);
        iconObject.transform.localPosition = new Vector3(-1.3f, -0.3f, -2f);

        var renderer = iconObject.AddComponent<SpriteRenderer>();
        var sprite = binding.Icon.LoadAsset();
        if (sprite is null)
        {
            Object.Destroy(iconObject);
            return;
        }

        renderer.sprite = sprite;
        renderer.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;

        var maxSize = Mathf.Max(sprite.bounds.size.x, sprite.bounds.size.y);
        var iconScale = maxSize > 0.001f ? 0.315f / maxSize : 0.05f;
        iconObject.transform.localScale = new Vector3(iconScale, iconScale, 1f);
    }

    private static void RestoreRoleRowChrome(RoleOptionSetting row)
    {
        foreach (var button in new[]
                 {
                     row.CountMinusBtn, row.CountPlusBtn, row.ChanceMinusBtn, row.ChancePlusBtn
                 })
        {
            button.gameObject.SetActive(true);
            var buttonSpriteTransform = button.transform.FindChild("ButtonSprite");
            if (buttonSpriteTransform)
            {
                buttonSpriteTransform.gameObject.SetActive(true);
                var renderer = buttonSpriteTransform.GetComponent<SpriteRenderer>();
                if (renderer)
                {
                    renderer.enabled = true;
                    renderer.color = Color.white;
                }
            }

            var buttonTextTransform = button.transform.FindChild("Text_TMP");
            if (buttonTextTransform)
            {
                buttonTextTransform.gameObject.SetActive(true);
                var buttonText = buttonTextTransform.GetComponent<TextMeshPro>();
                if (buttonText)
                {
                    buttonText.enabled = true;
                    buttonText.color = Color.black;
                }
            }
        }

        // These backgrounds/dividers live outside the four buttons. They are already correctly
        // configured on an initialised role row; just ensure the cloned renderers stayed active.
        foreach (var renderer in row.GetComponentsInChildren<SpriteRenderer>(true))
        {
            if (renderer == row.labelSprite)
            {
                continue;
            }

            var n = renderer.gameObject.name;
            if (n.Contains("Background", StringComparison.OrdinalIgnoreCase) ||
                n.Contains("Divider", StringComparison.OrdinalIgnoreCase) ||
                n.Contains("Outline", StringComparison.OrdinalIgnoreCase) ||
                n.Contains("Border", StringComparison.OrdinalIgnoreCase))
            {
                renderer.gameObject.SetActive(true);
                renderer.enabled = true;
            }
        }
    }

    private static void ConfigureButton(GameOptionButton button, GameOptionButton roleButtonSource, Action action)
    {
        // Copy the click clip from the stock RoleOptionSetting button itself. Modifier rows can be
        // cloned from a generated/hidden visual template, so relying on the clone to retain the
        // correct audio clip is not sufficient. Using the original quota-button source guarantees
        // the physical +/- click matches the Roles page.
        button.ClickSound = roleButtonSource.ClickSound;
        button.OnClick = new Button.ButtonClickedEvent();
        button.OnClick.AddListener((UnityAction)(() =>
        {
            if (!AmongUsClient.Instance || AmongUsClient.Instance.AmHost)
            {
                action();
            }
        }));
    }

    private static void ChangeNumber(
        ModdedNumberOption option,
        int direction,
        RoleOptionSetting row,
        ModifierBinding binding)
    {
        var next = option.Value + option.Increment * direction;
        if (next > option.Max)
        {
            next = option.Min;
        }
        else if (next < option.Min)
        {
            next = option.Max;
        }

        // Mirror the Roles quota UX in both directions:
        // - changing Chance above 0 while Amount is 0 enables one copy;
        // - enabling Amount from 0 while Chance is 0 defaults Chance to 50%.
        //
        // Mira's custom-role quota controls do the same thing: Increase/DecreaseChance enables
        // one role when starting at zero, while Increase/DecreaseCount seeds a zero chance to 50.
        if (ReferenceEquals(option, binding.Chance) && next > 0f && binding.Amount.Value <= 0f)
        {
            var enabledAmount = Mathf.Clamp(1f, binding.Amount.Min, binding.Amount.Max);
            binding.Amount.SetValue(enabledAmount);
        }

        if (ReferenceEquals(option, binding.Amount) &&
            binding.Amount.Value <= 0f &&
            next > 0f &&
            binding.Chance.Value <= 0f)
        {
            var defaultChance = Mathf.Clamp(50f, binding.Chance.Min, binding.Chance.Max);
            binding.Chance.SetValue(defaultChance);
        }

        option.SetValue(next);
        UpdateRowTexts(row, binding);
        NotifyModifierQuotaChanged(binding);
    }

    private static void NotifyModifierQuotaChanged(ModifierBinding binding)
    {
        if (!HudManager.InstanceExists || HudManager.Instance.Notifier is null)
        {
            return;
        }

        // Roles do not only make a button-click sound. Their quota ValueChanged path also calls
        // NotificationPopper.AddRoleSettingsChangeMessage(), whose SettingsChangeMessageLogic
        // plays the distinctive configuration-change sound heard in the Roles page. Reproduce
        // that same notification path for modifier quota changes instead of substituting a menu
        // or back-button clip.
        var key = MiraLocaleManager.GetOrCreateLocaleString(binding.DisplayName);
        var textColor = binding.Color.ToTextColor();
        var amount = Mathf.RoundToInt(binding.Amount.Value);
        var chance = Mathf.RoundToInt(binding.Chance.Value);
        var item = TranslationController.Instance.GetString(
            StringNames.LobbyChangeSettingNotificationRole,
            string.Concat(
                "<font=\"Barlow-Black SDF\" material=\"Barlow-Black Outline\">",
                textColor,
                binding.DisplayName,
                "</color></font>"),
            "<font=\"Barlow-Black SDF\" material=\"Barlow-Black Outline\">" + amount + "</font>",
            "<font=\"Barlow-Black SDF\" material=\"Barlow-Black Outline\">" + chance + "%");

        HudManager.Instance.Notifier.SettingsChangeMessageLogic(key, item, true);
    }

    private static void CreateGearButton(
        GameSettingMenu menu,
        RolesSettingsMenu roleMenu,
        RoleOptionSetting row,
        ModifierBinding binding)
    {
        var newButton = Object.Instantiate(row.buttons[0], row.transform);
        newButton.name = "ConfigButton";
        newButton.transform.localPosition = new Vector3(0.4473f, -0.3f, -2f);

        var text = newButton.transform.FindChild("Text_TMP");
        if (text)
        {
            Object.Destroy(text.gameObject);
        }

        if (newButton.activeSprites is not null && newButton.activeSprites)
        {
            newButton.activeSprites.Destroy();
        }

        var btnRend = newButton.transform.FindChild("ButtonSprite").GetComponent<SpriteRenderer>();
        btnRend.gameObject.SetActive(true);
        btnRend.enabled = true;
        btnRend.sprite = MiraAssets.Cog.LoadAsset();

        var passiveButton = newButton.GetComponent<GameOptionButton>();
        passiveButton.OnClick = new Button.ButtonClickedEvent();
        passiveButton.interactableColor = btnRend.color = binding.Color.FindAlternateColor();
        passiveButton.interactableHoveredColor = Color.white;
        passiveButton.OnClick.AddListener((UnityAction)(() => OpenAdvanced(menu, roleMenu, binding)));
    }

    private static void OpenAdvanced(GameSettingMenu menu, RolesSettingsMenu roleMenu, ModifierBinding binding)
    {
        StopMiraRoleBuild(roleMenu);

        // Clear *all* existing advanced role/modifier option objects, not only the ones
        // created by the previous modifier. This is the important distinction: the container
        // may still contain Tracer/SUI/Condemner options created by Mira before Modifiers was
        // opened, and those must not survive underneath this modifier's settings.
        ClearAdvancedOptionBehaviours(roleMenu);

        var categoryHeader = roleMenu.AdvancedRolesSettings.transform
            .Find("CategoryHeaderMasked").GetComponent<CategoryHeaderMasked>();
        if (categoryHeader.Title.TryGetComponent<TextTranslatorTMP>(out var translator))
        {
            Object.Destroy(translator);
        }
        categoryHeader.Title.text = "Return to Modifier Settings";

        // CategoryHeaderMasked is shared with the normal role advanced page. Reuse its existing
        // click surface instead of destroying/recreating PassiveButton/BoxCollider2D components.
        // Destroying those live UI components made the header visually correct but non-clickable
        // for both Roles and Modifiers on some transitions. We only swap the OnClick delegate.
        ConfigureSharedAdvancedReturnButton(roleMenu, () => ReturnToList(roleMenu));

        roleMenu.roleTitleText.text = binding.DisplayName;
        roleMenu.roleHeaderSprite.color = binding.Color;
        roleMenu.roleHeaderText.color = binding.Color.FindAlternateColor();
        roleMenu.roleDescriptionText.text = $"Configure {binding.DisplayName}.";

        var imgBg = roleMenu.AdvancedRolesSettings.transform.FindChild("Imagebackground");
        if (imgBg)
        {
            imgBg.gameObject.SetActive(false);
        }
        roleMenu.roleScreenshot.gameObject.SetActive(false);

        var labelBg = roleMenu.AdvancedRolesSettings.transform.FindChild("InfoLabelBackground");
        if (labelBg)
        {
            labelBg.localPosition = new Vector3(-0.7f, 0.1054f, -2.5f);
        }
        roleMenu.roleDescriptionText.transform.parent.localPosition = new Vector3(1.5f, -0.2731f, -1f);
        roleMenu.roleDescriptionText.transform.parent.localScale = new Vector3(0.09f, 0.2f, 0.5687f);

        var y = -1f;
        foreach (var option in binding.AdvancedOptions.Where(x => x.Visible()))
        {
            var newOpt = option.CreateOption(
                roleMenu.checkboxOrigin,
                roleMenu.numberOptionOrigin,
                roleMenu.stringOptionOrigin,
                menu.GameSettingsTab.playerOptionOrigin,
                roleMenu.AdvancedRolesSettings.transform);

            newOpt.SetClickMask(roleMenu.ButtonClickMask);
            foreach (var renderer in newOpt.GetComponentsInChildren<SpriteRenderer>(true))
            {
                renderer.material.SetInt(PlayerMaterial.MaskLayer, 20);
            }
            foreach (var textMesh in newOpt.GetComponentsInChildren<TextMeshPro>(true))
            {
                textMesh.fontMaterial.SetFloat(ShaderID.StencilComp, 3f);
                textMesh.fontMaterial.SetFloat(ShaderID.Stencil, 20);
            }

            newOpt.LabelBackground.enabled = false;
            newOpt.transform.localPosition = new Vector3(1.1f, y, -2f);
            newOpt.Initialize();
            newOpt.gameObject.SetActive(true);
            if (AmongUsClient.Instance && !AmongUsClient.Instance.AmHost)
            {
                newOpt.SetAsPlayer();
            }

            AdvancedBehaviours.Add(newOpt);
            roleMenu.advancedSettingChildren.Add(newOpt);
            y -= 0.45f;
        }

        roleMenu.RoleChancesSettings.SetActive(false);
        roleMenu.AdvancedRolesSettings.SetActive(true);
        roleMenu.scrollBar.Inner = roleMenu.AdvancedRolesSettings.transform;
        roleMenu.scrollBar.SetYBoundsMax(Mathf.Max(0f, -y - 3f));
        roleMenu.scrollBar.ScrollToTop();
    }

    private static void ConfigureSharedAdvancedReturnButton(RolesSettingsMenu roleMenu, Action action)
    {
        var headerTransform = roleMenu.AdvancedRolesSettings.transform.Find("CategoryHeaderMasked");
        if (!headerTransform)
        {
            return;
        }

        // Mira also uses this same header as a clickable return surface. Preserve the actual
        // components so Unity's UI/collider bookkeeping stays intact; only replace the listener
        // for the page that is currently being shown.
        var collider = headerTransform.GetComponent<BoxCollider2D>();
        if (!collider)
        {
            collider = headerTransform.gameObject.AddComponent<BoxCollider2D>();
        }
        collider.size = new Vector2(7f, 0.7f);
        collider.offset = new Vector2(1.5f, -0.3f);
        collider.enabled = true;

        var button = headerTransform.GetComponent<PassiveButton>();
        if (!button)
        {
            button = headerTransform.gameObject.AddComponent<PassiveButton>();
        }

        button.ClickSound = roleMenu.BackButton.GetComponent<PassiveButton>().ClickSound;
        button.OnMouseOver = new UnityEvent();
        button.OnMouseOut = new UnityEvent();
        button.OnClick = new Button.ButtonClickedEvent();
        button.OnClick.AddListener((UnityAction)(() => action()));
        button.SetButtonEnableState(true);
    }

    private static void ConfigureRoleReturnButton(RolesSettingsMenu roleMenu)
    {
        ConfigureSharedAdvancedReturnButton(roleMenu, () => ReturnToRolesList(roleMenu));
    }

    private static void ReturnToRolesList(RolesSettingsMenu roleMenu)
    {
        var originalRolesContainer = GetOriginalRolesContainer();
        if (originalRolesContainer is not null && originalRolesContainer)
        {
            roleMenu.RoleChancesSettings = originalRolesContainer;
            roleMenu.scrollBar.Inner = originalRolesContainer.transform;
            originalRolesContainer.SetActive(true);
        }

        roleMenu.AdvancedRolesSettings.SetActive(false);

        // Let Mira rebuild/restore the real role quota list through its normal patched path.
        // This mirrors its own return coroutine without relying on the one-time listener that
        // Mira only installs when no PassiveButton already exists on the shared header.
        roleMenu.OpenChancesTab();
        roleMenu.scrollBar.ScrollToTop();
    }

    private static void ReturnToList(RolesSettingsMenu roleMenu)
    {
        roleMenu.AdvancedRolesSettings.SetActive(false);
        var modifierContainer = _modifierContainer;
        if (modifierContainer is not null && modifierContainer)
        {
            roleMenu.RoleChancesSettings = modifierContainer;
            modifierContainer.SetActive(true);
            roleMenu.scrollBar.Inner = modifierContainer.transform;
            roleMenu.scrollBar.SetYBoundsMax(Mathf.Max(0f, -_listBottomY - 2f));
            roleMenu.scrollBar.ScrollToTop();
        }
    }

    private sealed class ModifierBinding
    {
        public ModifierBinding(
            string key,
            string displayName,
            string group,
            Color color,
            LoadableAsset<Sprite> icon,
            AbstractOptionGroup optionGroup,
            ModdedNumberOption amount,
            ModdedNumberOption chance)
        {
            Key = key;
            DisplayName = displayName;
            Group = group;
            Color = color;
            Icon = icon;
            OptionGroup = optionGroup;
            Amount = amount;
            Chance = chance;
            AdvancedOptions = optionGroup.Children
                .Where(x => !ReferenceEquals(x, amount) && !ReferenceEquals(x, chance))
                .ToList();
        }

        public string Key { get; }
        public string DisplayName { get; }
        public string Group { get; }
        public Color Color { get; }
        public LoadableAsset<Sprite> Icon { get; }
        public AbstractOptionGroup OptionGroup { get; }
        public ModdedNumberOption Amount { get; }
        public ModdedNumberOption Chance { get; }
        public List<IModdedOption> AdvancedOptions { get; }
    }
}

/// <summary>
/// Prevent Mira from starting/restarting its normal Roles quota builder while TownOfNDev's
/// Modifiers page is borrowing RolesSettingsMenu as its host.
/// </summary>
[HarmonyPatch(typeof(RolesSettingsMenu), nameof(RolesSettingsMenu.OpenChancesTab))]
public static class TownOfNDevModifierRoleBuildGuardPatch
{
    [HarmonyPrefix]
    [HarmonyPriority(Priority.First)]
    public static bool Prefix()
    {
        return !ModifierSettingsRoleStylePatch.IsTownOfNDevModifierMenu();
    }
}

/// <summary>
/// Tear down the borrowed RolesSettingsMenu before Mira changes the selected mod page.
/// This prevents even a one-frame overlap and guarantees UpdateUi starts from a clean host.
/// </summary>
[HarmonyPatch]
public static class TownOfNDevModifierModPageGuardPatch
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(MenuState), nameof(MenuState.NextMod))]
    public static void NextModPrefix() => HideIfVisible();

    [HarmonyPrefix]
    [HarmonyPatch(typeof(MenuState), nameof(MenuState.PreviousMod))]
    public static void PreviousModPrefix() => HideIfVisible();

    private static void HideIfVisible()
    {
        if (!ModifierSettingsRoleStylePatch.IsVisible)
        {
            return;
        }

        var menu = GameSettingMenu.Instance;
        if (menu is not null && menu)
        {
            ModifierSettingsRoleStylePatch.Hide(menu);
        }
    }
}

/// <summary>
/// Mira normally opens Modifiers inside GameOptionsMenu. For TownOfNDev only, replace that
/// final presentation step with the real RolesSettingsMenu after Mira has completed its tab switch.
/// Also tear the borrowed modifier container down whenever the user leaves the Modifiers tab.
/// </summary>
[HarmonyPatch(typeof(MenuState), "ChangeTabPatch")]
public static class TownOfNDevModifierTabBridgePatch
{
    [HarmonyPrefix]
    [HarmonyPriority(Priority.First)]
    public static void Prefix(GameSettingMenu menu, int tabNum, bool previewOnly)
    {
        if (previewOnly || !ModifierSettingsRoleStylePatch.IsVisible)
        {
            return;
        }

        // Restore the real Roles container BEFORE Mira processes a tab switch away from
        // Modifiers. Without this, Mira can begin OpenMenu/OpenChancesTab while
        // RolesSettingsMenu still points at the modifier container, corrupting the shared host
        // state before our old postfix cleanup ever runs.
        if ((MenuCategory)tabNum != MenuCategory.Modifiers)
        {
            ModifierSettingsRoleStylePatch.Hide(menu);
        }
    }

    [HarmonyPostfix]
    [HarmonyPriority(Priority.Last)]
    public static void Postfix(GameSettingMenu menu, int tabNum, bool previewOnly)
    {
        if (previewOnly)
        {
            return;
        }

        if ((MenuCategory)tabNum == MenuCategory.Modifiers && ModifierSettingsRoleStylePatch.IsTownOfNDevModifierMenu())
        {
            ModifierSettingsRoleStylePatch.Show(menu, false);
            return;
        }

        // This must run even when CurrentModIdx has already changed away from TownOfNDev.
        // Previous versions returned early for another mod, leaving our borrowed role-menu
        // container active and rendering TownOfNDev modifiers on TOU Mira's page.
        ModifierSettingsRoleStylePatch.Hide(menu);
    }
}
