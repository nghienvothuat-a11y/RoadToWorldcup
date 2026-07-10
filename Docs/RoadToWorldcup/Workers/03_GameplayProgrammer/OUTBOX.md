# Gameplay Programmer Outbox

## TASK-016 Camera / Game Feel Pass

Status: Implementation complete; Unity 6000 isolated compiler check passed. Live portrait Play Mode validation remains for Chief of Staff.

Changed files:

- `Assets/_RoadToWorldcup/Scripts/Gameplay/RoadToWorldcupGame.cs`
- `Docs/RoadToWorldcup/Workers/03_GameplayProgrammer/OUTBOX.md`
- `Docs/RoadToWorldcup/Workers/03_GameplayProgrammer/STATUS.md`

Gameplay changes:

- Power charge now fills from `GetChargeDuration()` instead of `level.chargeTime * 2f`.
- Early levels reach full charge in roughly 0.86 seconds; late levels clamp to roughly 0.62 seconds, preserving the existing level charge tuning while making hold/release feel responsive.
- Added pooled white `LineRenderer` pass dashes during aiming. The dash endpoint lands at the predicted release range using the current charge and lob amount, and the pool is reused instead of creating per-frame preview objects.
- `TouchPhase.Canceled` no longer launches a pass. Canceled touches abort aiming, hide visuals, and clear charge.
- App focus loss, app pause, pause overlay entry, and level-select entry now abort any active aim instead of resuming into a stale charged release.
- Removed the release-time camera snap from `LaunchPass()`.
- Camera still tracks the ball while traveling, but now eases with a softer travel rate and uses light look-ahead toward the pass direction and suggested receiver. Auto-shot camera focus lightly leads toward goal.
- Preserved the current marker rule: only `activeRing` is green; #10 keeps the yellow target ring and receiving teammates are not turned green unless they become active possession.
- Preserved #10 auto-shot, goalkeeper miss dive, win overlay timing, and the TASK-015 ParticleSystem warning fix (`StopEmittingAndClear` plus `playOnAwake = false` before duration setup).

Validation performed:

- Read the required gameplay script, worker OUTBOX/STATUS docs, TASK-016 prompt section, and inspected the portrait gameplay reference image.
- Unity 6000.4.10f1 Roslyn compile passed with no diagnostics:
  - `/Applications/Unity/Hub/Editor/6000.4.10f1/Unity.app/Contents/Resources/Scripting/NetCoreRunTime/dotnet /Applications/Unity/Hub/Editor/6000.4.10f1/Unity.app/Contents/Resources/Scripting/DotNetSdkRoslyn/csc.dll @/Users/mrk/RoadtoWorldCup/Library/Bee/artifacts/900b0aE.dag/Assembly-CSharp.rsp /out:/tmp/RoadToWorldcup_TASK016_Assembly-CSharp.dll /target:library`
- Static checks confirmed canceled touch is split from release, `LaunchPass()` no longer calls `SnapCameraToBall()`, and the ParticleSystem fix remains in place.

Risks / follow-up:

- Live mobile portrait Play Mode was not run in this worker pass, so Chief of Staff should verify dash readability, charge feel, canceled-touch behavior, and camera smoothness in-editor/on-device.
- The local workspace had other recent file changes outside this worker's allowed files before/during validation; they were not edited or reverted by this worker.

## TASK-015 Unity 6 Victory VFX Warning Fix

Status: Implementation complete; isolated Unity 6 and Unity 2020 compiler checks passed. Live Play Mode validation remains for Chief of Staff.

Changed files:

- `Assets/_RoadToWorldcup/Scripts/Gameplay/RoadToWorldcupGame.cs`
- `Docs/RoadToWorldcup/Workers/03_GameplayProgrammer/OUTBOX.md`
- `Docs/RoadToWorldcup/Workers/03_GameplayProgrammer/STATUS.md`

Root cause:

- `AddComponent<ParticleSystem>()` left each newly created victory system playing with its default play-on-awake behavior.
- `CreateBurstParticles` then assigned `main.duration` while the system was playing, which Unity 6000.4.10f1 rejects. The helper is called four times by `StartWorldCelebrationVfx`, so each `Win()` produced one warning per burst.

Implemented fix:

- Immediately stop and clear the new particle system with `Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear)` before reading or configuring its runtime modules.
- Set `main.playOnAwake = false` before assigning `main.duration`.
- Retained all existing particle settings, all four victory burst call sites, disabled continuous emission, and the final one-shot `particles.Emit(count)` behavior.

Validation performed:

- Confirmed the exact warning stack in `/Users/mrk/Library/Logs/Unity/Editor.log`: `CreateBurstParticles` at the old `main.duration` assignment, reached from all four `StartWorldCelebrationVfx` calls during `Win()`.
- Static inspection confirms stop-and-clear now precedes all module configuration, and the four burst calls plus one final `Emit(count)` remain unchanged.
- Unity 6000.4.10f1 Roslyn compile passed with no diagnostics using the current editor-generated `Assembly-CSharp.rsp`; outputs were redirected to `/tmp`.
- Unity 2020.3.2f1 compatibility compile passed with no diagnostics using that editor's compiler and engine/UI API assemblies; output was redirected to `/tmp`.
- No second Unity instance was launched and no project settings, scenes, prefabs, packages, or unrelated implementation files were modified.

Known limitation:

- Live post-patch Play Mode was not run. Unity was already open and executing the pre-patch assembly, so its current Editor log still contains warnings from that older code. Chief of Staff should exit Play Mode, allow script reload, trigger one win, and confirm four visible one-shot bursts with no new duration warning.

## TASK-010 Runtime Vertical Slice

Status: Complete for implementation; Unity play validation blocked by local license activation.

Changed files:

- `Assets/_RoadToWorldcup/Scripts/Core/GameSession.cs`
- `Assets/_RoadToWorldcup/Scripts/Core/SceneLoader.cs`
- `Assets/_RoadToWorldcup/Scripts/Core/RuntimeSceneBootstrap.cs`
- `Assets/_RoadToWorldcup/Scripts/UI/GeneratedMenuController.cs`
- `Assets/_RoadToWorldcup/Scripts/Gameplay/RoadToWorldcupGame.cs`
- `Docs/RoadToWorldcup/Workers/03_GameplayProgrammer/STATUS.md`
- `Docs/RoadToWorldcup/Workers/03_GameplayProgrammer/OUTBOX.md`

Editor build settings:

- Reviewed `ProjectSettings/EditorBuildSettings.asset`.
- Bootstrap, MainMenu, and Gameplay are already included and enabled in that order.
- No build settings edit was needed.

Implemented player-facing output:

- Main Menu:
  - Runtime-generated portrait UI.
  - Title: `ROAD TO WORLDCUP`.
  - Simple stadium/field background and generic #10 hero character.
  - Buttons: PLAY, TOURNAMENT, CUSTOMIZE, MISSIONS.
  - Left utility buttons: DAILY, SPIN, SHOP, SETTINGS.
  - Top-right coin/gem placeholders.
  - PLAY selects Level 1 and loads Gameplay.
  - Non-PLAY buttons show a Coming Soon panel.

- Gameplay:
  - Runtime-generated green field, goal, crowd blocks, fictional boards, and camera/light setup.
  - Generic blue/white friendlies, red opponents, ball, active green ring, #10 yellow ring, opponent red rings.
  - Aim arrow and dashed path preview.
  - Hold input freezes active behavior and charges power.
  - Release launches a deterministic scripted pass.
  - Next valid friendly receives and becomes active.
  - Opponent hit, out of bounds, invalid/missed receiver, or stopped-short pass fails.
  - #10 receive triggers an auto-shot into the goal and win overlay.
  - Retry, Menu, Pause/Resume, and Next flows are implemented.

- Levels:
  - Level 1: 3 friendlies, 1 opponent, tutorial chain to #10.
  - Level 2: 4 friendlies, 2 opponents, angled pass route.
  - Level 3: 5 friendlies, 3 opponents, first-player lateral timing challenge.

Validation performed:

- Static scans confirmed:
  - Expected runtime classes exist.
  - No `UnityEditor`, TextMeshPro, new Input System, or modern C# syntax references were found.
  - Scene loading, level creation, retry/next/menu, input, Canvas/Button/Text, and LineRenderer references are present.
- Unity batchmode command attempted:
  - `/Applications/Unity/Unity.app/Contents/MacOS/Unity -batchmode -projectPath /Users/mrk/RoadtoWorldCup -quit -logFile /tmp/RoadToWorldcup_UnityBatch.log`
  - Result: failed before compile/import due license activation.
  - Relevant log line: `Failed to activate/update license Missing or bad username or password.`

Known limitations:

- Unity compile and playthrough are not confirmed because local license activation blocks batchmode.
- Runtime visuals are intentionally placeholder/blocky and generated in code.
- Level tuning is first-pass; receiver/opponent radii and aim sweeps may need playtest adjustment after editor access is restored.
- `.meta` files for new scripts were not generated because Unity did not complete project import.

Recommended next task:

- Resolve Unity license activation, open the project in Unity 2020.3.2f1, let script `.meta` files import, then perform a MainMenu -> Level 1 -> Level 2 -> Level 3 playthrough tuning pass.
