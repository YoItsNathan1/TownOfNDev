## Beta.1 promotion validation

1. Restore/build Release and confirm `TownOfNDev-0.1.0-beta.1.dll` is produced.
2. Confirm the mod reports/version-labels itself as `0.1.0-beta.1` wherever the project version is surfaced.
3. Re-run the existing alpha.12.0.4 regression checklist unchanged.
4. Confirm the compiled DLL remains within the expected post-optimisation size range.
5. Confirm all role/modifier icons and action/status artwork render with the same apparent size as alpha.12.0.4.
6. Confirm Tank protection and Reverser reflection still pass their core gameplay tests.
7. Treat any beta-only code changes after this point as new beta patches rather than altering the alpha history.

## Alpha.12.0.4 asset optimisation regression

1. Restore/build Release and confirm `TownOfNDev-0.1.0-beta.1.dll` is produced.
2. Compare the compiled DLL size against alpha.12.0.3.
3. Check role settings cards for every TownOfNDev role and modifier: icons should retain their prior apparent size and transparency.
4. Check role-intro/reveal icons for Tank, Reverser, SUI, Tracer, Condemner, Fungi and Troll for sharpness and correct scale.
5. Check action/status artwork: Reverser Alert, Tank protection, SUI Protect, Tracer Dust, Condemner Death Note, Fungi Infect.
6. Confirm small Condemner trackers/skulls, Tracer handprint and Fungi strands are unchanged in apparent size.
7. Re-run one Tank protection and one Reverser reflection test to confirm this resource-only patch introduced no gameplay regression.

## Alpha.12.0.3 visual regression

1. Build Release and confirm `TownOfNDev-0.1.0-beta.1.dll` is produced.
2. Open Roles settings and confirm Tank's icon on the Tank quota/settings row is visibly smaller than alpha.12.0.2 while remaining centred.
3. Confirm Tank's intro/reveal description fits on-screen: `Finish your tasks to resist direct kills.`
4. Spawn Reverser and confirm the Alert artwork sits slightly smaller inside the HUD ability button while cooldown, uses and keybind indicators retain their normal positions.
5. Re-run the Reverser direct-reflection test; no gameplay behaviour should differ from alpha.12.0.2.

## Alpha.12.0.2 compile regression

1. Restore/build Release.
2. Confirm no unresolved `TownOfUsColors`, `MiraNumberSuffixes`, or `MeetingCheck` diagnostics.
3. If build succeeds, continue the Reverser gameplay regression from alpha.12.0 unchanged.

## Alpha.12.0.1 compile regression

1. Build Release and confirm `ReverserAlertButton.cs` no longer reports CS0246 for `ButtonUsesMode`.
2. Continue the alpha.12.0 Reverser/Tank gameplay tests unchanged.

## Alpha.12.0 Reverser + Tank visual regression

1. Build Release and confirm `TownOfNDev-0.1.0-beta.1.dll` is produced with no compile errors.
2. In role settings, confirm **Reverser** appears under **Impostor Power Roles**, its icon renders correctly, and its cog remains configurable at role count/chance `0`.
3. Confirm Reverser defaults: Number of Reverses `10`, Reverse Cooldown `30s`, Reverse Duration `15s`.
4. Spawn as Reverser. Confirm the normal Impostor Kill/vent/sabotage functions remain available and the **Alert** ability button appears.
5. Activate Alert. Confirm the button enters its active/effect timer state for the configured duration and consumes exactly one per-game use.
6. During Alert, have another player attempt an ordinary direct kill. The original kill must fail, Reverser must survive, and the attacker must receive the reflected murder instead.
7. Confirm Alert remains active after that first reflection. A second eligible attacker during the same window should also be reflected.
8. Let Alert expire without a meeting. Confirm Reverse mode ends and the configured cooldown begins.
9. Attempt the same direct kill while Alert is inactive. Reverser should die normally.
10. During Alert, test an indirect murder and a defense-bypassing murder. They must not be reflected by Reverser.
11. Start a meeting during Alert. Confirm Alert ends immediately. After the meeting, the ability must respect the configured cooldown and the spent use must remain spent.
12. Confirm a reflected kill does not reset Reverser's normal Impostor Kill cooldown.
13. Tank regression: confirm Tank's role-summary icon is slightly smaller than alpha.11.0.1 and the intro line `Finish your tasks to resist direct kills.` fits without clipping.
14. Re-run Tank's gameplay test to confirm task-completion protection is unchanged.

## Alpha.11.0.1 compile regression

1. Restore and build the solution in Release mode.
2. Confirm TankSystem produces no `HasDied`, `AdjustNotification`, or nullable dereference diagnostics.
3. Then continue the alpha.11.0 Tank gameplay tests unchanged.

## Alpha.11.0 Tank focused regression

1. Build and confirm `TownOfNDev-0.1.0-alpha.11.0.1.dll` is produced with no compiler errors.
2. Enable one Tank at 100% chance and start a normal game with at least one task.
3. Before the Tank finishes all tasks, confirm an ordinary Impostor kill can kill the Tank.
4. Complete the Tank's final task and confirm the activation notification + Tank Protection HUD status appear.
5. Attempt a normal direct Impostor kill after activation and confirm the Tank survives.
6. With `Can See Who Tried To Kill` On, confirm the Tank sees the attacker's name after the blocked attempt; turn it Off and confirm the reveal is suppressed.
7. Confirm meeting/ejection deaths still work normally and that known indirect or defense-bypassing kill paths are not blocked by Tank.
8. Regression: verify Tracer, SUI, Condemner, Fungi, Troll and the modifier settings pages still open/return correctly.

## Alpha.10.5.17 focused regression

1. Fresh lobby/settings session: open TownOfNDev **Modifiers first**, without opening Roles. Confirm the modifier list appears immediately.
2. Switch to **Roles**, open Tracer/SUI/Condemner cogs, and confirm their advanced controls are visible and interactive.
3. Open a modifier cog, return to the modifier list, switch to Roles, open a role cog, then press **Return to Role Settings**. Confirm it returns to the Roles list, never the Modifiers list.
4. Repeat Roles <-> Modifiers switching several times and confirm neither list overlays, blanks, or steals the other's return behaviour.

## Alpha.10.5.15 focused regression

1. Set a modifier to Amount `0` and Chance `0`.
2. Press Amount `+` once and confirm it becomes Amount `1`, Chance `50`.
3. Reset to `0 / 0`, then press Chance `+` and confirm Amount automatically becomes `1`.
4. Confirm the modifier quota click + settings-change audio still matches the Roles interaction.


## alpha.10.5.8 modifier row visual regression

- Confirm every modifier row shows the same white `-` and `+` button boxes as the Roles page.
- Confirm both Amount and Chance values use the same bordered black value boxes as role rows.
- Confirm the vertical divider between Amount and Chance is visible.
- Confirm modifiers with advanced settings show the cog icon in the same position/style as roles.
- Confirm no role names/rows appear on the Modifiers page.
- Confirm modifier amount/chance buttons still change only the modifier options.

# TownOfNDev testing checklist — through 0.1.0-alpha.10.5.17

# Alpha.10.5 modifier focused checklist

## Modifier configuration UI

- Open TownOfNDev → **Modifiers** and confirm the generic vertical option cards are replaced by role-style rows.
- Confirm **Eye Mask** appears under Crewmate Modifiers, **Menace** under Impostor Modifiers, and **Butterfingers / Laggy / Soulhandler / Miracle / Null** under Universal Modifiers.
- Confirm each row shows its approved icon/name plus **# OF ROLE** and **% CHANCE** controls.
- Confirm the amount/chance +/- buttons update the actual modifier assignment settings.
- Confirm modifiers with extra settings show a cog and expanding it exposes only that modifier's advanced options.
- Confirm faction headers can collapse/reopen their modifier rows and the scroll bounds remain usable.
- As a non-host, confirm amount/chance controls are read-only.

## 1. Registration and assignment

- Confirm Menace, Soulhandler, Miracle and Null appear in the TownOfNDev modifier settings.
- Confirm each Amount option defaults to 0 and exposes its Assignment Chance when Amount is above 0.
- Confirm Menace can only be assigned to Impostor-aligned roles.
- Confirm Soulhandler, Miracle and Null can be assigned across eligible non-Spectator factions under the normal universal-modifier rules.
- Confirm Town of Us' existing **Mini** modifier remains available separately and there is no TownOfNDev Mini naming collision; the new survival modifier appears only as **Miracle**.

## 2. Menace

- Set the lobby kill cooldown to 30 seconds and Menace reduction to 25%; confirm the effective cooldown is approximately **22.5 seconds**.
- Repeat at 15% (approximately **25.5 seconds**) and 50% (**15 seconds**).
- Confirm the reduction is applied to the cooldown already resolved by Town of Us (including map/compatible cooldown adjustments), not to a hard-coded value.
- Confirm the normal kill cooldown UI and kill reset use the same reduced value.
- Confirm the TOU 5-second lower safety bound is respected.
- Confirm non-Menace players receive unchanged cooldowns.

## 3. Soulhandler

- Kill one Crewmate, one Impostor and one Neutral where possible, then start a meeting as Soulhandler.
- Confirm only the Soulhandler client sees `Crewmate`, `Impostor`, or `Neutral` beneath the corresponding dead player's meeting entry.
- Confirm exact role names are not revealed by Soulhandler.
- Confirm living players and disconnected players receive no Soulhandler faction label.
- Confirm another client without Soulhandler sees no private labels.
- Confirm labels update safely if a player dies during a meeting and disappear with the meeting UI.

## 4. Miracle

- Set Kill Survival Chance to 100%; every eligible out-of-meeting murder attempt against Miracle should fail and show normal protected-kill feedback.
- Set the chance to 5% and 50% and confirm each attempted murder rolls independently rather than granting a permanent shield.
- Confirm a failed Miracle roll allows the kill to resolve normally.
- Confirm a successful Miracle roll does not kill Miracle and does not create a body.
- Confirm self-kills, meeting guesses/executions and ejections are not protected by Miracle.
- Confirm existing Guardian/protection mechanics resolve before Miracle so one attack does not consume two protection systems.
- Confirm Troll cannot receive Miracle. Where those role types exist, confirm Tank, Unstoppable and Terrorist are also excluded.

## 5. Null

- Give Null to one player and have three players visibly vote the same target, including Null. Confirm three vote icons are displayed but the target receives only **2 actual voting power**.
- Create a scenario where removing Null's vote changes an exile into a tie/no exile and confirm the numerical result follows the zero-weight tally.
- Have Null vote Skip; confirm the Skip vote is visually shown but contributes zero to the Skip tally.
- Test multiple Null holders and confirm every Null vote has zero tally weight while remaining visible.
- Confirm voting still completes normally for Null and they receive the normal local vote confirmation/UI state.
- Confirm systems that care who visibly voted (rather than vote weight) still receive the submitted vote.

## 6. Regression

- Confirm Condemner Death Row execution, end-game death cause, guess accounting and vision-aware Death Note tracker still behave as in alpha.10.4.4.2.
- Confirm Eye Mask, Butterfingers and Laggy still assign and function normally.
- Confirm Fungi, Tracer, Troll and SUI remain unaffected.


Run the compiler pass first. Then test with at least two modded clients; use three or more when checking propagation and multi-Fungi behavior.


## Alpha.10.4 Condemner visual checks

1. Give Death Notes to two or more valid players before a meeting. Confirm only the Condemner sees the parchment Death Note tracker beside every marked player's name in normal gameplay.
2. Confirm unmarked players never receive the private tracker and other players cannot see it.
3. Start a meeting. Confirm every living condemned player has the public red skull beside their name and the Condemner also sees the private tracker on their own view.
4. Confirm the new public marker reads clearly as a skull at normal meeting scale.
5. Let Death Row execute at voting resolution. Each executed player's skull and private tracker must disappear immediately when that player dies; neither marker may linger into the ejection transition.
6. Guess/kill/disconnect a condemned player during the meeting. Their markers must disappear immediately while other condemned players remain marked.
7. Confirm marker positions continue following the rendered player name through discussion, voting, and results layout changes.

## Alpha.10.3.3 build-output check

1. Build Release and confirm the output file is `TownOfNDev-0.1.0-alpha.10.4.4.2.1.dll`.
2. Confirm the versioned DLL is copied to the configured Among Us plugin directory when `AmongUs` is set.
3. Re-run the alpha.10.3.1 Condemner skull placement check and alpha.10.3 Troll guessability checks.


## Alpha.10.1 focused SUI UI checks

1. Spawn as SUI and confirm the role information panel is substantially narrower and reads as six compact lines without clipping.
2. Confirm the same compact description remains readable in both the initial role card and the persistent task/role information block.
3. Confirm Protect artwork is slightly larger than alpha.10.0 without escaping the native ability-button frame.
4. Confirm the `F` keybind badge and native cooldown number remain correctly positioned/readable and were not enlarged with the sprite.
5. Confirm no SUI options, protection behavior or role registration changed.

## 1. Registration and UI

- TownOfNDev loads without BepInEx/Mira/Reactor registration exceptions.
- Fungi appears under **Neutral Evil** role settings.
- Fungi uses the confirmed role icon.
- Fungi's role/task header identifies the Neutral Evil alignment correctly.
- Infect appears only while the local player is Fungi and alive.
- Infect uses the approved button artwork without a duplicated Mira text label.
- A nearby valid target highlights while Infect is cooling down, but clicking cannot infect until the timer reaches zero.
- Leaving range removes the valid target/highlight.

## 2. Direct Infect

- Clean non-Fungi player in range can be infected.
- Fellow Fungi cannot be targeted.
- Already-infected players cannot be targeted.
- Invalid/range-lost host validation returns the button without consuming cooldown.
- Successful infection consumes cooldown exactly once.
- Two Fungi attempting to infect the same clean player do not create duplicate infection state.
- With Butterfingers enabled for abilities, a fumbled Infect waits, then succeeds only if the original action is still valid when retried.

## 3. Infection stages

- Fresh infection is Stage 0 and visually hidden from normal players.
- Fungi can still identify the infected player at Stage 0 through the Fungi-side name tint.
- After the next meeting ends, Stage 0 becomes Strand 1.
- Following meetings advance to Strand 2 and Strand 3.
- Stage 3 does not advance beyond three strands.
- Strand art follows player facing direction and remains attached during ordinary movement.
- No stranded fungal sprites remain after disconnect/death/scene transitions.

## 4. Death and revival

- When an infected player dies, active infection and strand visuals disappear immediately.
- With **Restore Infection After Revival = On**, revival restores the exact pre-death stage.
- With the option Off, revival returns the player clean.
- Dead/disconnected non-Fungi do not count in the living-player threshold denominator.

## 5. Propagation

- For a successful player-targeted Mira/TOU custom ability interaction, infection spreads when exactly one side is contagious (Fungi itself OR an infected player).
- Infection does not spread when both players are clean.
- Infection does not duplicate when both are already infected.
- Infection does not spread merely from proximity, standing in the same room, voting, or chat.
- Cancelled/failed player-targeted abilities do not spread infection.
- Fungi's own Infect button is not double-counted by the generic propagation hook.

## 6. Multi-Fungi faction

- With 2–3 Fungi, each Fungi sees the other Fungi as faction members.
- The intro team contains the Fungi faction members rather than unrelated neutrals.
- All Fungi share the same infection pool/threshold.
- Infection created by one Fungi contributes to every Fungi member's objective.

## 7. Win modes

### Independent - Must Survive

- Reaching the configured threshold while at least one Fungi is alive triggers the Fungi custom game over.
- Fungi members are the winners; unrelated players are not added as Fungi winners.
- The end screen uses the Fungi win treatment.
- Reaching a near-threshold value does not end the match early.

### Additional Winner - Death Allowed

- Reaching the threshold latches the Fungi objective but does not immediately end the match.
- The game continues until another normal/custom ending occurs.
- Fungi are included as winners after the objective has been latched.
- Fungi do not win if the objective was never achieved.

## 8. Regression checks

- Eye Mask still sleeps, fades the overlay and restores normal movement/vision afterward.
- Butterfingers still fumbles its existing supported actions.
- Laggy still produces short movement-only stutters without persistent darkness or vision issues.
- Meetings, exile, vents, ladders/platforms and Time Lord rewind do not leave TownOfNDev movement locks stuck.
- Submerged transitions do not leave Fungi strands or TownOfNDev movement state in an invalid position/state.

## Alpha.6.5 visual/HUD regression checks

1. In Neutral Evil role settings, confirm the Fungi icon fits inside the row and does not overlap the category header or neighboring controls.
2. Change Fungi count/chance and confirm the notification says `Fungi`, never `TownOfUsMira.Role.Fungi`.
3. Infect a player and confirm Stage 0 remains completely invisible until the next meeting ends.
4. After the first post-infection meeting, confirm Strand 1 is a small attached growth and does not cover most of the visor/body.
5. Advance to Strands 2 and 3 and confirm each new growth remains compact, has no white reference-crewmate chunks, and does not swallow hats/skins.
6. Test both left- and right-facing movement and confirm all three anchors mirror correctly.
7. Infect an Impostor and confirm their actual role identity remains Impostor while the fungal status is only represented by the growth/infection visibility rules.

## Alpha.6.5 focused regression

1. Spawn as Fungi and confirm the Infect button is visible immediately after the HUD becomes active, even with no target in range.
2. With no valid target nearby, confirm Infect is greyed out rather than hidden.
3. Move a clean player into standard ability range and confirm the button lights up while the native cooldown can still continue counting down.
4. Move the player back out of range and confirm the button greys out again.
5. Confirm a meeting hides Infect and that it returns after the meeting ends.
6. Confirm the Fungi task/role information block is split into compact lines and no longer stretches as one long sentence across the screen.


## Alpha.6.6 focused visual regression

1. Spawn as Fungi and confirm the Infect artwork is approximately the same visual footprint as the other action buttons rather than filling the lower-right corner.
2. Confirm the native cooldown number remains centered/readable over the Infect artwork.
3. Confirm the Fungi role information block uses five short lines and stays inside its panel.
4. Infect a living player, hold through a meeting, and confirm Strand 1 appears physically attached to the rendered crewmate body.
5. Walk left/right and confirm Strand 1 moves with the body with no visible world-space gap.
6. Advance to Strand 2 and Strand 3 and confirm each added colony is clearly visible, body-attached, and mirrored correctly when the player turns.
7. Confirm the colonies are larger than alpha.6.5 but still do not cover most of the visor/body.
8. Confirm hats/skins and normal movement do not leave a fungal sprite behind at the old world position.

## alpha.6.7 focused checks
- As Fungi, attempt an assigned/fake normal task console (e.g. Wiring): it must not open/complete.
- Confirm non-task interaction and Infect still work normally.
- Confirm Infect art is slightly smaller but native cooldown number remains correctly positioned/readable.
- Confirm Strand 1/2/3 remain attached to the body exactly as in alpha.6.6.


### Interaction-transmission hotfix checks
- Clean player successfully targets/interacts with Fungi -> clean player becomes Stage 0 infected.
- Clean player successfully targets/interacts with an infected player -> clean player becomes Stage 0 infected.
- Infected player successfully targets/interacts with a clean player -> clean player becomes Stage 0 infected.
- Failed/cancelled player-targeted interaction -> no infection.
- Interaction where both players are already contagious -> no duplicate infection/change.
- Fungi never receives the victim infection modifier.
- Newly spread infection remains visually hidden at Stage 0 until the next meeting ends.


### alpha.6.7 cross-client infection-sync regression
1. Host as a clean role successfully uses a player-targeted ability (for example Jester Poke) on a Fungi client.
2. On the host, confirm the host becomes Stage 0 infected.
3. On the Fungi client's screen, confirm the host's name immediately becomes the Fungi infection colour even though Stage 0 has no visible mushroom growth yet.
4. Keep the host in Infect range: the Fungi client's Infect button must NOT select/highlight/flicker on that already-infected host.
5. Repeat with an infected non-Fungi player as the contagious side and a third clean player as the interaction target; all clients must agree that the third player becomes infected.
6. Direct Infect a clean remote player and verify host, Fungi owner and the infected player's client all agree on infection state before the next meeting.
7. After a meeting, confirm the same Stage 1 strand appears for that infected player on every client.


# Alpha.7 Tracer focused checklist

## 1. Registration / HUD

- Tracer appears under **Crewmate Investigative** settings and uses the approved Tracer role icon.
- Spawn as Tracer: Dust is visible immediately, uses the approved artwork, and does not show a duplicate Mira text label.
- Dust has one use for the round. No valid target -> grey; a valid nearby living target -> active.
- Dusting a player consumes the use only after host validation succeeds.
- After the next meeting, the Dust use returns to 1.

## 2. Dust privacy / duration

- The Dusted player receives no notification, UI modifier or visible body mark.
- Other non-Tracer players cannot identify who is Dusted.
- Default Dust remains active for about 25 seconds; repeat with 15s and 45s option values.
- The same Tracer cannot Dust the same target twice in one round and cannot use a second Dust before the meeting reset.

## 3. Successful interaction -> Trace

- Dust Red, then have Blue successfully use a player-targeted role ability on Red -> Blue becomes Traced.
- Dust Red, then have Red successfully use a player-targeted role ability on Blue -> Blue becomes Traced.
- Failed/cancelled/fumbled-and-invalid interactions produce no Trace.
- Merely standing near Red, voting, chatting, doing tasks, venting or reporting does not create Trace.
- A Traced Blue interacting with Green does **not** Trace Green.

## 4. Kill evidence

- Dust Red, then have Blue successfully kill Red -> Blue becomes Traced.
- The dead Dusted victim does not retain a useful Trace marker.
- A successful murder is not double-counted by the generic custom-button interaction watcher.

## 5. Private visuals

- Only the owning Tracer sees the small cyan powder handprint on a Trace carrier.
- The handprint stays attached while the carrier walks and mirrors sensibly with facing.
- At normal ability range/line of sight, the Trace carrier receives a soft cyan outline for the owning Tracer only.
- Moving out of range or behind normal line-of-sight obstruction removes the Tracer outline.
- Entering a vent or becoming truly invisible hides both the handprint and outline; no floating evidence reveals the hidden player.

## 6. Meeting lifecycle

- Calling a meeting immediately removes active Dust so no meeting action can create new Trace.
- Existing Trace remains during the meeting.
- With **Show Trace In Meetings = On**, the owning Tracer sees a small handprint marker beside each living Trace carrier.
- With the option Off, no meeting markers appear.
- When gameplay resumes after the meeting, all old Trace evidence and markers are gone and the Dust use is refreshed.

## 7. Death / disconnect / multiple Tracers

- Killing a Trace carrier removes their evidence. Revival starts clean.
- Killing/disconnecting the Tracer clears all Dust and Trace owned by that Tracer.
- With two Tracers, each can Dust a different player and sees only evidence generated by their own Dust.
- If both Tracers Dust the same target, a later contact can carry both private Trace records while each Tracer sees only their own evidence.

## 8. Fungi cross-role regression

- As clean Tracer, Dust an infected player/Fungi: the successful Dust contact can infect the Tracer through Fungi's existing interaction-spread system.
- Dust itself does **not** create the Tracer's own Trace mark.
- If a Dusted Fungi successfully Infects Blue, Blue becomes infected and also receives the owning Tracer's Trace evidence.
- Fungi direct Infect, name tint, strand growth and win conditions remain unchanged.

# Alpha.8 Condemner focused checklist

## 1. Registration / basic HUD

- Condemner appears under **Impostor Killing** and can only be configured as 0 or 1.
- Spawn as Condemner and confirm the approved flaming-scroll role icon appears correctly.
- Confirm **Death Note** uses the approved red Impostor artwork, has no duplicate Mira label, and begins on the configured full cooldown (45s default).
- No valid target -> button greyed. A nearby valid non-Impostor -> target outline/button active. Impostor teammates are never targetable.

## 2. Successful Death Note

- Use Death Note on a normal unprotected target. The target receives no notification or world/body marker.
- Confirm Death Note starts its own cooldown and the normal Kill button is reset to its full cooldown only after host confirmation.
- Confirm the same already-condemned target cannot be selected again.
- Normal Kill usage does not reset Death Note.

## 3. Round-use limit

- With the limit enabled and maximum 1–5, confirm exactly that many successful Death Notes can be committed in a round.
- Failed/invalid requests do not consume a use.
- After a completed meeting, the per-round count resets.
- With **Limit Death Note Uses Per Round = Off**, confirm there is no use cap and the 45s/configured cooldown is the only limiter.

## 4. Protection / interaction compatibility

- Death Note a Medic-shielded player: the sentence should apply because Medic protects against killing, not the interaction.
- Attempt Death Note on a **Warden Fortified** player: Warden must cancel it completely; no sentence, Death Note cooldown, round use or Kill reset should be consumed.
- If Butterfingers delays Death Note, move/fortify/otherwise invalidate the target before replay and confirm host revalidation rejects it cleanly.
- Death Note an infected/Fungi player and confirm the successful contact can spread Fungi infection to the Condemner.
- Death Note a Tracer-Dusted player and confirm the Condemner can receive that Tracer's one-hop Trace evidence.

## 5. Meeting skull reveal

- Condemn one or more players, call any body/emergency meeting, and confirm every living prisoner gets the public red skull beside their meeting card.
- All clients must see the same skull set.
- Prisoners can still vote, guess and use legal meeting abilities normally.
- If a prisoner is guessed/killed during the meeting, their existing skull visual may remain but they must not be executed again.

## 6. Default Condemner-death cancellation

With **Death Row Persists After Condemner Dies = Off**:

- Kill the Condemner during the round before a meeting -> all existing sentences are cancelled and no skulls should appear at the next meeting.
- Guess/kill the Condemner during the meeting -> every Death Row skull remains visible for the remainder of the meeting, but all prisoners survive the Death Row execution checkpoint.
- Vote the Condemner out -> skulls remain visible in the meeting results, but prisoners must survive.
- Confirm there is no immediate UI clue that killing the Condemner cancelled the sentence.

## 7. Persistence enabled

With **Death Row Persists After Condemner Dies = On**:

- Kill the Condemner during the round, then call a meeting: existing prisoners still show skulls and execute.
- Guess/kill or vote out the Condemner during the meeting: existing sentences still execute.

## 8. Vote-resolution / simultaneous execution

- Tie vote -> valid prisoners execute.
- Skip vote -> valid prisoners execute.
- Emergency/body-report meetings both resolve Death Row.
- If a Death Row prisoner is the player being ejected, the ejection takes priority and there is no second Death Row murder.
- With 2+ prisoners, confirm all valid Death Row nameplate deaths begin on the same results frame rather than a deliberate one-after-another sequence.
- Confirm the meeting then continues into the normal ejection/results transition without hanging or duplicating bodies.

## 9. Cross-client state

- Apply Death Note from a non-host Condemner and verify the host and all clients agree on the hidden condemned state once the meeting starts.
- Confirm skulls appear for everyone even though nobody saw a round-time notification.
- After meeting resolution/new round, old Death Row state must be fully cleared and cannot reappear in a later meeting.


# Alpha.9 Troll focused checklist

## 1. Registration / role UI

- Troll appears under **Neutral Evil** settings and uses the approved grey inverted-Jester icon.
- Spawn as Troll and confirm the role name/colour/icon render correctly in role intro, settings and wiki surfaces.
- Troll has no Kill, Sabotage or Vent ability.
- Troll cannot complete ordinary Crewmate task consoles; any assigned task list is cosmetic/fake.

## 2. Core win condition

- Have another player successfully kill the Troll during normal gameplay -> the game should immediately end with **Troll Wins!** and only that Troll listed as the winner.
- Repeat with an Impostor kill, a valid Crewmate-killing role, and a valid Neutral-killing role.
- Confirm protection-blocked/failed kill attempts do not arm the Troll win.

## 3. Meeting kills / guessing

- Troll should not be offered as a valid guess target in TOU guessing interfaces.
- Have the Troll misguess and kill themselves -> Troll must **not** win.
- Any other self-kill path where source == target must not count.

## 4. Ejection / non-murder deaths

- Vote the Troll out -> Troll loses; no Troll game-over should trigger.
- Disconnect the Troll -> no Troll win.
- Trigger a non-player-caused death path, if available -> no Troll win.
- With **Indirect Kills Count as Win** Off, chain/remote/indirect murders must not count even when another player is recorded as the source.

## 5. Multiple Trolls

- Enable 2–3 Trolls. Kill one Troll -> that killed Troll wins alone and the match ends immediately.
- Ensure the surviving Troll(s) are not included in the winner list.
- Confirm simultaneous/end-frame faction conditions do not override the Troll: Troll priority 3 should resolve before Fungi and normal TOU endings.

## 6. Regression

- Fungi independent win still resolves normally when no Troll murder has occurred.
- Condemner Death Row, Tracer Dust/Trace and existing modifiers behave unchanged.
- Start a fresh match after a Troll win and confirm no stale Troll winner state carries into the new game.


# Alpha.9.2 Troll configuration checklist

## 1. Settings UI

- Troll settings show **Can Use Button**.
- Troll settings show **Has Impostor Vision**.
- Troll settings show **After Win Type** with **Continues Game** and **Ends Game**.
- **Continues Game** is the default.
- **Announce Troll Win** is visible only while Continues Game is selected.
- Confirm there are no Troll vent, vent cooldown/duration, or scatter settings.

## 2. Continues Game mode

- Another player successfully kills Troll -> match does **not** end.
- Troll is recorded as having secured a personal win.
- If Announce Troll Win is enabled, all clients receive the Troll win notification once.
- If Announce Troll Win is disabled, no notification is shown.
- Finish the match with a normal Crew, Impostor, or Neutral ending -> successful Troll appears as an additional winner.
- With 2–3 Trolls, each one successfully killed by another player can independently become an additional winner.
- A surviving/unsuccessful Troll must not be added to the winners.

## 3. Ends Game mode

- Another player successfully kills Troll -> match immediately ends with **Troll Wins!**.
- Only the first successfully killed Troll is the winner for that immediate ending.

## 4. Button / vision options

- Disable Can Use Button -> Troll has no emergency meetings available.
- Enable Can Use Button -> normal emergency-button allowance is retained.
- Toggle Has Impostor Vision and verify the Troll's light radius follows the setting.

## 5. Invalid win paths remain invalid

- Troll is voted out -> no Troll win.
- Troll disconnects -> no Troll win.
- Troll self-kills/misguesses -> no Troll win.
- A failed/protected murder attempt -> no Troll win.


## Alpha.9.3 role browser / wiki cleanup

- Open the in-game role/modifier browser and inspect Troll, Fungi, Tracer, Condemner, Eye Mask, Butterfingers, and Laggy.
- Confirm each main description reads as a normal player-facing explanation of what the role/modifier does.
- Confirm Troll does not call itself the opposite of Jester and does not mention host choices in the description body.
- Confirm option values still appear in the normal Options section below the description.
- Confirm the internal Fungi Faction Marker does not appear anywhere in the role/modifier browser.
- Confirm Fungi additional-winner behaviour still works in later gameplay testing; the marker remains internal and functional.


## Alpha.9.4 Troll direct/indirect kill checklist

### 1. Settings
- Troll settings show **Indirect Kills Count as Win**.
- The option defaults **Off**.
- Troll remains absent from normal TOU guessing target lists.

### 2. Direct kills
- With the option Off, a normal Impostor kill on Troll secures the Troll objective.
- Repeat with direct Crewmate-killing and Neutral-killing role actions that produce a direct successful murder.
- Failed/protected direct attacks do not secure the objective.

### 3. Indirect kills disabled
- With the option Off, kill Troll through an available murder path flagged `IsIndirectAttack == true` (for example an AOE, delayed, remote, or chain-style role kill).
- Troll must not secure a win and no Troll win announcement/game-over should occur.

### 4. Indirect kills enabled
- Enable **Indirect Kills Count as Win** and repeat the same indirect murder.
- Troll should now secure the objective.
- Verify both **Continues Game** and **Ends Game** modes behave exactly as they do for a direct kill.

### 5. Invalid paths remain invalid
- Vote/eject Troll -> no win.
- Disconnect Troll -> no win.
- Troll self-kill -> no win.
- Confirm changing the indirect-kill option does not alter any of these paths.


# Alpha.10.0 SUI focused checklist

## 1. Registration / role UI

- SUI appears under **Crewmate Protective** and can be configured as 0 or 1.
- The approved orange/brown SUI role icon appears in settings, intro and the role browser without scaling/cropping problems.
- Spawn as SUI: the approved cyan **PROTECT** artwork appears as the role ability with no duplicate Mira `Protect` text over the image.
- SUI's role browser description is player-facing and the options appear underneath it normally.
- `SUI Protected`, `SUI Triggered Protection`, and `SUI Revealed Attacker` must **not** appear as selectable/wiki modifiers in the role/modifier browser.

## 2. Protect cooldown / targeting

- Default **Protect Cooldown** is 25s.
- Confirm the option can be changed only through the intended 20s, 25s, 30s, 35s, 40s and 45s values.
- SUI cannot target themselves.
- SUI can target the nearest eligible living player inside normal ability range and line of sight.
- Move the selected target out of range before the host accepts the request -> protection must fail without consuming the cooldown.
- Protect a player successfully -> the cooldown starts and that player is no longer a valid Protect target.
- SUI cannot manually remove, replace or re-protect a player who already has an active SUI protection.

## 3. Protection capacity

With **Limit Protected Players = On**:

- Default maximum is 3.
- Test each configured maximum from 1 through 5.
- Once the active protection count reaches the configured maximum, Protect remains visible but cannot select/apply another target.
- If one protected target dies/disconnects, that slot becomes available again while SUI remains alive.

With **Limit Protected Players = Off**:

- SUI can continue applying protection to additional eligible living players after five protections; cooldown remains the only normal limiter.

## 4. Protected-player HUD status

- The protected player's own client receives a visible **SUI Protected** modifier/status with the SUI icon and protection description.
- Unprotected players must not receive that status.
- The status persists through ordinary meetings while SUI remains alive.
- Killing/ejecting/disconnecting SUI removes every protected player's SUI status promptly.
- The status disappearing is therefore a valid indirect indication to the protected player that their SUI protection is gone.

## 5. First outside-meeting kill attempt

Use an ordinary Impostor Kill first:

- Before activation, the killer can select/highlight the protected target normally.
- Attempt the kill -> the target survives and no body is created.
- The protection itself remains active after the attempt.
- The protected target becomes activated for anti-spam targeting.
- Only the owning SUI sees the attacker with a persistent **red outline**.
- Other Crewmates, Impostors, Neutrals and the protected target must not receive the private red outline merely because SUI revealed the attacker.
- The failed attacker receives TOU's normal temporary save cooldown behaviour rather than being able to mash Kill continuously.

## 6. Anti-spam target suppression

After the protection has been activated once:

- The vanilla Kill button must no longer acquire/light up on that protected player.
- Test a TOU/Mira custom player-targeted killing button against the activated protected player. It may still visually acquire the player according to that role's own targeting rules, but clicking it must be cancelled and put onto the normal temporary save cooldown without killing or revealing a second attacker.
- Moving between protected and unprotected nearby players must not leave stale outlines or a stale `currentTarget` on the protected player.
- Despite target suppression, forcibly exercising another murder path against the protected player must still be blocked by the underlying SUI protection.

## 7. Direct / indirect / Neutral murder coverage

While SUI is alive, repeat against an actively protected player:

- Normal direct Impostor murder -> blocked.
- Direct Neutral-killing murder -> blocked.
- Direct Crewmate-killing murder, where applicable -> blocked.
- Indirect/delayed murder that reaches Mira `BeforeMurderEvent` -> blocked.
- AOE/chain/remote murder flagged indirect -> blocked.
- A murder path that requests `IgnoreDefense` -> SUI still blocks it.
- If an indirect attack resolves after its attacker is already dead, the protected target still survives; no living-attacker outline is required.

## 8. Existing protection interaction ordering

- Give the same target a TOU Medic shield and SUI protection. Attack them through a path Medic handles first -> SUI must not falsely activate/reveal the attacker if Medic already cancelled that attempt.
- Repeat with Warden/Cleric/Mirrorcaster-style protection where available.
- After the earlier protection is gone, a later attack handled by SUI must activate SUI normally.
- No double body, double notification, duplicate trigger modifier or duplicate red reveal should occur from one attempt.

## 9. Meeting bypass rules

- Correctly guess a SUI-protected player during a meeting -> the guess/execution is **not** blocked by SUI.
- Vote/eject a SUI-protected player -> ejection is **not** blocked.
- If the protected player dies through either meeting path, their SUI protection/status is removed and their slot becomes available if SUI survives.
- Condemner/other legitimate meeting execution mechanics remain unaffected by SUI protection.

## 10. SUI death / attacker death / disconnect lifecycle

- Kill SUI during normal gameplay -> every SUI protection stops applying immediately and all protected statuses disappear.
- Eject/guess SUI -> same cleanup.
- Disconnect SUI -> same cleanup.
- Once SUI is dead/disconnected, former protected players can be killed normally.
- Kill/disconnect the revealed attacker while SUI lives -> SUI's red outline for that attacker disappears.
- A reveal earned on attacker A must not transfer to attacker B just because B later attacks the same already-triggered protected target.
- If a previously revealed attacker dies, the same already-triggered protection does not reveal a new attacker later; only the first triggering attacker is earned.

## 11. Persistence across meetings

- Apply several protections, call/report a normal meeting, then return to gameplay.
- Protections and protected-player HUD status remain active after the meeting.
- Triggered anti-spam state remains active after the meeting.
- A living revealed attacker is outlined red again for SUI once normal world gameplay resumes.
- Meeting UI itself must not retain stray world-body outlines.

## 12. Multiple-client synchronization

Test with SUI on a non-host client:

- Protect another remote client -> host and every client agree the target has protection; only the protected owner's HUD shows the status.
- A third client attacks the protected target -> host blocks the murder, all clients agree the target survived, and only SUI sees the attacker outline.
- Repeat with the attacker as host and with SUI as host.
- Verify no client can continue selecting the activated protected player with a normal Kill button after trigger state synchronizes.

## 13. Cross-feature regression

- Troll direct/indirect win configuration behaves unchanged when SUI is absent.
- A failed SUI-blocked attack on Troll must not count as a successful Troll murder/win.
- Tracer murder evidence is not created for a SUI-blocked murder because no `AfterMurderEvent` should represent a successful death.
- Fungi interaction/infection behaviour remains unchanged by merely being protected.
- Condemner Death Row and meeting execution remain unaffected by SUI outside-meeting protection.
- Eye Mask, Butterfingers and Laggy still behave as before.
- Start a fresh game after a SUI match and confirm no old protection, trigger, reveal or outline state carries into the new match.


## Condemner summary/stat regression test
1. As Condemner, make one normal kill outside a meeting.
2. Place one or more different players on Death Row and let their sentences execute after voting.
3. Make one genuine meeting guess if Assassin is available.
4. End the game and verify: the normal victim says `Killed By <Condemner>`, Death Row victims say `Executed on Death Row By <Condemner>`, `Kills` only counts ordinary kills, and `Guesses` only counts genuine guesses.


## alpha.10.5.9 modifier advanced-panel regression
1. Open a role cog (especially SUI) and note its advanced settings.
2. Switch directly to Modifiers and open Menace, Butterfingers, Laggy, Eye Mask, and Miracle cogs.
3. Confirm no previous role-option labels/controls remain underneath the modifier options.
4. Return to Roles and confirm role advanced settings rebuild normally.

## alpha.10.5.14 page-isolation, first-load and Menace regression

- Fresh-lobby regression: without opening Roles first, open TownOfNDev > Modifiers. Confirm the modifier list appears immediately with no stock `All` button and no Tracer/SUI/Condemner list replacing it a moment later.
- Close/reopen the settings menu and repeat the direct Modifiers entry. Confirm the result is identical.
- With each modifier Amount at `0`, open every available cog and confirm its settings are still visible/configurable. Assignment chance must remain editable at Amount `0`.
- With every TownOfNDev role at `0` count / `0%` chance, confirm its cog remains available and its advanced settings can still be edited.

- Open TownOfNDev > Modifiers, then use the mod-page arrow to switch to TOU Mira while remaining on a compatible Roles/Modifiers page. Confirm **no TownOfNDev modifier headers or rows remain visible**.
- Switch back and forth several times, including from a modifier advanced-settings cog, and confirm no duplicated TownOfNDev rows appear.
- Set normal kill cooldown to `20s`, Menace reduction to `25%`, assign Menace, make a normal kill, and confirm the kill button resets to `15s`.
- Verify special timer resets such as `0s` are not incorrectly reduced.
- Confirm Menace's top-right modifier description reads `Your normal kill cooldown is reduced by 25%.` when configured to 25%.


## alpha.10.5.14 modifier quota UI regression
- Chance value boxes display the numeric value only; `% CHANCE` in the column heading supplies the unit.
- With modifier Amount = 0 and Chance = 0, pressing Chance + must set Amount to 1 and Chance to the next configured increment.
- With modifier Amount = 0, any chance change that results in a non-zero chance must automatically set Amount to 1.
- Modifier Amount/Chance +/- controls must play UI click audio, matching the Roles quota controls.

## alpha.10.5.14 modifier quota audio regression

1. Open TownOfNDev Roles and press a role Amount or Chance +/- button.
2. Open TownOfNDev Modifiers and press a modifier Amount or Chance +/- button.
3. Confirm both use the same quota-change click sound.
4. Confirm modifier Amount/Chance values still update normally and Chance > 0 still promotes Amount 0 to Amount 1.

## Alpha.10.5.17 focused regression

1. Fresh settings session: open TownOfNDev > Roles > Tracer cog. Confirm options render and `Return to Role Settings` returns to the role list.
2. Open TownOfNDev > Modifiers > Menace/Butterfingers cog. Confirm `Return to Modifier Settings` returns to the modifier list.
3. Repeat Roles -> cog -> return -> Modifiers -> cog -> return -> Roles -> cog -> return several times.
4. Confirm no return header becomes inert and no role/modifier list is substituted for the other.
5. Confirm opening Modifiers first in a fresh settings session still loads the modifier list immediately.
