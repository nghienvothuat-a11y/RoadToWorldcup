# Road To Worldcup - Backlog

> Historical note (2026-07-10): TASK-002 is complete and TASK-003 through TASK-008 were partially absorbed by the runtime-generated TASK-010 vertical slice. They remain here only as production-quality follow-up scopes until the active audit tasks split them into precise, non-conflicting fixes.

## TASK-002 - Unity Architect Creates Technical Architecture

- Worker owner: Unity Architect.
- Priority: P0.
- Status: BACKLOG.
- Dependency: TASK-001 complete.
- Input files: `Docs/RoadToWorldcup/GDD.md`, Chief of Staff status files, reference images.
- Output files: `Docs/RoadToWorldcup/TECH_ARCHITECTURE.md`, Unity Architect worker reports.
- Acceptance criteria: defines Unity project setup, folder structure, scenes, scripts, prefabs, data model, dependencies, render pipeline recommendation, TextMeshPro plan, and implementation sequencing.

## TASK-003 - Art Placeholder Builder Creates Visual Asset Plan

- Worker owner: Art Placeholder Builder.
- Priority: P1.
- Status: BACKLOG.
- Dependency: TASK-002 complete.
- Input files: `Docs/RoadToWorldcup/GDD.md`, `Docs/RoadToWorldcup/TECH_ARCHITECTURE.md`, reference images.
- Output target: `Assets/_RoadToWorldcup/Prefabs/`, `Assets/_RoadToWorldcup/Materials/`, `Assets/_RoadToWorldcup/ArtPlaceholders/`, worker reports.
- Acceptance criteria: simple blocky football player, opponent, ball, goal, field, stadium placeholders, and generic trophy/menu hero placeholders exist or are precisely specified if project initialization blocks creation.

## TASK-004 - Gameplay Programmer Creates Core Gameplay Foundation

- Worker owner: Gameplay Programmer.
- Priority: P0.
- Status: BACKLOG.
- Dependency: TASK-002 complete and no conflicting file locks.
- Input files: `Docs/RoadToWorldcup/GDD.md`, `Docs/RoadToWorldcup/TECH_ARCHITECTURE.md`.
- Output target: `Assets/_RoadToWorldcup/Scripts/`.
- Acceptance criteria: active player, ball attachment, hold/release input, pass movement, teammate success, opponent/miss fail, #10 win trigger, level state flow.

## TASK-005 - UI/UX Builder Creates Menu and HUD

- Worker owner: UI/UX Builder.
- Priority: P1.
- Status: BACKLOG.
- Dependency: TASK-002 complete, menu/HUD architecture stable.
- Input files: `Docs/RoadToWorldcup/GDD.md`, `Docs/RoadToWorldcup/TECH_ARCHITECTURE.md`, reference images.
- Output target: `Assets/_RoadToWorldcup/UI/`, `Assets/_RoadToWorldcup/Scenes/`.
- Acceptance criteria: portrait main menu, gameplay HUD, top currency placeholders, left utility buttons, core menu buttons, pause/win/lose overlays.

## TASK-006 - Level Designer Creates Three Levels

- Worker owner: Level Designer.
- Priority: P1.
- Status: BACKLOG.
- Dependency: TASK-002 complete and gameplay level data contract defined.
- Input files: `Docs/RoadToWorldcup/GDD.md`, `Docs/RoadToWorldcup/TECH_ARCHITECTURE.md`.
- Output target: `Assets/_RoadToWorldcup/Levels/`.
- Acceptance criteria: Level 1 tutorial, Level 2 angled routes, Level 3 timing challenge with 3-5 friendly players and 1-3 opponents.

## TASK-007 - Camera/Game Feel Improves Readability

- Worker owner: Camera/Game Feel.
- Priority: P2.
- Status: BACKLOG.
- Dependency: TASK-004 core gameplay foundation exists.
- Input files: `Docs/RoadToWorldcup/GDD.md`, `Docs/RoadToWorldcup/TECH_ARCHITECTURE.md`, gameplay scripts/prefabs.
- Output target: `Assets/_RoadToWorldcup/Scripts/`, `Assets/_RoadToWorldcup/Prefabs/`.
- Acceptance criteria: 70-degree gameplay camera, active/target/opponent rings, aim arrow, charge feedback, dashed path preview where feasible, mobile-readable feedback.

## TASK-008 - QA Reviewer Tests Vertical Slice

- Worker owner: QA Reviewer.
- Priority: P0 before final delivery.
- Status: BACKLOG.
- Dependency: TASK-003 through TASK-007 complete.
- Input files: Unity project, all worker reports, GDD, architecture.
- Output files: `Docs/RoadToWorldcup/Reviews/QA_REPORT.md`, QA worker reports.
- Acceptance criteria: compile status, play mode status, scene flow test, win/fail test, bug list with severity, final readiness recommendation.
