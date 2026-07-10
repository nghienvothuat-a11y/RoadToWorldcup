# Road To Worldcup - In Progress

## TASK-011 - Gameplay Logic and Regression Audit

- Worker owner: QA Reviewer / Gameplay Logic Auditor.
- Priority: P0.
- Status: INTERRUPTED.
- Dependency: TASK-010 complete and Unity 6000.4.10f1 Play Mode available.
- Input files: `RoadToWorldcupGame.cs`, Core scripts, scenes/build settings, Unity editor logs, and current gameplay requirements.
- Output: not produced; the sub-agent timed out and was closed by Chief of Staff.
- Acceptance criteria: concrete P0-P3 findings with reproduction, exact file/line anchors, recommended fix, and a minimal regression checklist.
- Review method: Chief of Staff cross-checks findings against source and live Play Mode.
- Definition of done: not reached. Reopen as a fresh QA task after the next playable test pass.

## TASK-012 - Camera and Game Feel Audit

- Worker owner: Camera / Game Feel.
- Priority: P1.
- Status: COMPLETE.
- Dependency: TASK-010 complete.
- Input files: gameplay implementation and `References:gameplay_reference.png.png`.
- Output: ranked sub-agent implementation brief; Chief of Staff review will be recorded under `Docs/RoadToWorldcup/Reports/`.
- Acceptance criteria: recommended timing/curve ranges, exact likely file ownership, and measurable acceptance criteria for aiming, ball motion, camera, animation, goalkeeper, goal climax, and transitions.
- Review method: compare recommendations with the reference and live portrait Game View.
- Definition of done: highest-value improvements can be assigned without overlapping write scopes.

## TASK-013 - Mobile UI/UX Audit

- Worker owner: UI / UX Builder.
- Priority: P1.
- Status: COMPLETE.
- Dependency: TASK-010 complete and menu UI resources available.
- Input files: `GeneratedMenuController.cs`, menu resources, scenes, portrait settings, and both reference images.
- Output: prioritized sub-agent report and iPhone aspect-ratio test matrix.
- Acceptance criteria: safe-area, touch-target, hierarchy, layout, readability, and scene-flow findings include concrete dimensions/constraints and file anchors.
- Review method: inspect 9:16 Game View and verify implementation feasibility in the current runtime-generated UI.
- Definition of done: UI fixes are separated into blockers, high-value polish, and later art production.

## TASK-014 - Unity 6 Build and Performance Audit

- Worker owner: Unity Architect.
- Priority: P0.
- Status: COMPLETE.
- Dependency: migration to Unity 6000.4.10f1 complete.
- Input files: Packages, ProjectSettings, editor setup, runtime scripts, and Unity logs.
- Output: prioritized sub-agent technical-readiness report.
- Acceptance criteria: exact evidence for migration issues, lifecycle/input risks, mobile performance risks, and iOS build blockers; each finding includes a verification command or test.
- Review method: Chief of Staff checks project settings/logs and separates playable-slice blockers from TestFlight production work.
- Definition of done: build and performance risks have owners and priorities.

## TASK-015 - Fix Unity 6 Victory Particle Warning

- Worker owner: Gameplay Programmer.
- Priority: P0.
- Status: PARTIAL_LIVE_PASS.
- Dependency: repeatable Unity 6 warning captured in `Editor.log`.
- Input files: `RoadToWorldcupGame.cs`, `Editor.log`, Gameplay Programmer `OUTBOX.md`, and `STATUS.md`.
- Output files: `RoadToWorldcupGame.cs`, Gameplay Programmer `OUTBOX.md`, and `STATUS.md`.
- Locked files: none; lock released after implementation. `RoadToWorldcupGame.cs` is now reserved for TASK-016.
- Acceptance criteria: `CreateBurstParticles()` configures a stopped/cleared ParticleSystem, retains four one-shot victory bursts, creates no compile error, and produces no `Setting the duration while system is still playing` warning.
- Exact worker prompt: recorded in `Docs/RoadToWorldcup/Workers/WORKER_PROMPTS.md` under TASK-015.
- Review method: inspect the minimal diff, recompile in the open Unity editor, trigger victory, and search the fresh log for the warning.
- Definition of done: code and worker reports are updated and live victory validation passes.

## TASK-016 - Gameplay Feel Pass 1

- Worker owner: Camera / Game Feel Worker.
- Priority: P0.
- Status: REVIEW.
- Dependency: TASK-012, TASK-013, and TASK-014 audit results available; TASK-015 implementation submitted.
- Input files: `RoadToWorldcupGame.cs`, Gameplay Programmer `OUTBOX.md`, Gameplay Programmer `STATUS.md`, reference gameplay image, and current user gameplay requirements.
- Output files: `RoadToWorldcupGame.cs`, Gameplay Programmer `OUTBOX.md`, and `STATUS.md`.
- Locked files: none; patch submitted and lock released.
- Scope: faster and more readable power charge, real dashed trajectory preview, touch cancel/focus-loss safety, smoother camera follow while the ball is away from the active player, and no receiver green-state regression.
- Acceptance criteria: aiming reaches useful power in under roughly one second on early levels, dashed preview renders before release and tracks the actual pass direction, canceled touches do not launch a pass, camera follows ball travel without a release snap, receiving teammates do not switch to green highlight, and C# compiles in Unity 6000.4.10f1.
- Exact worker prompt: recorded in `Docs/RoadToWorldcup/Workers/WORKER_PROMPTS.md` under TASK-016.
- Review method: inspect diff, allow Unity assembly reload, run mobile portrait Play Mode, verify at least one pass chain and check fresh log tail.
- Definition of done: code and worker reports are updated, compile succeeds, and Chief of Staff live-checks the improved feel.
- Chief of Staff live result: Unity reloaded with no new compile/runtime warnings after log line 1807; menu Play entered Gameplay; Level 1 debug select worked; charge reached 100% quickly; white aim preview rendered clearly; release launched a pass and produced the expected fail overlay after a deliberately imprecise Computer Use drag. Accurate #10 victory chain remains unverified.

## TASK-017 - Menu Input and Safe-Area Repair

- Worker owner: UI / UX Builder.
- Priority: P0.
- Status: LIVE_PASS.
- Dependency: TASK-013 audit and Chief of Staff menu click test.
- Input files: `GeneratedMenuController.cs`, `RuntimeSceneBootstrap.cs`, menu resources, and the main menu reference.
- Output files: to be assigned before implementation.
- Scope: fix Play button hit reliability/overlap, remove or reposition current-level label, add a full-screen blocking scrim for Coming Soon, and preserve visible reference-menu controls.
- Acceptance criteria: Play reliably loads Gameplay from the Unity Game View and on mobile touch, modal blocks background taps, top/right/left UI respects common iPhone safe areas, and visual hierarchy remains close to the reference.
- Review method: inspect patch, reload Unity, click/tap Play, test Coming Soon background blocking, and capture mobile portrait screenshots.
- Definition of done: menu can reliably enter gameplay and no important controls clip on tested portrait ratios.
- Chief of Staff live result: after follow-up repairs, reference-menu controls are visible again, the overlapping current-level label is gone, and Play reliably loads Gameplay in Unity Game View. Safe-area perfection was deferred because the first safe-root implementation hid the reference controls.
