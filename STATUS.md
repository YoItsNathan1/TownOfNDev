## Beta.1 current milestone

- First TownOfNDev beta candidate.
- Source baseline is identical in behaviour to `0.1.0-alpha.12.0.4`.
- Version promoted to `0.1.0-beta.1`.
- No gameplay changes are included in the beta promotion commit/package.
- Next stage is beta regression testing, packaging, repository publishing and release preparation.

## Alpha.12.0.4 current change

- Asset-size optimisation pass complete.
- Embedded PNG resources reduced from 15.20 MiB to 3.49 MiB (77.0% smaller).
- Large artwork is capped at 512px and matching PPU values preserve existing in-game dimensions.
- No gameplay changes from alpha.12.0.3.

## Alpha.12.0.3 current change

- Tank settings-row icon now targets Mira's actual `Configuration.Icon` SpriteRenderer path via Tank-only sprite PPU.
- Reverser Alert action graphic is slightly smaller.
- Tank intro description remains shortened to avoid clipping.
- Gameplay logic is unchanged from alpha.12.0.2.

## Alpha.12.0.2 current change

- Compile-corrected Reverser namespaces after the alpha.12.0.1 external build.
- `TownOfUsColors` now resolves through `TownOfUs`.
- `MiraNumberSuffixes` and `MeetingCheck` now resolve through `MiraAPI.Utilities`.
- No gameplay changes.

# TownOfNDev v0.1.0-beta.1

## Alpha.12.0.1 current change

- Compile-only Reverser correction: restored the missing `MiraAPI.Hud` namespace for `ButtonUsesMode`.
- No gameplay changes.

## Alpha.12.0 current change

- Reverser implemented as the final planned pre-beta role: **Impostor Power** with a timed Alert/Reverse stance.
- Reverser settings: Number of Reverses 10 (1-15), Reverse Cooldown 30s (2.5-120s), Reverse Duration 15s (2.5-120s).
- Direct, defense-respecting incoming murders are cancelled during Alert and reflected onto the attacker through MiraAPI's normal custom-murder pipeline.
- The active Alert is synchronized with a timed modifier and is removed on death/meeting.
- Reverser's reflected kill deliberately leaves the normal Impostor kill timer unchanged.
- Tank visual follow-up included: smaller TMP role-list icon and shorter intro description.
- Tank gameplay remains validated and unchanged.
- Next milestone after Reverser validation: full regression/testing pass and first beta version planning.

## Alpha.11.0.1 current change

- Compile-corrected TankSystem after the first alpha.11.0 external build.
- Restored the `TownOfUs.Utilities` extension namespace used by `HasDied()` and `LobbyNotificationMessage.AdjustNotification()`.
- Cleaned the Tank task-completion nullability path; no gameplay changes.

## Alpha.11.0 current change — Tank

- Tank source implementation added and awaiting Windows compile/in-game validation.
- Protection activation is derived from the Tank's actual assigned task list being non-empty and fully complete.
- Direct, defense-respecting out-of-meeting murders are blocked after activation; indirect/defense-bypassing/meeting deaths are intentionally not blocked.
- Optional attacker-identification notification and private Tank Protection HUD status are included.
- Reverser remains pending until Tank passes validation.

# TownOfNDev v0.1.0-alpha.11.0.1 — Tank

## Alpha.10.5.17 current change

- Repairs shared RolesSettingsMenu state after direct-first Modifiers navigation.
- Role advanced options are no longer left inactive after visiting Modifiers.
- Role return headers no longer retain TownOfNDev's modifier-return listener.
- Restores the real Roles container before Mira handles a Modifiers -> Roles transition.


# TownOfNDev v0.1.0-alpha.10.5.17 — Modifier expansion

## alpha.10.5 modifier first-load isolation + config independence + Menace cooldown fix


- Fixed the first-open modifier race where RolesSettingsMenu briefly showed the stock `All` tab and then populated the TownOfNDev role list before a Roles -> Modifiers round-trip corrected it.
- The borrowed RolesSettingsMenu behaviour is now disabled while the modifier page is active, and the modifier container is fully prepared before the host GameObject is shown.
- First-time modifier rendering no longer depends on opening Roles first; a hidden real TownOfNDev role is used only to initialise the stock role-row chrome when no built role row exists yet.
- Modifier assignment chance and advanced settings no longer hide when Amount is `0`; modifiers can be configured before they are enabled. Existing genuine dependency visibility (for example SUI's protected-player limit or Condemner's per-round limit) is preserved.
- Fixed the TownOfNDev modifier RolesSettingsMenu container surviving when switching to another mod page.
- The borrowed modifier container is now torn down before Next/Previous mod navigation and on every non-TownOfNDev-modifier tab switch.
- Restores the destination mod's normal Roles container when appropriate and otherwise disables the borrowed RolesSettingsMenu host.
- Menace now rewrites normal full kill-cooldown resets through `PlayerControl.SetKillTimer`, so a 20s cooldown with 25% Menace correctly resets to 15s instead of returning to 20s.
- Menace's HUD/modifier description now displays the live configured percentage (for example, `25%`) rather than saying only “configured percentage”.

## alpha.10.5.8 modifier row chrome completion

- Modifier rows now clone the visual state of a hidden, fully initialised real role row and immediately strip all role ownership/state from the clone.
- Restores the exact Roles-page white `- / +` button boxes, bordered value boxes and centre divider without reintroducing the dummy-Crewmate bug.
- Advanced-setting cog buttons now inherit the fully initialised role-button visuals and are explicitly enabled.
- Preserves modifier-coloured left panels and the isolated modifier-only RolesSettingsMenu container from alpha.10.5.6/10.5.7.


- **alpha.10.5.1 UI:** TownOfNDev modifier configuration now uses role-style amount/chance rows with faction headers and per-modifier advanced-setting cogs.

## Alpha.10.5 modifier implementation

**Implementation status:** Source-complete; requires Windows build and in-game validation.

- Menace: Impostor-only percentage kill-cooldown reduction (15–50%, default 25%).
- Soulhandler: private dead-player faction labels in meetings (Crewmate / Impostor / Neutral).
- Miracle: host-authoritative independent survival roll (5–100%, default 50%) with protected-kill feedback; meetings/self-kills excluded.
- Null: vote is submitted/displayed normally but filtered from the numerical vote tally.
- Current dependency target remains Town of Us: Mira 1.7.3 / Mira API 0.5.0.
- Approved modifier artwork is embedded for Menace, Soulhandler, Miracle and Null.


## Alpha.10.4 current change

- New clear public Death Row skull artwork.
- New Condemner-only Death Note tracker for all currently condemned players.
- Tracker appears beside marked player names in-world and in the local Condemner's meeting UI.
- Public Death Row skulls and private tracker markers are destroyed immediately when the marked player dies or disconnects.
- Dead players are removed from the meeting-local Death Row snapshot so skulls cannot linger into the ejection transition.
- No changes to Condemner execution rules, Death Note validation, Troll win logic, or SUI logic.
- Version advanced to `0.1.0-alpha.10.4.4.2.1`; expected DLL is `TownOfNDev-0.1.0-alpha.10.4.4.2.1.dll`.


## Alpha.10.3.3 current change

- Fixes the versioned assembly-name property evaluation order.
- Alpha.10.3.2 evaluated `AssemblyName` before `VersionPrefix` / `VersionSuffix`, producing `TownOfNDev--.dll`.
- Expected DLL for this source: `TownOfNDev-0.1.0-alpha.10.3.3.dll`.
- No gameplay changes from alpha.10.3.1/alpha.10.3.2.


## Alpha.10.1 current change

- Compact SUI HUD/task role information into six short manual lines to reduce the role panel width.
- Scale only the Protect button artwork to `1.12x`; the native keybind and cooldown UI are untouched.
- No SUI gameplay/networking mechanics changed.
- Version advanced to `0.1.0-alpha.10.1`.

## Alpha.10.1 validation status

- Alpha.10.0 compiled successfully on Windows with zero compiler errors before this UI-only patch.
- Static validation for alpha.10.1 covers version metadata, locale XML, resource integrity, C# delimiter balance, and package/source equality.
- Run the normal Windows compiler pass before runtime UI verification.


## Alpha.10.0 current change

- Added **SUI** as a maximum-1 **Crewmate Protective** role with the approved role icon and Protect button artwork.
- Added host-authoritative **Protect** targeting with a 25s default cooldown (20–45s range), self-target rejection, nearest-target/range revalidation and Butterfingers compatibility.
- Added configurable simultaneous protection capacity: limit On/Off, default maximum 3, configurable 1–5. Unlimited mode is cooldown-limited only.
- Added synchronized `SUI Protected` status modifiers. The protected player sees the status; SUI cannot remove/reassign it manually.
- Added direct/indirect outside-meeting murder interception, including kill paths that request defense bypass. Meeting guesses/executions and ejections are intentionally not intercepted.
- Added first-trigger state per protected player. The first attacker whose murder SUI actually blocks is synchronized as SUI's private reveal target and receives a red world outline only on the owning SUI client.
- Added post-trigger suppression for the normal Among Us Kill button so an activated protected player no longer lights up as its target. TOU/Mira custom killing abilities remain protected at the interaction/murder layer and receive the normal temporary save cooldown when blocked.
- Added SUI-death/disconnect cleanup: all owned protections, activated-protection state and attacker reveals are removed. Protected-target and revealed-attacker death/disconnect cleanup is also host maintained.
- Added compatibility ordering so existing TOU protective handlers (Medic/Mirrorcaster/Warden/Cleric-style) resolve before SUI; SUI ignores an interaction already cancelled by another protection.
- Added hidden internal SUI trigger/reveal modifiers so implementation bookkeeping does not appear in the role/modifier wiki.
- Version advanced to `0.1.0-alpha.10.0`.


## Alpha.10.0 validation status

- Final source/static audit completed in the generation environment: C# delimiter balance, locale XML parsing, embedded-resource name resolution, SUI artwork byte-for-byte verification, version metadata, RPC id uniqueness, and clean-package checks all pass.
- The generation environment does not provide the .NET SDK, so the authoritative compiler pass must still be run on the Windows development machine with the commands below.
- No source package should be treated as gameplay-verified until that compiler pass and the focused multiplayer checklist in `TESTING.md` are complete.

```powershell
dotnet restore .\TownOfNDev.sln
dotnet build .\TownOfNDev.sln -c Release --no-restore
```

## Alpha.9.4 current change

- Troll now distinguishes direct and indirect successful murders using MiraAPI's `AfterMurderEvent.IsIndirectAttack` flag.
- New Troll option: **Indirect Kills Count as Win** (default Off).
- Direct murders are always eligible; indirect murders are eligible only when the option is enabled.
- Troll remains unguessable because it does not implement TOU's `IGuessable` role contract.
- Existing Continues Game / Ends Game handling and additional-winner bookkeeping are unchanged.


- Reworked all current TownOfNDev role and modifier wiki descriptions into concise, player-facing gameplay explanations.
- Troll no longer describes itself as the opposite of Jester and no longer explains host choices in the description body.
- Fungi, Tracer, Condemner, Eye Mask, Butterfingers, and Laggy descriptions no longer expose implementation/configuration details that belong in the options list.
- The internal Fungi Faction Marker is now explicitly hidden from the wiki list instead of receiving a generated soft entry.
- No gameplay mechanics, role options, or approved artwork changed in this update.

# TownOfNDev v0.1.0-alpha.9.2.1 — Troll configuration update
- alpha.9.2.1 compiler fix: imports `TownOfUs.Utilities` in `TrollEvents.cs` so the TOU Mira `AdjustNotification()` extension resolves correctly.

## Current status

- alpha.9.1 compiled successfully on Windows.
- alpha.9.2 expands Troll configuration before the multiplayer gameplay pass.
- Troll now defaults to **Continues Game** rather than forcing an immediate game end.
- Successful Trolls are retained as additional winners for the eventual normal game ending.
- Hosts can optionally select **Ends Game** for the original immediate solo Troll ending.
- Added **Can Use Button**, **Has Impostor Vision**, and conditional **Announce Troll Win** options.
- Troll deliberately does **not** receive Jester vent/scatter mechanics.
- No Troll icon/art changes in this update.

# TownOfNDev v0.1.0-alpha.9.1 — Troll compiler repair


## Alpha.9.1 compiler repair

- Windows compiler pass found one error in `TrollGameOver.cs`: the `ToHtmlStringRGBA()` extension was unavailable because its extension namespace was not imported.
- Added `using Reactor.Utilities.Extensions;`, matching the already-working `FungiGameOver.cs` implementation.
- No gameplay logic or assets changed.
- Version advanced to `0.1.0-alpha.9.1`.

## Alpha.9 implemented

- Added **Troll**, a **Neutral Evil** role and direct inverse of Jester.
- Troll wins alone only when another player successfully murders them.
- Ejection, disconnects and self-kills do not satisfy the Troll win condition.
- Successful meeting murders/guesses by another player are eligible because they remain player-caused kills.
- Added `TrollSystem` to latch the first valid Troll murder consistently across clients.
- Added a priority-3 `TrollWinCondition`, ahead of Fungi (4), TOU generic neutrals (5), Lovers and normal faction endings.
- Added a dedicated `TrollGameOver` result with the Troll as the sole winner.
- Troll has no kill, sabotage or vent access and cannot complete ordinary Crewmate task consoles.
- Embedded the approved Troll icon unchanged as `Troll_Role_Icon.png`.
- Version advanced to `0.1.0-alpha.9`.

## Historical alpha.8 — Condemner

- Added **Condemner**, an **Impostor Killing** role with a hard maximum of 1.
- Added host-authoritative **Death Note** targeting for living non-Impostors in normal ability range.
- Death Note starts at 45s by default (30–60s option range) and resets the normal Kill cooldown only after host-confirmed success.
- Added configurable per-round Death Note cap: limit toggle plus 1–5 maximum; disabling the toggle gives unlimited uses.
- Added synchronized hidden `CondemnedModifier` state through MiraAPI `RpcAddModifier`.
- Added Warden Fortify host revalidation; a fortified target cannot be condemned and no Death Note cooldown/use is consumed.
- Death Note remains a normal successful non-killing interaction so existing Fungi and Tracer interaction observers can react after the ability commits.
- Added public red Death Row skull markers to meeting vote areas. Markers are intentionally not removed when the Condemner dies during the meeting.
- Added configurable **Death Row Persists After Condemner Dies**, default Off.
- Added host-only `ProcessVotesEvent` resolution after normal meeting mechanics, including Condemner-voted-out cancellation, guessed/dead prisoner filtering and ejection double-kill prevention.
- Valid Death Row executions are dispatched during the same ProcessVotes host frame using TOU meeting nameplate murder animations.
- Embedded the approved Condemner role and Death Note button artwork plus a compact red skull meeting marker.
- Cleaned the two alpha.7 nullable-analysis warnings in `TracerTraceRenderer` by explicitly narrowing Unity sprite-renderer references before `SetOutline`.
- Version advanced to `0.1.0-alpha.8`.

## Historical compiler status

The generation environment does not contain `dotnet`; the Windows compiler commands used for these alpha builds are:

```powershell
dotnet restore .\TownOfNDev.sln
dotnet build .\TownOfNDev.sln -c Release --no-restore
```

## Historical alpha.7 / alpha.6 notes

## Compiler follow-up

- `dotnet restore` for alpha.6 completed successfully on the development PC.
- The first alpha.6 compile exposed one error only: `PlayerReviveEvent` was unresolved in `FungiEvents.cs`.
- TOU Mira 1.7.2 defines `PlayerReviveEvent` in `TownOfUs.Events.TouEvents`; alpha.6.4 imports that namespace explicitly.
- A fresh build is required to confirm compiler success.

## Finished in this pass

- Updated the project dependency line to TOU Mira **1.7.2**, MiraAPI **0.5.0** and Among Us GameLibs **2026.8.18**.
- Fixed TOU 1.7.2 modifier API migration: `LocaleKey` → `IdPart` for Eye Mask, Butterfingers and Laggy.
- Replaced obsolete TOU locale internals with MiraAPI 0.5.0 embedded-locale registration.
- Registered Fungi's custom win condition through TOU 1.7.2's public `WinConditionRegistry`.
- Added the Neutral Evil task header to Fungi and matched TOU's task-header cleanup on deinitialization.
- Preserved TOU 1.7.2 modifier bookkeeping in Laggy's activation/deactivation overrides.
- Excluded disconnected Fungi from the explicit custom game-over winner payload.
- Confirmed all project/config/locale XML files parse successfully.
- Confirmed all eight embedded PNG assets decode successfully.
- Confirmed coarse C# brace balance across the project.
- Rewrote the README for alpha.6 and added `TESTING.md`.

## Fungi alpha.6.7 implementation state

Implemented:

- Neutral Evil Fungi role, maximum role count 3.
- Shared multi-Fungi infection objective.
- Host-authoritative Infect button with target/range revalidation.
- Native cooldown rendering and approved Infect artwork.
- Butterfingers integration for Infect.
- Stage 0 hidden infection.
- Post-meeting Strand 1/2/3 visual growth.
- Infection removal on death and configurable stage restoration on revival.
- Fungi-side infected/faction visibility.
- Infection propagation through successful player-targeted Mira/TOU custom abilities.
- Configurable living-player infection threshold.
- Independent faction win and additional-winner modes.
- Dedicated Fungi custom game over for the independent mode.

## Remaining gate

**Compiler/runtime verification only.**

The current execution environment has no `dotnet` executable, so alpha.6.4 cannot be truthfully marked compiler-verified here. On the development PC run:

```powershell
dotnet restore .\TownOfNDev.sln
dotnet build .\TownOfNDev.sln -c Release --no-restore
```

If that produces `0 Error(s)`, install the DLL and run the checklist in `TESTING.md`.


## alpha.6.4 compiler-fix pass

- Fixed the four `CS0121` byte `Math.Clamp` ambiguities by selecting the byte overload explicitly.
- Added the missing `MiraAPI.Utilities` imports required for `ShaderID` and `TmpSpriteUtils` on the TOU Mira 1.7.3 / MiraAPI 0.5.0 stack.
- Hardened the Fungi click/spread/growth code for nullable analysis around Unity/IL2CPP object truthiness.
- This package is ready for the next Windows `dotnet build` compiler pass.

## alpha.6.4 compiler-fix pass

- Fixed the remaining `CS0103` for `TmpSpriteUtils` by importing its actual MiraAPI 0.5.0 namespace: `MiraAPI.Utilities.Assets`.
- TOU Mira 1.7.2 uses `TmpSpriteUtils` from that namespace for role/option TMP sprite assets.
- Previous alpha.6.2 build result was otherwise clean: `0 Warning(s)`, `1 Error(s)`.

## alpha.6.4 UI/runtime polish

- Added the missing `TownOfUsMira.Role.Fungi` localization family used by TOU/Mira role-setting notifications.
- Increased the role icon resource PPU from 200 to 1000 so the 1254px approved icon fits the Neutral Evil role settings row instead of overflowing it. TMP notification art remains sourced from the same approved icon.
- Reduced fungal growth scales and moved the three anchors outward around the character silhouette.
- Cleaned white reference-crewmate fragments from Strand 2 and Strand 3 assets.
- Unified growth sorting to `body.sortingOrder + 1` so later stages no longer stack increasingly far above cosmetics.
- Intentionally did not replace an infected player's actual role/faction label: an infected Impostor remains an Impostor, an infected Crewmate remains a Crewmate, etc.
- Requires one fresh Windows compiler pass, then the visual regression checks in `TESTING.md`.

## alpha.6.5 HUD follow-up

- Added a resilient `FungiInfectButton.Enabled(...)` check that accepts the actual Fungi role or the shared Fungi faction registry.
- Added a HUD self-heal path that recreates/re-shows the registered Infect button if a HUD/role refresh leaves it hidden after Fungi assignment.
- The self-heal does not bypass cooldown or target validity; out-of-range remains grey/disabled and meetings/death still hide the button.
- Replaced the single very long Fungi role-tab sentence with a compact three-line HUD description.
- Requires a fresh Windows compiler pass and then one focused Fungi HUD test.


## alpha.6.7 HUD/growth follow-up

- Changed the Infect icon resource from 200 PPU to 650 PPU so the approved 1254px artwork is rendered near the normal TOU ability-button footprint rather than filling the lower-right HUD.
- Expanded the Fungi task-panel description from three long lines to five shorter lines.
- Rebuilt fungal visual anchoring around `currentBodySprite.BodySprite.transform`; strands are children of the actual rendered body and are reparented if the live body renderer changes.
- Anchor positions are derived from the active body sprite bounds and mirrored with facing direction.
- Increased the visible mushroom sizes from alpha.6.4/6.5 and assigned three separate body-relative positions/rotations.
- Reused the clean Strand 1 mushroom art for all three visible colonies to avoid the white reference-crewmate remnants still present in the older Strand 2/3 source images.
- Requires a fresh Windows compiler pass followed by the alpha.6.7 focused visual test.

## alpha.6.7 task/HUD follow-up
- Fungi cannot complete ordinary Crewmate task consoles; behaviour mirrors TOU Neutral Evil roles such as Jester/Doomsayer.
- Infect artwork PPU increased from 650 to 750 for a modest HUD-size reduction.
- Fungi growth attachment remains body-relative; nullable body-renderer guard was made compiler-safe.


## alpha.6.7 interaction-transmission hotfix
- Fixed Fungi itself not being treated as a contagious source during successful player-to-player ability interactions.
- Host-side spread validation now uses `IsContagious = IsFungi || IsInfected`.
- Removed the client-side infected-vs-clean prefilter so host state is authoritative and recently synced infection state cannot suppress a valid spread request.
- Version intentionally remains `0.1.0-alpha.6.7`.


## alpha.6.7 infection synchronization hotfix (same version)
- Diagnosed the host/client split where successful interaction spread existed only in the host's local `FungiInfectedModifier` state.
- Infection application is now performed with MiraAPI's canonical networked modifier API: `RpcAddModifier<FungiInfectedModifier>`.
- Both direct Infect and interaction spread use the same host-authoritative synchronized path.
- Fungi clients should now immediately agree on `IsInfected`, stop selecting an infected player, tint that player's name green for Fungi, and grow the same strand stage after meetings.
- The old TownOfNDev `FungiApplyInfection` RPC id is left reserved for protocol stability but is no longer used.
- Version intentionally remains `0.1.0-alpha.6.7`.


## alpha.7 — Tracer

- Added **Tracer**, a Crewmate Investigative role with a maximum count of 2.
- Added host-authoritative **Dust** targeting with one use per round and configurable 15–45 second duration.
- Dust state synchronizes through MiraAPI networked modifiers and is completely hidden from the Dusted player.
- Added one-hop Trace evidence for confirmed successful `CustomActionButton<PlayerControl>` interactions.
- Added successful-murder evidence: killing a Dusted victim leaves Trace on the living killer.
- Trace ownership is private per Tracer and does not propagate from Trace carriers.
- Added body-attached powder handprint visuals, proximity/line-of-sight cyan outline, and private meeting handprint markers.
- Dust is removed at meeting start; Trace is kept through the meeting and removed at the next round start.
- Death/disconnect/role-loss cleanup removes invalid evidence without restoring it after revival.
- Dust itself is excluded from Tracer self-tracing but remains eligible for Fungi's direct-interaction infection system.
- Embedded the approved Tracer role and Dust button artwork.
- Version advanced to `0.1.0-alpha.7`.


## 0.1.0-alpha.10.4.4.2.1 corrections
- Normal Condemner kills now use the normal `Killed` cause instead of `Executed on Death Row`.
- Actual Death Row executions use a dedicated `CondemnerDeathRow` cause.
- Death Row executions no longer increment the Assassin/meeting `Guesses` statistic.
- Death Row executions are removed from the ordinary `Kills` tally, while genuine normal kills and genuine guesses remain counted normally.


## alpha.10.5.2 UI correction
- Corrected the modifier role-style header construction to mirror Mira custom-role headers.
- Normalized modifier icon rendering to role-icon size.
- Corrected row/header spacing and resets the modifier scroller to the top when the layout is first built.

## alpha.10.5.3 modifier layout coordinate correction

- Corrected the role-style modifier page to use spacing appropriate for the GameOptionsMenu container.
- Preserves the role-style headers, quota labels, rows, icons and cog controls without stacking category headers over the preceding modifier row.
- Starts the first modifier category at the normal Mira custom-options top anchor and uses expanded row/group spacing for the modifier container.

## alpha.10.5.4 modifier layout rebuild from Roles settings geometry

- Removed the hand-tuned modifier spacing introduced in alpha.10.5.2/10.5.3.
- Modifier category and row placement now derives from MiraAPI's actual Roles settings coordinates and converts those offsets into the Modifiers/GameOptions container transform.
- The modifier page now uses the same 0.522 / 0.422 / 0.43 / 0.4 layout rhythm as the Roles screen.
- Collapsed modifier categories now hide their `# OF ROLE / % CHANCE` quota strip, matching the Roles screen behaviour.
- The whole role-style modifier layout is anchored at the normal GameOptions left/top content position rather than relying on the Roles tab's incompatible parent transform.

## alpha.10.5.6 modifier settings host rewrite

- Removed the failed approach that tried to transplant RolesSettingsMenu geometry into GameOptionsMenu.
- TownOfNDev's Modifiers tab is now rendered directly inside the real RolesSettingsMenu used by the Roles tab.
- Modifier headers, quota headings, rows, masks, scrollbar, collapse behaviour and row controls now use the same templates and local coordinate system as custom roles.
- Modifier advanced settings open in the RolesSettingsMenu advanced-settings panel via the cog button.


## alpha.10.5.6 root-cause correction

- Fixed the actual alpha.10.5.5 overlay bug: Mira's original TownOfNDev Roles container was still active when the borrowed RolesSettingsMenu was used for Modifiers, so Tracer/SUI/Condemner and role headers rendered underneath the modifier UI.
- The original Roles container is now explicitly disabled for the Modifiers page and restored only when returning to Roles.
- Added a guard that prevents Mira's normal role quota builder from starting while TownOfNDev Modifiers is using RolesSettingsMenu.
- Modifier rows no longer bind a dummy Crewmate RoleBehaviour. RoleOptionSetting is now used strictly as a visual prefab, with TownOfNDev owning the amount/chance text and button handlers. This prevents a modifier row from being repainted as the vanilla `Crewmate` role.
- Modifier rows are no longer registered in Mira's `roleChances` list, preventing role-update code from treating them as real roles.
- Added explicit teardown when leaving Modifiers so the custom modifier container cannot remain visible on the Roles page.
- Tightened nullable guards in the modifier UI, Soulhandler meeting renderer, and Condemner tracker to remove the known CS8602 warning sites rather than ignoring them.


## alpha.10.5.9 advanced-settings visual fix
- Clears stale role advanced-setting OptionBehaviour objects before the Modifiers page/advanced modifier panel uses RolesSettingsMenu.
- Prevents SUI/other role settings from rendering underneath Menace, Butterfingers, Laggy, Eye Mask, Miracle, or other modifier settings.

### alpha.10.5.14 modifier quota parity
- Modifier chance rows now match Roles visually by showing plain numbers under `% CHANCE` (no duplicate percent suffix).
- Raising a modifier assignment chance above zero while Amount is zero automatically enables one copy, matching Roles quota behaviour.
- Modifier Amount/Chance controls now explicitly use the settings-menu click sound for audible configuration feedback.

## alpha.10.5.14 modifier quota audio parity

- Modifier Amount/Chance +/- controls now retain the same `GameOptionButton` click sound as the Roles quota controls.
- Removed the incorrect override that assigned the Roles menu Back button sound to modifier quota buttons.
- No modifier assignment/configuration behaviour changed in this patch.

## Alpha.10.5.17 shared advanced-header return fix

- Fixes both `Return to Role Settings` and `Return to Modifier Settings` becoming non-clickable after alpha.10.5.16.
- The shared `CategoryHeaderMasked` return surface is now reused instead of destroying/recreating its `PassiveButton` and `BoxCollider2D` components.
- TownOfNDev swaps only the active `OnClick` handler: modifier advanced pages return to the modifier list; role advanced pages return through Mira's normal `OpenChancesTab` path.
- Keeps the alpha.10.5.16 role-menu state fixes (fresh Modifiers-first opening and role advanced-option visibility).
