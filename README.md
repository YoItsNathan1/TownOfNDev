# TownOfNDev v0.1.0-beta.1

## Beta.1 milestone

This source is the first TownOfNDev beta candidate.

- Promoted directly from the validated `0.1.0-alpha.12.0.4` source baseline.
- No gameplay, role, modifier, UI, asset, or configuration behaviour has been changed for the beta promotion itself.
- Includes the completed pre-beta role roster, including Tank and Reverser.
- Includes the alpha.12.0.4 embedded-asset optimisation pass.
- Version identity is now `0.1.0-beta.1`.


## Alpha.12.0.4 asset-size optimisation

- Optimised all embedded TownOfNDev PNG resources.
- Large 1254px/1282px role, modifier, status and ability artwork is now capped at **512px on the longest edge** using high-quality Lanczos downsampling.
- Existing small tracker/strand artwork keeps its original pixel dimensions and is only losslessly PNG-optimised.
- Sprite pixels-per-unit values were scaled with each resized image, preserving the same in-game physical/rendered dimensions. This means the Tank settings-row size fix and Reverser Alert HUD sizing from alpha.12.0.3 are intentionally preserved.
- Alpha transparency is preserved.
- Embedded PNG payload reduced from **15.20 MiB** to **3.49 MiB** — a **77.0% reduction** before compilation.
- No gameplay code or role/modifier behaviour changed.


## Alpha.12.0.3 visual follow-up

- **Tank role settings card:** corrected the actual rendering path. Mira renders `Configuration.Icon` at a fixed row scale, so Tank's sprite PPU is now increased from `1000` to `1150` to reduce the card icon itself.
- Tank's TMP icon scale is restored to the normal TownOfNDev value because the earlier TMP-only change did not affect the settings card.
- Tank keeps the shortened intro text: `Finish your tasks to resist direct kills.`
- **Reverser Alert HUD icon:** reduced the button graphic scale from `1.06` to `0.92`; cooldown/uses/keybind UI are untouched.
- No Tank or Reverser gameplay logic changed.


## Alpha.12.0.2 Reverser compile correction

- Added the missing `TownOfUs` import required by `TownOfUsColors` in `ReverserAlertButton`.
- Added the missing `MiraAPI.Utilities` import required by `MiraNumberSuffixes` in `ReverserOptions`.
- Added the same utilities namespace to `ReverserEvents` for the `MeetingCheck` type used by reflected murders.
- No Reverser or Tank gameplay behaviour changed.


## Alpha.12.0.1 compile correction

- Added the missing `MiraAPI.Hud` import required for `ButtonUsesMode` in `ReverserAlertButton`.
- No Reverser gameplay or Tank visual behavior changed.

## Alpha.12.0 focus — Reverser + Tank visual cleanup

- Adds **Reverser**, a TownOfNDev **Impostor Power** role.
- Reverser keeps the normal Impostor Kill, vent and sabotage access and gains an **Alert** ability.
- Alert defaults to **10 uses per game**, a **30 second cooldown**, and a **15 second active duration**.
- While Alert is active, ordinary direct defense-respecting kill attempts against Reverser are cancelled and reflected onto the attacker.
- Alert remains active after a reflection, allowing multiple attackers to be reversed during one active window.
- Indirect attacks, defense-bypassing attacks, meeting deaths and ejections bypass Alert.
- Meetings immediately end Alert; Mira's normal button reset path then applies the configured cooldown without restoring per-game uses.
- Reflected kills do **not** reset Reverser's normal Impostor Kill cooldown.
- The approved `ReverserIcon.png` and `ReverserAlertIcon.png` are embedded as the role and Alert-button artwork.
- Tank gameplay is unchanged. Its role-summary icon is slightly smaller and its intro line is shortened to prevent the visual clipping seen during testing.

Expected Release DLL: `TownOfNDev-0.1.0-beta.1.dll`.

## Alpha.10.5 focus — modifier expansion

This update adds the next four confirmed TownOfNDev modifiers:

- **Menace** — Impostor-only passive modifier. Reduces the holder's resolved kill cooldown by a configurable percentage. Default reduction is **25%**, configurable from **15%–50%** in **5%** steps.
- **Soulhandler** — universal visibility modifier. During meetings, only the holder sees whether each dead player belonged to the **Crewmate**, **Impostor**, or **Neutral** faction; exact roles are not revealed.
- **Miracle** — universal passive modifier. Eligible incoming out-of-meeting murder attempts independently roll a configurable survival chance. Default is **50%**, configurable from **5%–100%** in **5%** steps. A successful roll produces protected-kill feedback and the Miracle survives.
- **Null** — universal passive modifier. The holder votes normally and the vote remains visually present, but it contributes **zero** to the actual exile/tie tally.

The modifiers are disabled by default through their Amount option and retain the normal Town of Us/Mira assignment-chance model. Menace is restricted to Impostor-aligned roles. Miracle excludes Troll and known survival/special-death role names (`TankRole`, `UnstoppableRole`, `TerroristRole`) to avoid conflicting death mechanics.

The approved modifier icons for **Menace**, **Soulhandler**, **Miracle**, and **Null** are embedded and used in modifier UI/notifications.

The TownOfNDev **Modifiers** settings page now mirrors the role-settings presentation: faction-style headers, one modifier row with **# OF ROLE** and **% CHANCE** controls, the approved modifier icon/name, and a cog for modifiers with additional configuration.

Expected Release DLL: `TownOfNDev-0.1.0-beta.1.dll`.



## Alpha.10.4 focus — Condemner visual tracking

- Replaces the old Death Row marker artwork with a clearer red cartoon skull.
- Adds a private Death Note tracker beside every player condemned by the local Condemner, allowing multiple marked players to be tracked collectively.
- The private tracker is visible beside marked players in normal gameplay and on the Condemner's meeting view.
- Public Death Row skulls are now shown only while the condemned player is alive and are destroyed immediately on death/disconnect instead of lingering into the ejection transition.
- Keeps the alpha.10.3/10.3.1 placement fixes and the alpha.10.3 Troll guessing rules.
- Expected Release DLL: `TownOfNDev-0.1.0-alpha.10.4.4.2.1.dll`.


## Alpha.10.3.3 packaging/build-name fix

- Fixes the MSBuild property-order bug that produced `TownOfNDev--.dll` and `TownOfNDev--.deps.json`.
- `AssemblyName` is now evaluated after `VersionPrefix` and `VersionSuffix` are defined.
- A successful Release build should output `TownOfNDev-0.1.0-alpha.10.3.3.dll`.
- Future version bumps inherit the same naming rule automatically.
- Includes the alpha.10.3 Troll guessing safeguards and the alpha.10.3.1 Condemner Death Row marker placement fix.


## Alpha.10.1 focus — SUI UI polish

- Shortened and manually wrapped the in-game SUI role information into six compact lines so the role panel no longer stretches across most of the screen.
- Increased only the Protect artwork scale by **12%** inside Mira's native ability button; cooldown/keybind overlays retain their normal scale and positioning.
- No SUI gameplay, protection, targeting, reveal, networking, settings or artwork files changed.
- Alpha.10.0 compiled successfully on the Windows development machine before this UI-only follow-up.


## Alpha.10.0 focus — SUI

**SUI** is a TownOfNDev **Crewmate Protective** role built around persistent multi-player protection.

- **Protect** targets the nearest valid living player. SUI cannot protect themselves and cannot manually remove or move an existing protection.
- Protect starts on a **25 second** cooldown, configurable from **20–45 seconds**.
- **Limit Protected Players** defaults On. When enabled, **Maximum Protected Players** defaults to **3** and is configurable from **1–5**. Turning the limit Off allows SUI to protect every eligible living player over time.
- A protected player receives a visible **SUI Protected** HUD modifier so they know the protection is active. Internal trigger/reveal bookkeeping is hidden from the role/modifier browser.
- While SUI is alive, protected players are immune to player-caused lethal murder interactions outside meetings, including direct, indirect and defense-bypassing murder paths.
- Meeting guesses/executions and voting/ejection are intentionally outside SUI protection.
- The first attack that SUI itself blocks for a protected player activates that protection and privately reveals the attacker to SUI with a persistent **red outline**.
- After activation, the normal Among Us Kill button stops selecting that protected player, preventing the repeated base-game Kill spam seen in the original role footage. TOU/Mira custom killing abilities are still cancelled by SUI protection and receive the normal temporary save cooldown when attempted. The underlying protection remains active against later lethal murder attempts.
- A revealed attacker stays marked until either the attacker dies/disconnects or the owning SUI dies/disconnects. Only SUI receives the red outline.
- If SUI dies or disconnects, every protection and reveal state owned by that SUI is removed immediately. Protected-player HUD modifiers disappear at the same time.
- Existing TOU protection handlers are allowed to resolve before SUI. If another shield/fortification already cancelled an attack, SUI does not falsely activate or reveal that attacker.
- The approved `SUIIcon.png` and `SUIProtectIcon.png` artwork are embedded unchanged as `SUI_Role_Icon.png` and `SUI_Protect_Button.png`.


## Alpha.9.4 focus — Troll direct/indirect kill rules

- Added **Indirect Kills Count as Win** to the Troll configuration. It defaults **Off**.
- Direct successful player-to-player murders still always count toward the Troll objective.
- When the new option is Off, MiraAPI murders flagged as indirect (delayed, AOE, chain/remote-style kills) do not secure the Troll win.
- When the new option is On, those successful indirect murders can secure the Troll win as long as another player is the murder source.
- Troll remains **unguessable**; enabling indirect kills does not make Troll available as a guessing target.
- Ejection, disconnects, self-kills and failed/protected murder attempts remain invalid win paths.

## Alpha.9.3 focus — role browser description cleanup

- Rewrote the in-game role/modifier browser descriptions for **Troll, Fungi, Tracer, Condemner, Eye Mask, Butterfingers, and Laggy** so they read as normal player-facing explanations rather than implementation notes or option summaries.
- Removed host/configuration wording from the description body because the relevant options are already displayed directly below it.
- Removed technical implementation language such as network-packet details, trigger-roll ranges, and host-authoritative wording.
- Hid the internal **Fungi Faction Marker** from the role/modifier browser while preserving its additional-winner bookkeeping.

## Alpha.9.2 focus — Troll configuration and continuing-win mode

- Added a dedicated Troll configuration panel with **Can Use Button**, **Has Impostor Vision**, **After Win Type**, and **Announce Troll Win**.
- **Continues Game** is the default After Win Type. A Troll killed by another player secures a personal win, the match continues, and that Troll is included as an additional winner when the eventual Crew/Impostor/Neutral ending occurs.
- **Ends Game** remains available for hosts who want the first successfully killed Troll to immediately end the match and win alone.
- Troll still has **no vent access, no scatter mechanic, no kill button, and no sabotage access**.
- In Continues Game mode, multiple Trolls can independently secure their own personal wins if each is successfully killed by another player.
- Announce Troll Win is only shown as an option when Continues Game is selected.

## Alpha.9 focus — Troll

**Troll** is a TownOfNDev **Neutral Evil** role centered on convincing another player to kill them.

- The Troll wins **alone** when another player successfully kills them.
- The win is armed only from a confirmed player-caused murder event.
- Being **voted out does not count**.
- Disconnecting does not count.
- Self-inflicted deaths, including a meeting misguess/self-kill, do not count.
- Direct successful murders count by default. Indirect murders are configurable from alpha.9.4 onward; Troll is not a valid guessing target.
- Troll has no vanilla kill button, sabotage access or vent access.
- Ordinary Crewmate task consoles are blocked, matching TOU Neutral Evil behaviour; any shown task list is cosmetic/fake.
- Up to **3 Trolls** can be configured. The first Troll successfully killed ends the game and wins alone.
- A dedicated priority-3 Troll win condition resolves before Fungi, TOU generic Neutral wins, Lovers and standard Crew/Impostor endings.
- A dedicated Troll game-over presentation uses the approved grey Troll icon/theme.
- Approved `TrollIcon.png` artwork is embedded unchanged as `Troll_Role_Icon.png`.

## Alpha.8 focus — Condemner

**Condemner** is a TownOfNDev **Impostor Killing** role (maximum **1** per game).

- **Death Note** secretly places the nearest valid non-Impostor on **Death Row**.
- Death Note starts on its full cooldown: **45 seconds** by default, configurable from **30–60 seconds**.
- Using Death Note successfully resets the Condemner's normal Kill button to its full cooldown. A normal Kill does not reset Death Note.
- Death Row is a successful **non-killing player interaction**: Fungi/Tracer contact systems can react to it, and **Warden Fortify blocks it completely**. Ordinary kill protection such as Medic does not stop the sentence.
- The target receives no notification or visible status during the round. Already-condemned players and Impostor-aligned teammates cannot be targeted.
- At the next meeting, every living Death Row target receives a public **red skull** beside their meeting card. The skull stays visible for the rest of that meeting even if the Condemner dies.
- Death Row does not stop voting, guessing, or other meeting abilities. A prisoner guessed/killed before resolution is not killed again. A prisoner selected for ejection is also not double-killed.
- Death Row resolves in `ProcessVotesEvent`, after votes are calculated and immediately before the results display. All valid executions are dispatched on the same host frame so the meeting nameplate deaths begin together.
- **Death Row Persists After Condemner Dies** defaults **Off**. With it Off, killing/guessing the Condemner or voting the Condemner out cancels every active sentence, but the meeting skulls deliberately remain so prisoners only learn they were saved when no execution occurs.
- The per-round use cap is configurable. **Limit Death Note Uses Per Round** defaults On, with **2** uses by default and a configurable **1–5** maximum. Turning the limit Off makes uses unlimited; the cooldown remains the limiter.
- Approved `CondemnerIcon.png` and `CondemnerAbilityIcon.png` artwork are embedded. A compact red skull marker is included for the public meeting indicator.

## Alpha.7 focus — Tracer

**Tracer** is the first TownOfNDev **Crewmate Investigative** role.

- **Dust** secretly marks the nearest valid living player using the normal role ability range.
- Tracer has exactly **1 Dust use per round**; the use resets after each meeting.
- Dust lasts **25 seconds** by default and is configurable from **15–45 seconds**.
- The Dusted player receives no notification and no visible status.
- A successful direct player-to-player interaction involving the Dusted player leaves a private **Trace** on the other participant.
- Trace is exactly one hop: a Traced player does **not** transmit evidence to anyone else.
- Successful murders are supported: killing a Dusted player leaves Trace on the living killer.
- Failed/cancelled interactions, proximity, voting, chat, tasks, vents, reports and passive/AOE effects do not create Trace.
- Only the owning Tracer sees Trace evidence. In normal gameplay this is a small powder handprint attached to the carrier's live body.
- When the owning Tracer is within normal ability range and line of sight, the Trace carrier also receives a soft blue/cyan outline.
- Dust ends when a meeting starts; existing Trace remains through the meeting.
- With **Show Trace In Meetings** enabled, the owning Tracer receives a private handprint marker beside living Trace carriers in the meeting UI.
- Trace clears when gameplay resumes after the meeting.
- Dead Trace carriers lose the mark. A dead/disconnected Tracer loses all Dust and Trace they own. Revival does not restore old evidence.
- Up to **2 Tracers** can be enabled. Their Dust/Trace ownership is independent.
- Dust itself is excluded from creating the Tracer's own Trace, but it is still a real successful player interaction for other systems such as Fungi infection spread.
- Approved `TracerIcon.png` and `TracerDustIcon.png` artwork are embedded, plus a compact thumb-plus-three-finger handprint evidence sprite generated for the in-world/meeting Trace indicator.

## Alpha.6 focus — Fungi

**Fungi** is the first custom TownOfNDev role and is a **Neutral Evil** role/faction.

Current alpha.6.7 implementation:

- Up to **3 Fungi** can exist in one match and share one infection objective.
- Fungi has no vanilla kill button, no sabotage access and no vent access.
- **Infect** targets the nearest valid clean player within the normal role ability range.
- Default Infect cooldown: **20 seconds**, configurable from **10–45 seconds**.
- Infect is host-authoritative. Invalid/range-lost requests do not consume the cooldown.
- The approved Fungi role icon and approved Infect button artwork are embedded.
- New infections begin at **Stage 0**, with no visible fungal strand.
- After each completed meeting, living infected players advance one stage: **Strand 1 → Strand 2 → Strand 3**.
- Dead infected players immediately lose their active infection. A configurable option can restore the exact previous infection stage if they are revived.
- Fungi players can identify infected players by the Fungi name tint and can see fellow Fungi as their faction.
- Successful **Mira/TOU player-targeted custom ability interactions** pass infection when exactly one participant is contagious. Fungi members count as contagious sources, as do already-infected players.
- Default win threshold: **75% of living non-Fungi players**, configurable from **50–100%**.
- Win modes:
  - **Independent - Must Survive**: the Fungi faction ends the game when the threshold is reached while at least one Fungi remains alive.
  - **Additional Winner - Death Allowed**: reaching the threshold latches the Fungi objective; the match continues and Fungi are added as winners when the game later ends.
- Independent Fungi wins use a dedicated multi-Fungi custom game-over result.

## Alpha.6.6 HUD/growth follow-up

- Rescaled the 1254px approved Infect artwork at the asset level so the action graphic fits the normal Among Us/TOU button footprint while keeping the native cooldown text.
- Split the in-task Fungi explanation across five deliberately short lines so it stays inside the role information panel.
- Reworked fungal growth attachment: every visible strand is now parented directly to the live body renderer instead of the broader `PlayerControl` transform.
- Growth anchors are calculated from the current body sprite bounds, so the colonies follow the rendered crewmate body rather than floating beside it.
- Increased the growth size from the over-corrected alpha.6.4 values and gave the three stages distinct body-relative positions/rotations.
- The three visible colonies reuse the clean mushroom strand art with different orientation/placement, avoiding the reference-crewmate remnants that were present in the older Strand 2/3 source images.

### Carried forward from alpha.6.4

- Fixed the raw `TownOfUsMira.Role.Fungi` text in role-setting change notifications by supplying the TOU role locale keys that `ITownOfUsRole` expects.
- Reduced the direct Fungi role sprite size used by the Neutral Evil settings row without changing the approved source artwork.
- Reduced Strand 1/2/3 world scale and moved the anchors toward the character silhouette so growth no longer covers most of the visor/body.
- Cleaned reference-crewmate pixels from Strand 2/3 and kept all fungal growth on one body-relative sorting level to reduce cosmetic clipping.
- An infected Impostor/Crewmate/Neutral still keeps their real role identity; infection is a status and does not replace their underlying role.

## Existing modifiers

### Eye Mask

Crewmate-only modifier.

- Completing a task rolls a configurable **50–100%** sleep trigger.
- Random pre-sleep delay: **1–10 seconds**.
- Default sleep duration: **5 seconds**.
- Screen darkness fades in/out instead of snapping instantly.
- Movement and interactions are locked while sleeping.
- Sleep is cancelled safely around meetings, exile, death, disconnects and traversal transitions.
- Uses a private TownOfNDev overlay rather than mutating Among Us' shared fullscreen renderer.

### Butterfingers

Universal modifier.

- Eligible actions can fumble and replay after a configurable short delay.
- Default fumble chance: **25%**.
- Default delay: **0.5–1.25 seconds**.
- Default internal cooldown: **5 seconds**.
- Kills, reports, vents, normal use/tasks, vanilla abilities and supported custom ability paths can be configured independently.
- Fungi's Infect button explicitly participates in Butterfingers and revalidates the target before the delayed retry.

### Laggy

Universal modifier.

- Simulates intermittent **movement-only** stutter; it never delays or drops network packets.
- Default arm chance: **60%**.
- Default random check interval: **8–15 seconds**.
- Default movement stall: **0.4 seconds**, configurable **0.2–0.6 seconds**.
- Skips vents, ladders/platform movement, targeting transitions, meetings, exile, minigames and Time Lord rewind.
- Optional short local afterimage provides a visual cue.
- Does **not** suppress the full `PlayerPhysics.FixedUpdate` loop.

## Target stack

Alpha.9 remains aligned to the **Town of Us: Mira 1.7.2** dependency line:

- Target framework: **.NET 6**
- Build SDK: **.NET 8 SDK recommended**
- TownOfUsMira **1.7.3**
- AllOfUs.MiraAPI **0.5.0**
- Reactor **2.5.0-ci.371**
- PerfectComms.Api **4.1.7.1**
- Among Us GameLibs **2026.8.18**

## Build

From PowerShell in the project root:

```powershell
dotnet restore .\TownOfNDev.sln
dotnet build .\TownOfNDev.sln -c Release --no-restore
```

Or use the included helper:

```powershell
.\build.ps1
```

To copy the resulting DLL directly into an Among Us installation:

```powershell
.\build.ps1 -AmongUsPath "C:\Program Files (x86)\Steam\steamapps\common\Among Us"
```

Expected output:

```text
TownOfNDev\bin\Release\net6.0\TownOfNDev.dll
```

Install the DLL into:

```text
Among Us\BepInEx\plugins\
```

Town of Us: Mira, MiraAPI and Reactor must already be installed. TownOfNDev uses `RequireOnAllClients`, so everyone in the lobby must have the mod.

## Alpha.9 compatibility audit

The alpha.9 source has been statically audited against the TOU Mira 1.7.3 / MiraAPI 0.5.0 source interfaces. Important migration fixes include:

- TownOfNDev TOU modifiers now use the 1.7.2 `IdPart` API instead of the removed 1.7.1 `LocaleKey` override.
- Embedded localization now registers through MiraAPI 0.5.0's `MiraLocaleManager.Register(...)` API instead of removed TOU locale internals.
- Troll and Fungi implement TOU's extension `IWinCondition` interface and are explicitly registered in `WinConditionRegistry` at plugin load. Troll resolves first at priority 3; Fungi remains priority 4.
- Fungi displays the normal TOU **Neutral Evil** task header and clears it correctly if the role is deinitialized/changed.
- Laggy now preserves TOU 1.7.2's base modifier activation/deactivation bookkeeping while resetting its local stutter state.
- Disconnected Fungi are excluded from the explicit custom-game-over winner payload.

This environment does not have the .NET SDK installed, so the final compiler pass must be run on the development PC using the commands above. `TESTING.md` contains the first runtime checklist after the build succeeds.

## Assets

Confirmed/current embedded assets:

- `EyeMask_Modifier_Icon.png`
- `Butterfingers_Modifier_Icon.png`
- `Laggy_Modifier_Icon.png`
- `Fungi_Role_Icon.png`
- `Fungi_Infect_Button.png`
- `Fungi_Strand_1.png`
- `Fungi_Strand_2.png`
- `Fungi_Strand_3.png`
- `Tracer_Role_Icon.png`
- `Tracer_Dust_Button.png`
- `Tracer_Trace_Handprint.png`
- `Condemner_Role_Icon.png`
- `Condemner_DeathNote_Button.png`
- `Condemner_DeathRow_Skull.png`
- `Troll_Role_Icon.png`

## License

GPL-3.0. TownOfNDev is an unofficial Among Us mod and is not affiliated with or endorsed by Innersloth.

alpha.6.7 follow-up:
- Fungi now matches TOU Neutral Evil task-console behaviour: ordinary Crewmate task consoles cannot be completed by Fungi.
- Infect button artwork is slightly smaller to better match the surrounding vanilla/TOU HUD buttons while keeping native cooldown text.
- Fungi growth body-renderer null handling was tightened to remove the alpha.6.6 nullable compiler warning without changing attachment behaviour.


alpha.6.7 interaction-transmission hotfix:
- Fungi members now count as contagious interaction sources even though they do not carry the victim infection modifier.
- Successful player-targeted interactions are sent to the host for authoritative transmission validation instead of being filtered using possibly stale client infection state.
- If a clean player successfully interacts with Fungi or an infected player, the clean player becomes Stage 0 infected. If an infected player successfully interacts with a clean player, the clean player becomes Stage 0 infected. Fungi members never become infected victims.


alpha.6.7 network-state hotfix (same version):
- Replaced TownOfNDev's local/custom infection-application route with MiraAPI's built-in `RpcAddModifier<FungiInfectedModifier>` synchronization path.
- The host now installs a newly infected player's modifier once through MiraAPI, giving host and all clients the same modifier GUID, Stage 0 constructor state, target filtering, Fungi name tint and growth state.
- Direct Infect also uses the same synchronized modifier path; its existing result RPC now only confirms success/cooldown to the Fungi owner.
- This specifically fixes the case where the host knew a player was infected but another Fungi client still saw that player as clean and kept flickering/targeting them with Infect.
