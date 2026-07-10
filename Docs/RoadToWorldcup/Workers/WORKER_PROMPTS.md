# Road To Worldcup - Worker Prompts

## TASK-001 - Product Designer Creates GDD

Copy this complete prompt into a separate Codex worker session.

```text
You are the PRODUCT DESIGNER WORKER for the Unity mobile hybrid-casual game project Road To Worldcup.

PROJECT ROOT:
/Users/mrk/RoadtoWorldCup

WORKER ROLE:
Design the game rules, user flow, level flow, UI flow, and Game Design Document for a playable vertical slice.

ROLE LIMIT:
You are a documentation-only worker for this task.
Do not write code.
Do not create Unity scenes.
Do not create prefabs.
Do not edit C# scripts.
Do not modify implementation folders.

FILES TO READ:
1. /Users/mrk/.codex/attachments/c4cc5ceb-28c2-4031-9ed1-9ec195c3f63f/pasted-text.txt
2. /Users/mrk/RoadtoWorldCup/Docs/RoadToWorldcup/ChiefOfStaff/STATUS.md
3. /Users/mrk/RoadtoWorldCup/Docs/RoadToWorldcup/ChiefOfStaff/MASTER_PLAN.md
4. /Users/mrk/RoadtoWorldCup/Docs/RoadToWorldcup/Workers/01_ProductDesigner/BRIEF.md
5. /Users/mrk/RoadtoWorldCup/Docs/RoadToWorldcup/Workers/01_ProductDesigner/TASKS.md
6. /Users/mrk/RoadtoWorldCup/Docs/RoadToWorldcup/References/main_menu_reference.png.png
7. /Users/mrk/RoadtoWorldCup/Docs/RoadToWorldcup/References/References:gameplay_reference.png.png

ALLOWED FILES TO EDIT:
1. /Users/mrk/RoadtoWorldCup/Docs/RoadToWorldcup/GDD.md
2. /Users/mrk/RoadtoWorldCup/Docs/RoadToWorldcup/Workers/01_ProductDesigner/OUTBOX.md
3. /Users/mrk/RoadtoWorldCup/Docs/RoadToWorldcup/Workers/01_ProductDesigner/STATUS.md

FORBIDDEN FILES/FOLDERS:
1. /Users/mrk/RoadtoWorldCup/Assets/
2. /Users/mrk/RoadtoWorldCup/Packages/
3. /Users/mrk/RoadtoWorldCup/ProjectSettings/
4. /Users/mrk/RoadtoWorldCup/Docs/RoadToWorldcup/Workers/02_UnityArchitect/
5. /Users/mrk/RoadtoWorldCup/Docs/RoadToWorldcup/Workers/03_GameplayProgrammer/
6. /Users/mrk/RoadtoWorldCup/Docs/RoadToWorldcup/Workers/04_LevelDesigner/
7. /Users/mrk/RoadtoWorldCup/Docs/RoadToWorldcup/Workers/05_ArtPlaceholderBuilder/
8. /Users/mrk/RoadtoWorldCup/Docs/RoadToWorldcup/Workers/06_UIUXBuilder/
9. /Users/mrk/RoadtoWorldCup/Docs/RoadToWorldcup/Workers/07_CameraGameFeel/
10. /Users/mrk/RoadtoWorldCup/Docs/RoadToWorldcup/Workers/08_QAReviewer/

TASK OBJECTIVE:
Create /Users/mrk/RoadtoWorldCup/Docs/RoadToWorldcup/GDD.md for the Road To Worldcup vertical slice. The GDD must give implementation workers clear, unambiguous guidance for a playable portrait mobile football passing prototype.

DESIGN REQUIREMENTS:
1. Main Menu screen:
   - Portrait 9:16 layout.
   - ROAD TO WORLDCUP title.
   - Stadium background mood.
   - Generic blocky football star #10 holding a generic trophy.
   - Buttons: PLAY, TOURNAMENT, CUSTOMIZE, MISSIONS.
   - Left buttons: DAILY REWARD, SPIN, SHOP, SETTINGS.
   - Top-right coins and gems placeholders.
2. Gameplay screen:
   - Top-down camera around 70 degrees.
   - Football field and stadium feeling.
   - Friendly players in generic blue/white kit.
   - Opponents in red.
   - Active player green ring.
   - Target #10 yellow glow.
   - Opponents red ring.
   - Ball at active player feet.
   - Aim arrow, power charge, dashed pass path where feasible.
3. Core mechanic:
   - Ball starts at first friendly player.
   - Active player can auto-rotate or move left-right before input.
   - Holding input stops movement/rotation, clarifies aim, and charges power.
   - Releasing input sends ball in the current aim direction.
   - Reaching teammate succeeds and makes that teammate active.
   - Missing, hitting opponent, leaving field, or stopping short fails level.
   - Reaching final friendly player #10 triggers auto-shot and win.
4. Levels:
   - Level 1 tutorial with 3 friendly players, 1 opponent, simple path, #10 near goal.
   - Level 2 with 4 friendly players, 2 opponents, angled routes.
   - Level 3 with 5 friendly players, 3 opponents, timing challenge.
5. Brand safety:
   - Do not use real player likenesses.
   - Do not use real federation logos.
   - Do not use FIFA branding.
   - Do not use official World Cup logos.
   - Use generic tournament, trophy, and football-star language.

STEP-BY-STEP PLAN:
1. Read all required files.
2. Inspect the two reference images enough to describe intended UI hierarchy and gameplay readability.
3. Create GDD.md with these sections:
   - One-page vision.
   - Target platform and orientation.
   - Audience and feel.
   - Core loop.
   - Controls.
   - Gameplay rules.
   - Player, opponent, ball, goal, and level entities.
   - Win/fail conditions.
   - Camera and readability requirements.
   - Main menu flow.
   - Gameplay HUD flow.
   - Level designs for Level 1, Level 2, Level 3.
   - Tutorial text.
   - Art direction.
   - Audio and feedback direction, even if optional for prototype.
   - Implementation acceptance criteria for workers.
   - Open questions and risks.
4. Update OUTBOX.md with a concise completion report.
5. Update STATUS.md with task status, changed files, and any blockers.

ACCEPTANCE CRITERIA:
1. GDD.md exists and is detailed enough for Unity Architect, Gameplay Programmer, UI/UX Builder, Art Placeholder Builder, Level Designer, Camera/Game Feel, and QA workers.
2. GDD avoids restricted branding, real likenesses, real federation logos, FIFA branding, and official World Cup logos.
3. GDD defines exact vertical slice scope and excludes nonessential systems from the first prototype.
4. GDD includes all three level examples with clear layout intent and success/fail requirements.
5. GDD includes concrete acceptance criteria for gameplay, UI, level, art, camera, and QA workers.
6. OUTBOX.md and STATUS.md are updated.

REQUIRED FINAL REPORT FORMAT:
When finished, respond with:
1. Status: Complete or Blocked.
2. Files changed.
3. Summary of GDD decisions.
4. Open questions or blockers.
5. Suggested next worker task.

IMPORTANT:
After completing your work, make sure /Users/mrk/RoadtoWorldCup/Docs/RoadToWorldcup/Workers/01_ProductDesigner/OUTBOX.md and /Users/mrk/RoadtoWorldCup/Docs/RoadToWorldcup/Workers/01_ProductDesigner/STATUS.md are updated. Then return control to the Chief of Staff session for review.
```

## TASK-015 - Fix Unity 6 Victory Particle Warning

```text
PROJECT ROOT: /Users/mrk/RoadtoWorldCup

WORKER ROLE: Gameplay Programmer.

FILES TO READ: `Assets/_RoadToWorldcup/Scripts/Gameplay/RoadToWorldcupGame.cs`, the current Unity `Editor.log`, and Gameplay Programmer `OUTBOX.md`/`STATUS.md`.

ALLOWED EDITS: `RoadToWorldcupGame.cs`, `Docs/RoadToWorldcup/Workers/03_GameplayProgrammer/OUTBOX.md`, and `STATUS.md` only.

FORBIDDEN EDITS: every other implementation file, scene, prefab, package, ProjectSettings file, task queue, and Chief of Staff document.

OBJECTIVE: remove the Unity 6000.4.10f1 warning `Setting the duration while system is still playing is not supported` raised by `CreateBurstParticles()` during Win(), while preserving the four one-shot victory bursts.

PLAN: inspect the warning stack; stop and clear each new ParticleSystem before setting runtime module properties; disable play-on-awake if appropriate; preserve `Emit(count)`; keep the diff minimal; update worker reports.

ACCEPTANCE: no warning from `CreateBurstParticles`, no compile error, and victory bursts still emit once.

FINAL REPORT: status, files changed, root cause, exact fix, verification, remaining limitation. Update `OUTBOX.md` and `STATUS.md` before returning control.

IMPORTANT: other agents are active. Do not revert unrelated changes and do not edit outside your ownership.
```

## TASK-016 - Gameplay Feel Pass 1

```text
PROJECT ROOT: /Users/mrk/RoadtoWorldCup

WORKER ROLE: Camera / Game Feel Worker acting as Gameplay Programmer for one bounded patch.

YOU ARE NOT ALONE IN THE CODEBASE:
Other workers and the Chief of Staff may have recent edits. Do not revert unrelated changes. Read the current files before editing and adapt to the current state.

FILES TO READ:
1. Assets/_RoadToWorldcup/Scripts/Gameplay/RoadToWorldcupGame.cs
2. Docs/RoadToWorldcup/Workers/03_GameplayProgrammer/OUTBOX.md
3. Docs/RoadToWorldcup/Workers/03_GameplayProgrammer/STATUS.md
4. Docs/RoadToWorldcup/References/References:gameplay_reference.png.png

ALLOWED EDITS:
1. Assets/_RoadToWorldcup/Scripts/Gameplay/RoadToWorldcupGame.cs
2. Docs/RoadToWorldcup/Workers/03_GameplayProgrammer/OUTBOX.md
3. Docs/RoadToWorldcup/Workers/03_GameplayProgrammer/STATUS.md

FORBIDDEN EDITS:
Every other implementation file, scene, prefab, package, ProjectSettings file, TaskQueue document, and Chief of Staff document.

OBJECTIVE:
Improve the mobile gameplay feel without changing core rules or adding new systems. Prioritize visible playability in portrait Play Mode.

REQUIRED FIXES:
1. Make power charge feel responsive. Current charge is too slow because it divides by `level.chargeTime * 2f`. Target useful/full charge in roughly 0.75-0.95 seconds for early levels and slightly faster on late levels. Keep deterministic behavior and avoid making levels impossible.
2. Implement the dashed aiming/pass preview instead of hiding it. It should show short white dash segments from the active player toward the intended pass direction while aiming, with an endpoint close to the predicted travel direction/range. Do not create per-frame garbage-heavy objects.
3. Treat `TouchPhase.Canceled` as an abort, not as a release/pass. If a touch is canceled or the app loses focus/pause during aiming, cancel aiming visuals and do not launch.
4. Smooth the camera behavior when the ball leaves a player. Preserve the user's requirement that the camera follows the ball during travel, but remove harsh release snapping and add light look-ahead toward the target/receiver where the current data allows it.
5. Preserve the current rule that receiving teammates do not turn into a green state/highlight. Only the active possession marker should stay visually green.
6. Preserve #10 auto-shot, goalkeeper miss, win overlay timing, and the TASK-015 ParticleSystem warning fix.

ACCEPTANCE:
- C# compiles under Unity 6000.4.10f1.
- Aiming preview is visible while holding/dragging.
- Releasing still launches passes and #10 still auto-shoots.
- Canceled touch/focus loss does not launch a pass.
- Camera follows the ball smoothly while it travels and does not visibly snap on release.
- No edits outside the allowed files.

FINAL REPORT:
Update OUTBOX.md and STATUS.md with status, files changed, exact gameplay changes, compile/check performed, and risks. Then return control to Chief of Staff.
```

## TASK-017 - Menu Input and Safe-Area Repair

```text
PROJECT ROOT: /Users/mrk/RoadtoWorldCup

WORKER ROLE: UI / UX Builder.

YOU ARE NOT ALONE IN THE CODEBASE:
Other workers and the Chief of Staff may have recent edits. Do not revert unrelated changes. Read the current files before editing and adapt to the current state.

FILES TO READ:
1. Assets/_RoadToWorldcup/Scripts/UI/GeneratedMenuController.cs
2. Assets/_RoadToWorldcup/Scripts/Core/RuntimeSceneBootstrap.cs
3. Docs/RoadToWorldcup/Workers/06_UIUXBuilder/OUTBOX.md
4. Docs/RoadToWorldcup/Workers/06_UIUXBuilder/STATUS.md
5. Docs/RoadToWorldcup/References/main_menu_reference.png.png

ALLOWED EDITS:
1. Assets/_RoadToWorldcup/Scripts/UI/GeneratedMenuController.cs
2. Docs/RoadToWorldcup/Workers/06_UIUXBuilder/OUTBOX.md
3. Docs/RoadToWorldcup/Workers/06_UIUXBuilder/STATUS.md

FORBIDDEN EDITS:
Every other implementation file, gameplay script, scene, prefab, package, ProjectSettings file, TaskQueue document, Chief of Staff document, and unrelated worker docs.

OBJECTIVE:
Repair the main menu so it reliably supports mobile portrait touch input and is safe-area aware while keeping the visual close to the provided reference.

REQUIRED FIXES:
1. Fix Play hit reliability. The current `CURRENT LEVEL` label overlaps the Play button and Chief of Staff could not activate Play through repeated Unity Game View clicks after reload. Remove, reposition, or integrate that label so it cannot intercept or visually collide with Play.
2. Make the Coming Soon popup truly modal. Add a full-screen scrim or equivalent raycast blocker so taps outside the card cannot trigger menu buttons behind it. Keep the close/OK target at least mobile-friendly size.
3. Add safe-area-aware content positioning for portrait phones. The background should remain full bleed, but important controls and currencies should live inside a safe root or equivalent inset/scale strategy.
4. Preserve the current reference-style art assets and overall button stack.
5. Do not change gameplay code or scene flow semantics except ensuring Play calls `SceneLoader.LoadGameplay()` reliably.

ACCEPTANCE:
- C# compiles under Unity 6000.4.10f1 or via the current project compiler setup.
- Play button has no overlapping text/raycast target and still calls `SceneLoader.LoadGameplay()`.
- Coming Soon blocks background taps.
- Currency, utility, and bottom action buttons fit within common iPhone portrait safe areas.
- No edits outside the allowed files.

FINAL REPORT:
Update UIUX Builder OUTBOX.md and STATUS.md with status, files changed, exact UI changes, compile/check performed, and risks. Then return control to Chief of Staff.
```
