# Road To Worldcup - Master Plan

## Target

Coordinate production of a playable portrait mobile vertical slice for Road To Worldcup, a hybrid-casual football passing game with blocky cartoon characters, hold-to-aim passing, level layouts, basic win/fail flow, and a menu inspired by the provided references.

## Role Boundary

The Chief of Staff only creates and edits coordination documentation. Implementation files under `Assets/_RoadToWorldcup/` must be created or modified only by assigned worker agents.

## Phase Plan

### Phase 0 - Inspection

Goal: determine project readiness and create coordination system.

Current finding: the repository is docs-only and is not yet a valid Unity project.

Outputs:

- Chief of Staff status files.
- Worker registry.
- Task queue.
- First worker prompt.

### Phase 1 - Design

Workers:

- Product Designer.
- Unity Architect.

Outputs:

- `Docs/RoadToWorldcup/GDD.md`
- `Docs/RoadToWorldcup/TECH_ARCHITECTURE.md`
- Implementation task breakdown.

### Phase 2 - Core Foundation

Workers:

- Gameplay Programmer.
- Art Placeholder Builder.
- UI/UX Builder.

Outputs:

- Basic project folders.
- Core scripts.
- Placeholder prefabs and materials.
- Gameplay and menu scene foundations.

### Phase 3 - Playable Core Loop

Workers:

- Gameplay Programmer.
- Level Designer.
- Camera/Game Feel.

Outputs:

- Hold-to-aim.
- Release-to-pass.
- Pass success/fail.
- Final #10 auto-shot.
- Basic level completion.

### Phase 4 - Menu and UI

Workers:

- UI/UX Builder.
- Art Placeholder Builder.

Outputs:

- Portrait main menu.
- Gameplay HUD.
- Win/lose overlays.
- Currency placeholders.

### Phase 5 - Levels

Workers:

- Level Designer.
- Gameplay Programmer if data integration is needed.

Outputs:

- At least three level layouts.
- Tutorial progression.
- Difficulty ramp.

### Phase 6 - QA

Worker:

- QA Reviewer.

Outputs:

- Compile check.
- Play mode checklist.
- Bug report.
- Regression notes.

### Phase 7 - Final Report

Chief of Staff output:

- `Docs/RoadToWorldcup/ChiefOfStaff/FINAL_REPORT.md`

## Dependency Flow

1. TASK-001 Product Designer creates GDD.
2. TASK-002 Unity Architect creates implementation architecture using GDD.
3. TASK-003 Art Placeholder Builder creates visual asset plan and placeholder asset implementation.
4. TASK-004 Gameplay Programmer creates core gameplay foundation using architecture.
5. TASK-005 UI/UX Builder creates menu and HUD once scene/UI plan is stable.
6. TASK-006 Level Designer creates three level layouts after gameplay data contract exists.
7. TASK-007 Camera/Game Feel improves aiming readability after core pass mechanic exists.
8. TASK-008 QA Reviewer tests the vertical slice after implementation tasks report complete.

## Quality Bar

- Portrait 9:16 is the default layout.
- Gameplay must be readable on mobile.
- All worker outputs must include changed files, validation steps, risks, and next recommendations.
- No worker may edit files outside its assigned allowed paths.
