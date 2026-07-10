# Unity 6 Playable Audit - 2026-07-10

## Summary

The project is on Unity `6000.4.10f1`, opens through Unity Hub, compiles in the editor, and has a playable portrait slice. The main menu is close to the supplied visual direction. Gameplay is functional but still needs feel, mobile layout, performance, and TestFlight readiness passes before external testing.

## Completed Read-Only Audits

- TASK-012 Camera / Game Feel: charge timing is too slow, dashed preview is currently disabled, camera snaps/follows too mechanically, touch cancellation can launch passes, and the #10 climax needs tighter pacing.
- TASK-013 Mobile UI / UX: safe-area handling is missing, several hit targets are below mobile minimums, the Coming Soon modal does not block background taps, current-level text overlaps Play, and gameplay HUD text lacks readable backing treatment.
- TASK-014 Unity 6 Build / Performance: Unity 6 migration compiles, but TestFlight is blocked by Xcode toolchain selection, app identity/signing, missing iOS player build proof, runtime shader lookup risk, and high primitive/collider count at late levels.

## Immediate Implementation Queue

1. TASK-016: gameplay feel pass in `RoadToWorldcupGame.cs`.
2. TASK-017: safe-area, modal blocking, and Play button hit repair in `GeneratedMenuController.cs`.
3. Runtime performance task for unused colliders, shadow settings, and cached ring geometry.
4. iOS/TestFlight task after the user provides bundle ID, Apple Team ID, signing preference, and app icons.

## Validation Needed

- Reload Unity scripts after TASK-015 and TASK-016.
- Run Gameplay in mobile portrait ratio.
- Complete a Level 1 pass chain to #10 and verify victory.
- Check fresh `Editor.log` lines for zero new ParticleSystem duration warnings. After TASK-016 reload, no fresh ParticleSystem duration warning appeared after log line 1657, but no victory replay was completed through Computer Use.

## Live Check Addendum

- Main menu controls are visible again after TASK-017 third follow-up.
- Play button enters Gameplay in Unity Game View.
- Level 1 can be selected through debug input.
- TASK-016 charge/aim preview is visible and responsive in Play Mode.
- Release launches a pass and the fail overlay appears correctly on a missed pass.
- Fresh log after line 1807 contains no compile errors, runtime exceptions, or fresh ParticleSystem duration warnings.
- Full #10 victory chain remains to be verified with accurate manual input.
- Later, test at 375x667, 390x844, 393x852, 430x932, and 440x956 point-equivalent portrait layouts.
