# Road To Worldcup - Game Design Document

Last updated: 2026-07-09  
Vertical slice scope: portrait mobile football passing prototype

## 1. One-Page Vision

Road To Worldcup is a portrait 9:16 hybrid-casual football passing game where the player guides a ball through a chain of friendly teammates and finishes by reaching the star #10 near the goal. The experience should feel like a bright, blocky, toy-like stadium moment: simple to read, satisfying to pass, and immediately replayable after a mistake.

The vertical slice must prove:

- A polished-feeling main menu inspired by a stadium celebration screen.
- A playable hold-to-aim, release-to-pass football puzzle mechanic.
- Three short levels with increasing passing complexity.
- Clear visual language for active player, target player, opponents, ball, aim, power, pass previews, win, and fail.
- A brand-safe football tournament fantasy without real player likenesses, federation logos, FIFA branding, or official World Cup marks.

The core fantasy is: "Thread passes through pressure, deliver the ball to #10, and watch him score."

## 2. Target Platform and Orientation

- Platform target: mobile-first Unity prototype.
- Primary aspect ratio: portrait 9:16.
- Input model: single-finger touch or mouse equivalent in editor.
- Session length: 10 to 40 seconds per level.
- Performance target for prototype: stable 30 FPS minimum on common mobile devices, with simple geometry and readable effects.
- Primary scene flow: Main Menu -> Level Select implicit through Play -> Gameplay -> Win/Fail Overlay -> Next/Retry/Menu.

## 3. Audience and Feel

Target audience:

- Hybrid-casual mobile players.
- Football fans who enjoy quick puzzle-action challenges.
- Players who understand "pass to the highlighted teammate" without needing football simulation depth.

Feel pillars:

- Instant readability: the next useful pass should be visually obvious, even when timing is required.
- Tactile aim-and-release: holding should make the player feel in control, with power and direction feedback.
- Low-friction failure: mistakes restart quickly.
- Toy-stadium spectacle: blocky characters, bright grass, big UI, trophy/confetti mood.

## 4. Brand Safety Rules

Implementation must avoid:

- Real player names, faces, tattoos, likenesses, or signature celebrations.
- Real federation badges, national team crests, sponsor logos, or kit designs.
- FIFA, World Cup, official trophy, or tournament branding.
- Real stadium signage, official tournament typography, or copyrighted UI copying.

Allowed generic substitutes:

- Game title: Road To Worldcup.
- Generic football star: blocky friendly player wearing #10.
- Generic blue and white friendly kit.
- Generic red opponent kit.
- Generic gold trophy shape, not an official trophy replica.
- Generic stadium boards with fictional text such as "ROAD FC", "CHAMPIONS ROAD", "GOAL TIME", or abstract stripes.

## 5. Core Loop

1. Player enters a level with the ball attached to the first friendly player.
2. Active player idles by auto-rotating toward changing aim angles or moving laterally along a short lane.
3. Player studies teammate positions, opponents, and aim direction.
4. Player holds input.
5. Active player movement or auto-rotation stops.
6. Aim arrow becomes clearer, power begins charging, and a dashed path preview appears where feasible.
7. Player releases input.
8. Ball travels in the selected direction using the charged power.
9. If the ball contacts a valid friendly teammate, the pass succeeds and that teammate becomes active.
10. If the receiver is #10, #10 auto-shoots into the goal and the level is won.
11. If the ball misses, hits an opponent, exits the field, or stops short, the level fails.
12. Player retries, continues, or returns to menu.

## 6. Controls

### Touch Input

- Hold anywhere on the screen: lock the active player's current movement/rotation, show clear aim state, and charge pass power.
- Release: pass the ball in the aim direction with the current charged power.
- Tap buttons: interact with menu, pause, retry, next, and skip tutorial.

### Editor Input

- Mouse down mirrors touch hold.
- Mouse up mirrors touch release.

### Input Timing Rules

- Holding should be accepted only while the level is in the Playing state and the active player has the ball.
- Release with very low power should still launch a weak pass. If it stops before a teammate, the level fails.
- Input is ignored while the ball is traveling, during auto-shot, and after win/fail overlay begins.

## 7. Gameplay Rules

### Active Player Behavior

Each level may assign one of two active-player pre-input behaviors:

- Auto-rotate: the active player slowly sweeps aim left and right through a defined angle range.
- Lateral move: the active player moves left and right along a short lane while aim remains generally forward.

For the vertical slice:

- Level 1 uses slow auto-rotation only.
- Level 2 uses auto-rotation with wider angles.
- Level 3 uses lateral movement plus tighter timing.

On hold:

- Current movement/rotation freezes immediately.
- Aim arrow snaps to a crisp, high-contrast presentation.
- Power charge starts from 0 percent and fills toward 100 percent.
- Dashed path preview updates if the implementation supports it.

### Aim Direction

- Aim direction is determined by the active player's current forward direction when hold begins.
- The player does not drag to steer during the vertical slice.
- Designers may tune sweep speed and angle per level.

### Power Charge

- Power charges while the input is held.
- Prototype recommended charge time: 0.8 seconds from 0 to 100 percent.
- If held beyond full charge, power should either stay full or pulse at full. Do not overcharge to fail in this slice.
- Recommended usable range:
  - 0 to 30 percent: short pass, likely stops early.
  - 31 to 70 percent: medium pass.
  - 71 to 100 percent: long pass.
- UI should show a percentage or fill indicator during hold.

### Ball Travel

- Ball starts attached at the active friendly player's feet.
- On release, ball detaches and travels forward along the aim direction.
- Ball may travel as a flat ground pass or slightly lifted pass, but must remain readable from the 70-degree camera.
- Ball should visibly rotate or trail subtly while moving.
- Ball stops when:
  - It contacts a valid friendly receiver.
  - It contacts an opponent.
  - It leaves the playable field bounds.
  - Its travel distance or velocity ends before a valid receiver.

### Pass Success

A pass succeeds when:

- The ball contacts a friendly player who is not the current active passer.
- The receiver is reachable under level rules.
- The receiver is not blocked by an opponent collision first.

On success:

- Ball attaches to receiver feet.
- Receiver becomes active.
- Previous active ring is removed.
- New active player gains green active ring.
- A short positive feedback effect plays.
- If receiver is #10, trigger auto-shot instead of waiting for more input.

### Pass Failure

A pass fails when:

- Ball contacts an opponent.
- Ball exits field bounds.
- Ball stops before contacting a teammate.
- Ball misses all valid friendly receivers.
- Ball contacts environmental blockers if any are added later.

On failure:

- Freeze or slow the failed ball moment.
- Show clear fail overlay.
- Offer Retry and Menu.
- Restart should reload the current level quickly.

### Final #10 Auto-Shot

When #10 receives the ball:

- #10 glows yellow and faces the goal.
- Ball performs a brief auto-shot into the goal.
- Opponents no longer intercept after #10 contact in the vertical slice.
- Goal net or goal mouth should provide visible hit feedback.
- Win overlay appears after the shot completes.

## 8. Entities

### Friendly Player

- Blocky cartoon humanoid.
- Generic blue and white kit.
- Friendly players may use visible jersey numbers.
- Normal friendly players can receive passes and become active.
- #10 is the final target and should be visually distinct.

Required friendly visual states:

- Idle friendly: blue and white kit, no ring.
- Active friendly: green ring at feet, ball attached, aim arrow visible.
- Target #10: yellow glow or aura, #10 number visible, positioned near goal.
- Successful receiver: brief green flash or pop.

### Opponent

- Blocky cartoon humanoid.
- Red kit.
- Stands as an obstacle/interceptor.
- Opponent may be static for the vertical slice.

Required opponent visual states:

- Idle opponent: red ring at feet.
- Hit opponent: red flash, failure feedback.

### Ball

- Generic black and white football or simplified stylized ball.
- Starts at active friendly feet.
- Must be visible against grass at all times.
- Should not use official ball patterns or tournament branding.

### Goal

- Generic football goal with net.
- Used in all three levels.
- #10 is positioned near the attacking goal.
- Auto-shot should visibly enter the goal.

### Field and Stadium

- Green pitch with simple striping and white lines.
- Stadium seating or perimeter boards in the background to create scale.
- Do not place gameplay-critical information under heavy shadows.

## 9. Win and Fail Conditions

### Win

Win condition:

- Ball reaches #10, then #10 auto-shoots successfully into the goal.

Win overlay should show:

- "LEVEL COMPLETE"
- Buttons: NEXT, RETRY, MENU.
- Optional coin reward placeholder.

### Fail

Fail conditions:

- Ball hits red opponent.
- Ball misses friendlies and exits field.
- Ball stops short before reaching a friendly.
- Ball exits playable bounds.

Fail overlay should show:

- "TRY AGAIN"
- Short reason text when feasible:
  - "Opponent intercepted"
  - "Pass missed"
  - "Not enough power"
  - "Out of bounds"
- Buttons: RETRY, MENU.

## 10. Camera and Readability

Gameplay camera:

- Top-down angled camera around 70 degrees.
- Portrait composition.
- Field should run vertically with the goal near the top of the screen.
- Active player usually appears in the lower half.
- Target #10 and goal should remain readable when possible.

Readability requirements:

- Active green ring must be visible under the active player.
- Target #10 yellow glow must not be confused with the active ring.
- Opponent red rings must remain visible and distinct from friendly markers.
- Aim arrow must start near the active player's feet or ball and point in the pass direction.
- Power indicator must not cover the ball, active player, or receiver.
- Dashed path preview should be white or light-colored and contrast against the grass.
- UI panels must not hide the active pass lane.

Recommended camera behavior:

- Static camera per level for the vertical slice.
- Optional subtle camera follow or zoom on auto-shot only.
- Avoid camera shake during aiming; use small feedback only on goal or fail.

## 11. Main Menu Flow

The main menu is a portrait 9:16 stadium celebration screen.

Visual hierarchy:

1. Large Road To Worldcup title near top center.
2. Stadium background with lights, crowd, confetti, and pitch.
3. Central blocky generic #10 holding a generic trophy.
4. Top-right currency placeholders:
   - Coins with plus button.
   - Gems with plus button.
5. Left utility stack:
   - DAILY REWARD
   - SPIN
   - SHOP
   - SETTINGS
6. Primary bottom-center button stack:
   - PLAY, green, largest.
   - TOURNAMENT, blue.
   - CUSTOMIZE, yellow/orange.
   - MISSIONS, purple.

Button behavior for vertical slice:

- PLAY starts Level 1.
- TOURNAMENT opens a placeholder "Coming Soon" panel.
- CUSTOMIZE opens a placeholder "Coming Soon" panel.
- MISSIONS opens a placeholder "Coming Soon" panel.
- DAILY REWARD, SPIN, SHOP, SETTINGS may open placeholder panels or be disabled with visible feedback.
- Coin and gem plus buttons may open placeholder panels.

Main menu acceptance:

- Menu fits 9:16 without cropped critical buttons.
- PLAY is the most visually dominant actionable element.
- Title is legible and brand-safe.
- Character does not use real logo, federation badge, or real likeness.
- Top-right currencies are placeholders and do not require economy implementation.

## 12. Gameplay HUD Flow

HUD should be sparse and readable.

Always visible during gameplay:

- Top-left level label: "LEVEL 1", "LEVEL 2", "LEVEL 3".
- Top-right currency placeholder, optional for vertical slice.
- Pause button in top-right or near currency.

Tutorial level UI:

- Goal panel in upper-left:
  - Shows generic #10 icon -> trophy or goal icon.
  - Text: "Pass to #10 to score!"
- How-to-play panel near lower-left:
  - "HOLD to aim"
  - "RELEASE to pass"
  - "Reach #10 to score!"
- Optional SKIP TUTORIAL button lower-right.

During hold:

- Power percentage or fill appears near aim arrow.
- Aim arrow brightens.
- Dashed path preview appears if feasible.

Ball traveling:

- Hide or reduce power UI.
- Keep pass path ghost briefly if useful.

After win/fail:

- Gameplay input disabled.
- Overlay appears centered.
- Background may dim but should leave field result visible.

## 13. Level Designs

Coordinate language:

- Screen bottom is the starting side.
- Screen top is the attacking goal side.
- Left and right are from the player's view.
- Distances are relative and should be tuned in Unity by feel.

### Level 1 - Tutorial: Simple Chain

Goal:

- Teach hold, release, teammate transfer, and final #10 score.

Layout:

- Friendlies: 3 total.
  - F1: starting active player near lower center, jersey #7.
  - F2: receiver near mid-left or mid-center, jersey #8.
  - F3: final #10 near upper-right, just outside penalty area.
- Opponents: 1 total.
  - O1: placed off the direct ideal lane, between F2 and #10 but with generous clearance.
- Goal: top center.

Behavior:

- F1 uses slow auto-rotation with a narrow sweep toward F2.
- F2 uses slow auto-rotation toward #10.
- Opponent is static.

Suggested pass sequence:

- F1 -> F2 -> #10 -> auto-shot.

Difficulty:

- Very forgiving.
- Wide receiver collision radius.
- Slow aim sweep.
- Clear dashed route.

Tutorial text:

- At level start: "Pass to #10 to score!"
- First hold prompt: "Hold to aim"
- First release prompt: "Release to pass"
- After first success: "Good! Pass again."

Acceptance:

- Player can complete level in two passes.
- One opponent is visible but not frustrating.
- #10 yellow glow is visible before the first pass.

### Level 2 - Angled Routes

Goal:

- Teach angled passes and avoiding multiple opponents.

Layout:

- Friendlies: 4 total.
  - F1: starting active player lower center-left, jersey #6.
  - F2: receiver mid-right, jersey #8.
  - F3: receiver upper-mid-left, jersey #9.
  - F4: final #10 upper-right near goal.
- Opponents: 2 total.
  - O1: center lane blocker between F1 and F3.
  - O2: near right lane between F2 and #10, leaving a diagonal pass window through F3.
- Goal: top center.

Behavior:

- F1 uses medium auto-rotation sweep covering F2 and risky center.
- F2 uses medium auto-rotation sweep covering F3.
- F3 uses slow auto-rotation toward #10.
- Opponents are static.

Suggested pass sequence:

- F1 -> F2 -> F3 -> #10 -> auto-shot.

Difficulty:

- Moderate.
- Player must avoid tempting straight lanes blocked by opponents.
- Power matters more than Level 1.

Tutorial text:

- Level start: "Avoid red opponents."
- On opponent fail if reason text supported: "Opponent intercepted"

Acceptance:

- Requires three successful passes before auto-shot.
- Both opponents are meaningful but leave readable safe lanes.
- Angled route is visually understandable from the initial camera.

### Level 3 - Timing Challenge

Goal:

- Add timing pressure with lateral movement and tighter pass windows.

Layout:

- Friendlies: 5 total.
  - F1: starting active player lower center, jersey #5.
  - F2: receiver lower-mid right, jersey #7.
  - F3: receiver mid-left, jersey #8.
  - F4: receiver upper-mid center, jersey #9.
  - F5: final #10 upper-right or upper-center near goal.
- Opponents: 3 total.
  - O1: lower-mid central obstacle between F1 and F3.
  - O2: mid-right obstacle narrowing F2 -> F4.
  - O3: upper-left or upper-center obstacle narrowing F4 -> #10.
- Goal: top center.

Behavior:

- F1 moves laterally along a short lane before input.
- F2 uses auto-rotation with a moderate sweep.
- F3 may use lateral movement or slow auto-rotation depending on implementation bandwidth.
- F4 uses narrow auto-rotation toward #10.
- Opponents are static for vertical slice. Moving opponents are out of scope unless all required slice work is complete.

Suggested pass sequence:

- F1 -> F2 -> F3 -> F4 -> #10 -> auto-shot.

Difficulty:

- Highest of the slice.
- Pass windows are narrower.
- Timing the hold matters because active position or aim changes before input.
- Still solvable without pixel-perfect precision.

Tutorial text:

- Level start: "Time your hold."
- Optional after first fail: "Hold when the lane is clear."

Acceptance:

- Requires four successful passes before auto-shot.
- Lateral movement visibly changes the viable pass lane.
- At least one pass should fail if released with clearly poor timing or too little power.

## 14. Tutorial Text Library

Use short, high-contrast text. Avoid paragraphs in gameplay.

Primary tutorial strings:

- "Pass to #10 to score!"
- "Hold to aim"
- "Release to pass"
- "Reach #10 to score!"
- "Avoid red opponents"
- "Time your hold"
- "Good! Pass again."

Overlay strings:

- "LEVEL COMPLETE"
- "TRY AGAIN"
- "Opponent intercepted"
- "Pass missed"
- "Not enough power"
- "Out of bounds"

Menu placeholder strings:

- "Coming Soon"
- "Rewards unlock later"
- "Customize unlocks later"

## 15. Art Direction

Style:

- 3D cartoon, blocky, toy-like football characters.
- Bright saturated mobile palette with clean silhouettes.
- Friendly, celebratory, readable over realism.

Main menu:

- Stadium celebration mood.
- Big title with bold dimensional lettering.
- Confetti and lights can add energy but should not reduce button readability.
- Central generic #10 holding a generic trophy.
- Avoid real crests. Use blank shirt, simple stripes, or fictional badge shapes only.

Gameplay:

- Field: bright green with alternating stripe bands and white markings.
- Friendlies: blue and white kits.
- Opponents: red kits.
- Active ring: green.
- Target #10 glow: yellow.
- Opponent rings: red.
- Aim arrow: white with subtle green accent if needed.
- Dashed pass preview: white dashed curve or line.
- Power: readable fill, percentage, or bar near the arrow.

Visual priority order:

1. Ball.
2. Active player and aim arrow.
3. Target receiver or #10.
4. Opponents in the lane.
5. HUD.
6. Stadium detail.

## 16. Optional Audio and Feedback

Audio is optional for the vertical slice, but recommended if time allows.

Suggested sounds:

- Button click.
- Hold start soft tick.
- Power charge rising tone.
- Pass kick.
- Successful receive pop.
- Opponent intercept thud.
- Out-of-bounds whistle.
- Goal kick and crowd cheer.
- Win sting.
- Fail sting.

Suggested visual feedback:

- Active ring pulse while waiting for input.
- Aim arrow brightens on hold.
- Power fill pulses at 100 percent.
- Receiver ring flash on success.
- Red flash on opponent hit.
- Small confetti burst on goal.

Do not let feedback delay restart or next-level flow longer than necessary.

## 17. Out of Scope for Vertical Slice

The following are not required for this slice:

- Real tournament bracket.
- Real team selection.
- Economy, purchases, rewarded ads, or inventory.
- Character customization implementation.
- Missions implementation.
- Moving opponents unless all required features are complete.
- Advanced physics simulation.
- Goalkeeper AI.
- Multiple camera angles.
- Online features.
- Licensed branding.

Placeholder menu panels are acceptable for non-PLAY buttons.

## 18. Implementation Acceptance Criteria

### Gameplay

- Ball begins attached to first friendly at level start.
- Holding input freezes active pre-input movement or rotation.
- Holding displays clear aim and power feedback.
- Releasing sends the ball in the aim direction.
- Friendly contact transfers ball and active state.
- Opponent contact fails the level.
- Out-of-bounds fails the level.
- Stopping short before a teammate fails the level.
- #10 contact triggers auto-shot and win.
- Input is ignored while ball is traveling and after win/fail.

### UI

- Main menu supports portrait 9:16.
- Main menu includes title, central #10 trophy character, PLAY, TOURNAMENT, CUSTOMIZE, MISSIONS, left utility buttons, and top-right currency placeholders.
- PLAY starts Level 1.
- Non-PLAY buttons show placeholder feedback or placeholder panels.
- Gameplay HUD includes level label and pause.
- Win overlay includes NEXT, RETRY, MENU.
- Fail overlay includes RETRY, MENU and optional fail reason.

### Levels

- Level 1 contains 3 friendlies, 1 opponent, and a simple path to #10.
- Level 2 contains 4 friendlies, 2 opponents, and angled routes.
- Level 3 contains 5 friendlies, 3 opponents, and at least one timing challenge through lateral movement or tighter aim sweep.
- Each level can be completed through a clear intended pass sequence.
- Each level can fail through at least one readable mistake.

### Art and Readability

- Friendly team is generic blue-white.
- Opponents are red.
- Active player has green ring.
- #10 has yellow glow.
- Opponents have red rings.
- Ball remains visible on the grass.
- Aim arrow is readable on mobile.
- Dashed pass path is shown where feasible; if omitted, aim arrow and power must still be clear enough to play.
- No restricted real-world branding appears.

### Camera

- Gameplay camera uses an angled top-down view around 70 degrees.
- Level framing keeps active player, useful receiver, and major obstacle readable.
- UI does not block the active pass lane.
- Main menu and gameplay are designed for portrait, not landscape.

### QA

- Prototype can start from main menu, play Level 1, win, proceed to Level 2, win, proceed to Level 3, and win.
- Retry works after failure on all three levels.
- Menu return works from gameplay overlays.
- Pause opens and closes without breaking input state.
- There are no real names, official badges, official trophies, federation marks, FIFA marks, or official World Cup marks.
- Touch and mouse-equivalent input both work for testing.

## 19. Open Questions and Risks

Open questions:

- Is the Unity project going to be created from scratch in this repository, and which Unity version/render pipeline will be used?
- Will dashed path preview be implemented with physics prediction, simple ray/line projection, or a handcrafted visual approximation?
- Should active pre-input behavior be data-driven per player, per level, or hardcoded for the prototype?
- Should Level 3 include moving opponents later, or remain focused on active-player timing?

Risks:

- If the camera is too low, players may not read lanes and rings clearly on portrait mobile.
- If power tuning is too strict, the prototype may feel frustrating rather than hybrid-casual.
- If UI panels copy the reference too closely, brand and originality concerns increase; use the reference only for hierarchy and mood.
- If non-PLAY menu buttons are left inert without feedback, the menu may feel broken. Use simple placeholder panels.

## 20. Recommended Next Worker Use

The Unity Architect should convert this GDD into:

- Scene list and scene flow.
- Script and prefab responsibility map.
- Level data contract for the three layouts.
- UI prefab/screen plan.
- Camera plan for portrait 9:16 gameplay.
- Brand-safe art placeholder plan.
