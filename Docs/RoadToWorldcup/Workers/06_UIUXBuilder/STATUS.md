# UI/UX Builder Status

- Current task: TASK-017 - Menu Input and Safe-Area Repair.
- Status: Complete, third follow-up playable visibility patch applied and returned to Chief of Staff.
- Files changed:
  - `Assets/_RoadToWorldcup/Scripts/UI/GeneratedMenuController.cs`
  - `Docs/RoadToWorldcup/Workers/06_UIUXBuilder/OUTBOX.md`
  - `Docs/RoadToWorldcup/Workers/06_UIUXBuilder/STATUS.md`
- Summary:
  - Reference-art main menu controls now render directly under the canvas root again for reliable visibility after reload, while the reference background remains full bleed and behind controls.
  - Follow-up render-order fix pins the reference background behind canvas-root controls and keeps the modal overlay topmost.
  - Third follow-up bypasses the safe-area/content-root approach for the reference-art path because live reload still hid controls; fallback UI still uses the safe-area root.
  - Removed the `CURRENT LEVEL` label that could visually collide with Play.
  - Play remains wired to `SceneLoader.LoadGameplay()`.
  - Coming Soon is now a modal full-screen overlay with a raycast-blocking scrim and larger OK target.
- Checks:
  - Direct C# compile against Unity 6000.4.10f1 scripting assemblies passed after the third follow-up with only existing obsolete API warnings.
  - Unity batchmode compile/import could not complete because local Unity licensing activation failed before compilation.
- Blockers: None for code handoff. Final Unity Editor reload/touch validation remains recommended.
