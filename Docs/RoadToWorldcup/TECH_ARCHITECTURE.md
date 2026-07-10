# Road To Worldcup - Technical Architecture

Last updated: 2026-07-09  
Owner: Unity Architect Worker  
Target slice: portrait mobile passing prototype

## 1. Current Project State

The repository at `/Users/mrk/RoadtoWorldCup` is not currently a valid Unity project. These required Unity folders are missing:

- `/Users/mrk/RoadtoWorldCup/Assets/`
- `/Users/mrk/RoadtoWorldCup/Packages/`
- `/Users/mrk/RoadtoWorldCup/ProjectSettings/`

Implementation workers must not assume existing scenes, prefabs, scripts, package manifests, render pipeline settings, or TextMeshPro resources. The first implementation task should initialize the Unity project using the installed editor:

- Editor path: `/Applications/Unity/Unity.app`
- Editor version: `2020.3.2f1`
- Recommended template: `3D`
- Primary aspect: portrait `9:16`
- Initial platform target: Android or iOS can be configured later; prototype should run in Editor first.

Unity `2020.3.2f1` should be used for the vertical slice unless Chief of Staff approves a version change. The architecture below is intentionally conservative for this editor version.

## 2. Render Pipeline Recommendation

Use the Built-in Render Pipeline for the prototype.

Rationale:

- The repo has no Unity package baseline yet, so Built-in minimizes setup risk.
- The art style is simple, blocky, and toy-like; it can be achieved with unlit or simple lit materials.
- Built-in avoids URP asset creation, graphics settings migration, shader compatibility checks, and mobile renderer configuration.
- Faster prototype iteration matters more than final visual scalability.

Do not add URP in the vertical slice unless a later art/performance review identifies a specific need. If URP is adopted later, it should be a single coordinated migration task owned by Chief of Staff, not a parallel worker change.

## 3. Required Packages and Project Baseline

Minimum packages for `/Users/mrk/RoadtoWorldCup/Packages/manifest.json` after project initialization:

- `com.unity.textmeshpro`: use TextMeshPro for all in-game and menu text.
- `com.unity.ugui`: use Unity UI canvases for menu, HUD, pause, win, and fail screens.
- `com.unity.modules.physics`: use 3D physics triggers/colliders for ball, players, opponents, goal, and bounds.
- `com.unity.modules.audio`: optional, but keep available for click/pass/goal/fail feedback.
- `com.unity.modules.particlesystem`: optional, for confetti, pass pop, and goal feedback.

Recommended optional package:

- `com.unity.cinemachine`: optional only if Camera/Game Feel wants simple shot framing or goal zoom helpers. For the first playable build, a plain Unity `Camera` with a custom `CameraController` is enough.

Input recommendation:

- Use legacy `Input` APIs for this prototype (`Input.GetMouseButtonDown`, `Input.GetMouseButton`, `Input.GetMouseButtonUp`, `Input.touchCount`) instead of adding the new Input System. This keeps Unity 2020.3 setup low-risk.

TextMeshPro recommendation:

- Use TextMeshPro everywhere for UI and world labels, including jersey numbers if rendered as text. The UI/UX Builder should import TMP essentials during setup if Unity prompts for it.

## 4. Asset Folder Structure

All implementation files should live under `/Users/mrk/RoadtoWorldCup/Assets/_RoadToWorldcup/`.

Recommended structure:

```text
Assets/
  _RoadToWorldcup/
    Animations/
    Art/
      Materials/
      Meshes/
      Textures/
      VFX/
    Audio/
    Data/
      Levels/
    Fonts/
    Prefabs/
      Characters/
      Gameplay/
      UI/
      Environment/
    Scenes/
      Bootstrap.unity
      MainMenu.unity
      Gameplay.unity
    Scripts/
      Camera/
      Core/
      Data/
      Gameplay/
      Input/
      UI/
      Utilities/
    Settings/
    Tests/
      PlayMode/
      EditMode/
```

Keep third-party or generated Unity folders outside `_RoadToWorldcup` only when Unity requires it, such as `TextMesh Pro/`.

## 5. Scene Plan

### `Assets/_RoadToWorldcup/Scenes/Bootstrap.unity`

Optional but recommended once implementation begins.

Purpose:

- Own persistent app services if needed.
- Load `MainMenu.unity` on start.
- Set target frame rate and portrait orientation.

Keep it tiny. If implementation bandwidth is tight, workers may skip Bootstrap and start directly from `MainMenu.unity`, but app-wide setup must then be placed in `AppBootstrap`.

### `Assets/_RoadToWorldcup/Scenes/MainMenu.unity`

Purpose:

- Portrait stadium celebration menu inspired by the provided reference.
- Buttons: Play, Tournament, Customize, Missions, Daily Reward, Spin, Shop, Settings.
- Currency placeholders for coins and gems.

Play starts Level 1 by loading `Gameplay.unity` and setting selected level index through `GameSession`.

### `Assets/_RoadToWorldcup/Scenes/Gameplay.unity`

Purpose:

- Single reusable gameplay scene.
- Loads level layouts from `LevelDefinition` assets.
- Contains gameplay managers, camera, HUD, field root, and runtime spawn roots.

Do not create separate Unity scenes per level for the slice. Use data-driven level definitions so the Level Designer can tune levels without duplicating scene setup.

## 6. Script Responsibility List

Exact proposed C# class names and one-line responsibilities:

### Core

- `AppBootstrap`: Sets portrait orientation, frame rate, persistent services, and initial scene load.
- `SceneLoader`: Centralizes loading `MainMenu` and `Gameplay`.
- `GameSession`: Stores selected level index, last result, and lightweight cross-scene state.
- `ServiceLocator`: Optional tiny registry for shared scene services if direct serialized references become messy.

### Data

- `LevelDefinition`: ScriptableObject containing all tunable data for one level.
- `FriendlySpawnData`: Serializable data for friendly player id, number, position, rotation, behavior, receiver settings, and final target flag.
- `OpponentSpawnData`: Serializable data for opponent id, position, rotation, radius, and optional behavior.
- `ReceiverLinkData`: Serializable data for allowed receiver chain ordering and per-link tuning.
- `FieldBoundsData`: Serializable data for playable field min/max bounds and out-of-bounds margin.
- `ActiveBehaviorConfig`: Serializable data for auto-rotate or lateral-move parameters.

### Gameplay

- `GameplayController`: Owns gameplay state transitions, level start, win, fail, retry, and next level.
- `LevelRuntimeBuilder`: Spawns and wires runtime prefabs from `LevelDefinition`.
- `LevelRegistry`: Holds references to Level 1, Level 2, and Level 3 definitions.
- `PlayerController`: Represents one friendly player and exposes active, idle, receiver, and #10 target states.
- `OpponentController`: Represents one opponent obstacle and exposes idle and hit feedback states.
- `BallController`: Handles attach, launch, travel, collisions, stop detection, and reset.
- `PassController`: Coordinates hold-to-aim, power charge, release-to-pass, receiver validation, and fail reasons.
- `ActiveBehaviorController`: Runs auto-rotation or lateral movement before input hold and freezes on hold.
- `ReceiverChainController`: Tracks current friendly, valid next receivers, and final #10 delivery.
- `GoalController`: Receives #10 auto-shot and reports goal completion.
- `FieldBoundsController`: Detects ball leaving playable bounds.
- `AutoShotController`: Moves the ball from #10 into the goal and triggers win timing.
- `GameplayEvents`: Defines events such as pass started, pass succeeded, pass failed, target reached, goal scored, state changed.
- `FailReason`: Enum for `None`, `OpponentIntercepted`, `PassMissed`, `NotEnoughPower`, `OutOfBounds`, `StoppedShort`.
- `GameplayState`: Enum for `Loading`, `Ready`, `Playing`, `Aiming`, `BallTraveling`, `AutoShot`, `Won`, `Failed`, `Paused`.

### Input

- `TouchInputReader`: Converts touch and mouse into hold started, hold maintained, and hold released events.
- `InputLock`: Blocks gameplay input during travel, auto-shot, pause, win, and fail states.

### Camera

- `CameraController`: Applies static portrait gameplay framing and optional goal zoom.
- `CameraLevelFraming`: Stores per-level camera position, rotation, field of view, and safe UI framing offsets.

### UI

- `MainMenuController`: Wires main menu buttons and placeholder panels.
- `CurrencyBarView`: Displays coin and gem placeholders with plus buttons.
- `GameplayHUDController`: Displays level label, pause button, tutorial panel, goal panel, and power UI.
- `PowerMeterView`: Displays hold charge as fill and percentage near the aim lane without covering players.
- `TutorialPromptView`: Shows level-specific tutorial prompts.
- `PauseMenuController`: Handles pause, resume, retry, and menu actions.
- `ResultOverlayController`: Shows win/fail overlays and dispatches next, retry, and menu actions.
- `ComingSoonPanel`: Shared placeholder panel for locked menu features.

### Visual Helpers

- `RingView`: Shows active green ring, opponent red ring, and target yellow ring/glow.
- `AimArrowView`: Shows the current aim arrow from the active player.
- `PathPreviewView`: Draws a dashed path preview for the expected pass route.
- `JerseyNumberView`: Displays generic player numbers with TMP or simple mesh/text planes.
- `SimpleFeedbackView`: Plays flashes, pops, trail toggles, and particles.

## 7. Level Data Model Plan

Create level assets under:

- `Assets/_RoadToWorldcup/Data/Levels/Level_01_Tutorial.asset`
- `Assets/_RoadToWorldcup/Data/Levels/Level_02_AngledRoutes.asset`
- `Assets/_RoadToWorldcup/Data/Levels/Level_03_TimingChallenge.asset`

`LevelDefinition` fields:

```text
string levelId
string displayName
int levelNumber
Vector2 fieldMinXZ
Vector2 fieldMaxXZ
float outOfBoundsMargin
Vector3 goalPosition
Vector3 goalForward
Vector3 ballStartOffset
float receiverRadius
float opponentRadius
float ballSpeedMin
float ballSpeedMax
float chargeTimeToFull
float ballStopSpeedThreshold
float ballMaxTravelSeconds
List<FriendlySpawnData> friendlies
List<OpponentSpawnData> opponents
List<ReceiverLinkData> receiverChain
CameraLevelFraming cameraFraming
string tutorialGoalText
string tutorialHowToText
```

`FriendlySpawnData` fields:

```text
string friendlyId
int jerseyNumber
Vector3 position
float yRotation
bool startsActive
bool isTargetTen
ActiveBehaviorConfig activeBehavior
float customReceiverRadius
Color ringColorOverride
```

`OpponentSpawnData` fields:

```text
string opponentId
Vector3 position
float yRotation
float collisionRadius
bool isStaticForSlice
```

`ActiveBehaviorConfig` fields:

```text
ActiveBehaviorType behaviorType
float baseAimYaw
float sweepMinYaw
float sweepMaxYaw
float sweepSpeedDegreesPerSecond
Vector3 lateralStart
Vector3 lateralEnd
float lateralMoveSpeed
bool freezeOnHold
```

`ActiveBehaviorType` enum:

```text
None
AutoRotate
LateralMove
```

`ReceiverLinkData` fields:

```text
string fromFriendlyId
string toFriendlyId
bool required
float minPowerPercent
float maxPowerPercent
float suggestedPowerPercent
bool showPreviewHint
```

Receiver chain requirements:

- Level 1: `F1 -> F2 -> F3`, where `F3` is #10.
- Level 2: `F1 -> F2 -> F3 -> F4`, where `F4` is #10.
- Level 3: `F1 -> F2 -> F3 -> F4 -> F5`, where `F5` is #10.

Validation rule: a pass only succeeds if the receiver is a friendly in the current allowed chain step. This prevents accidental skips unless a later design task explicitly enables optional routes.

## 8. Prefab Plan

### Characters

`Assets/_RoadToWorldcup/Prefabs/Characters/FriendlyPlayer.prefab`

- Root with `PlayerController`.
- Blocky placeholder body.
- Blue/white generic kit material.
- Jersey number anchor.
- Ball attach point at feet.
- Active ring anchor.
- Target glow anchor.
- Capsule or sphere trigger collider.

`Assets/_RoadToWorldcup/Prefabs/Characters/Opponent.prefab`

- Root with `OpponentController`.
- Blocky placeholder body.
- Red generic kit material.
- Red ring anchor.
- Trigger collider used by `BallController`.

### Gameplay

`Assets/_RoadToWorldcup/Prefabs/Gameplay/Ball.prefab`

- Visible football placeholder.
- Sphere collider or trigger.
- `BallController`.
- Optional trail renderer.

`Assets/_RoadToWorldcup/Prefabs/Gameplay/Goal.prefab`

- Goal frame, net placeholder, and `GoalController`.
- Trigger volume in mouth of goal.

`Assets/_RoadToWorldcup/Prefabs/Gameplay/Field.prefab`

- Pitch plane, stripes, white lines, penalty box, center circle.
- `FieldBoundsController` can live on a child bounds object.

`Assets/_RoadToWorldcup/Prefabs/Environment/Stadium.prefab`

- Simple stands, crowd blocks, lights, and fictional boards.
- Must avoid official federation badges, FIFA marks, real sponsors, and copied signage.

`Assets/_RoadToWorldcup/Prefabs/Gameplay/ActiveRing.prefab`

- Green ring with soft glow for active friendly.

`Assets/_RoadToWorldcup/Prefabs/Gameplay/OpponentRing.prefab`

- Red ring for opponents.

`Assets/_RoadToWorldcup/Prefabs/Gameplay/TargetTenRing.prefab`

- Yellow ring/glow for #10.

`Assets/_RoadToWorldcup/Prefabs/Gameplay/AimArrow.prefab`

- White or green arrow aligned to active player forward direction.

`Assets/_RoadToWorldcup/Prefabs/Gameplay/PathPreview.prefab`

- Dashed white path segments plus optional arrowhead.

### UI

`Assets/_RoadToWorldcup/Prefabs/UI/MainMenuCanvas.prefab`

- Title, currency bar, left utility stack, main button stack, and central generic #10/trophy composition.

`Assets/_RoadToWorldcup/Prefabs/UI/GameplayHUDCanvas.prefab`

- Level label, currency placeholder, pause button, goal panel, tutorial panel, power meter.

`Assets/_RoadToWorldcup/Prefabs/UI/PauseOverlay.prefab`

- Resume, Retry, Menu.

`Assets/_RoadToWorldcup/Prefabs/UI/ResultOverlay.prefab`

- Win and fail states with Level Complete/Try Again, reason text, Next, Retry, Menu.

`Assets/_RoadToWorldcup/Prefabs/UI/ComingSoonPanel.prefab`

- Shared placeholder for Tournament, Customize, Missions, Daily Reward, Spin, Shop, Settings, and currency plus buttons.

## 9. UI Architecture Plan

Use one Screen Space - Overlay canvas per scene, with TextMeshPro for all labels.

### Main Menu

`MainMenuController` owns:

- Play button: calls `GameSession.SelectLevel(0)` and `SceneLoader.LoadGameplay()`.
- Tournament, Customize, Missions, Daily Reward, Spin, Shop, Settings: open `ComingSoonPanel`.
- Coin/gem plus buttons: open `ComingSoonPanel`.

Main menu layout requirements:

- Canvas scaler: `Scale With Screen Size`, reference resolution `1080 x 1920`, match `0.5`.
- Play button is largest and bottom-center.
- Tournament, Customize, Missions stack below Play.
- Currency placeholders sit top-right.
- Utility buttons stack left.
- Critical buttons remain visible in 9:16 and do not depend on device notch-safe areas for the prototype.

### Gameplay HUD

`GameplayHUDController` subscribes to `GameplayEvents` and updates:

- Top-left level label.
- Top-right pause button and optional currency placeholder.
- Goal panel for #10 objective.
- Tutorial panel for Level 1 and short level prompts for Level 2/3.
- Power meter only while aiming.

### Pause

`PauseMenuController` sets `GameplayState.Paused`, freezes input and time-dependent gameplay, and offers:

- Resume
- Retry
- Menu

### Win

`ResultOverlayController.ShowWin()` displays:

- `LEVEL COMPLETE`
- Optional coin reward placeholder.
- Next, Retry, Menu.

### Fail

`ResultOverlayController.ShowFail(FailReason reason)` displays:

- `TRY AGAIN`
- Reason text mapped from fail reason.
- Retry, Menu.

UI must never hide the active player, ball, receiver, or current pass lane. Place tutorial panels at left/lower-left as shown in the gameplay reference.

## 10. Camera Plan

Gameplay camera:

- Projection: Perspective.
- Orientation: portrait 9:16.
- Gameplay angle: approximately 70 degrees top-down.
- Field direction: bottom is start side, top is attacking goal.
- Goal should appear near the top of the screen.
- Active player usually in lower half.

Initial camera recommendation:

```text
Position: per level, roughly centered on field X, above and behind lower field side
Rotation: X = 70 degrees, Y = 0 degrees, Z = 0 degrees
Field of View: 45 to 55
```

`CameraController` should read `CameraLevelFraming` from the active `LevelDefinition` so the Level Designer and Camera/Game Feel worker can tune framing per level.

Camera behavior for vertical slice:

- Static during aiming and ball travel.
- No shake during hold.
- Optional very small punch/zoom on opponent hit, goal shot, and win.
- Optional short follow or zoom during #10 auto-shot only after core gameplay is stable.

Main menu camera:

- Use UI-first composition.
- If a 3D menu character is added, render it with a scene camera behind/under the UI, but do not block button readability.

## 11. Gameplay State Flow

State sequence:

```text
Loading
  -> Ready
  -> Playing
  -> Aiming
  -> BallTraveling
  -> Playing
```

Win branch:

```text
BallTraveling
  -> AutoShot
  -> Won
```

Fail branch:

```text
BallTraveling
  -> Failed
```

Pause branch:

```text
Playing or Aiming
  -> Paused
  -> previous state
```

Level start:

1. `GameplayController` receives selected level from `GameSession`.
2. `LevelRuntimeBuilder` clears spawn roots.
3. `LevelRuntimeBuilder` instantiates field, goal, friendlies, opponents, ball, rings, aim arrow, and path preview.
4. `ReceiverChainController` sets first active friendly.
5. `BallController.AttachTo(activeFriendly.ballAttachPoint)`.
6. `CameraController.ApplyFraming(level.cameraFraming)`.
7. `GameplayHUDController.ShowLevel(level)`.
8. State becomes `Playing`.

Pass flow:

1. `TouchInputReader` reports hold started.
2. `PassController` verifies state is `Playing` and active friendly has ball.
3. State becomes `Aiming`.
4. `ActiveBehaviorController.Freeze()` locks auto-rotation or lateral movement.
5. `AimArrowView` brightens and `PathPreviewView` displays expected lane.
6. `PowerMeterView` charges from 0 to 100 percent over `chargeTimeToFull`.
7. `TouchInputReader` reports release.
8. `PassController` computes launch speed between `ballSpeedMin` and `ballSpeedMax`.
9. `BallController.Launch(activeForward, speed)`.
10. State becomes `BallTraveling`.

Success flow:

1. `BallController` contacts friendly trigger.
2. `ReceiverChainController` verifies the friendly is the next allowed receiver.
3. `BallController.AttachTo(receiver.ballAttachPoint)`.
4. `PlayerController` states update: old active idle, receiver active.
5. `GameplayEvents.PassSucceeded` fires.
6. If receiver is #10, state becomes `AutoShot`; otherwise state becomes `Playing`.

Failure flow:

1. Ball contacts opponent, leaves `FieldBoundsController`, stops early, times out, or hits an invalid receiver.
2. `PassController` maps the condition to `FailReason`.
3. `GameplayEvents.PassFailed` fires.
4. `GameplayController` sets state `Failed`.
5. `ResultOverlayController` displays fail UI.

Final #10 auto-shot flow:

1. #10 receives ball.
2. `PlayerController` enables yellow target success glow.
3. `AutoShotController` faces #10 toward goal.
4. Ball moves from #10 to `GoalController` target point.
5. `GoalController` plays hit feedback.
6. `GameplayController` sets state `Won`.
7. `ResultOverlayController` displays win UI.

## 12. Worker Implementation Order and Ownership Boundaries

All output paths below are under `/Users/mrk/RoadtoWorldCup/Assets/_RoadToWorldcup/`.

### 1. Project Initialization Worker or Gameplay Programmer

Owns:

- `Packages/manifest.json`
- `ProjectSettings/`
- `Assets/_RoadToWorldcup/Scenes/`
- Initial empty folder structure under `Assets/_RoadToWorldcup/`

Must not implement full gameplay during project initialization. Create the baseline Unity project first so later workers can open it.

### 2. Gameplay Programmer

Owns:

- `Scripts/Core/`
- `Scripts/Data/`
- `Scripts/Gameplay/`
- `Scripts/Input/`
- `Prefabs/Gameplay/Ball.prefab`
- Runtime wiring in `Scenes/Gameplay.unity`

May read UI prefabs and art prefabs, but should not restyle UI or rebuild character art. If placeholders are needed before Art/UI are complete, create temporary simple prefabs only in agreed gameplay paths and report them.

### 3. Art Placeholder Builder

Owns:

- `Art/`
- `Prefabs/Characters/`
- `Prefabs/Environment/`
- `Prefabs/Gameplay/Field.prefab`
- `Prefabs/Gameplay/Goal.prefab`
- `Prefabs/Gameplay/ActiveRing.prefab`
- `Prefabs/Gameplay/OpponentRing.prefab`
- `Prefabs/Gameplay/TargetTenRing.prefab`
- `Prefabs/Gameplay/AimArrow.prefab`
- `Prefabs/Gameplay/PathPreview.prefab`

Must avoid real player likenesses, official crests, official tournament marks, real sponsor logos, and copied federation visuals.

### 4. UI/UX Builder

Owns:

- `Scripts/UI/`
- `Prefabs/UI/`
- Menu/HUD canvas objects in `Scenes/MainMenu.unity`
- HUD/result/pause canvas objects in `Scenes/Gameplay.unity`

Should use public methods/events from gameplay systems rather than editing gameplay logic.

### 5. Level Designer

Owns:

- `Data/Levels/`
- Level spawn positions and tuning fields in `LevelDefinition` assets.
- Non-code layout tuning in `Scenes/Gameplay.unity` only when required for spawn roots or markers.

Must not modify gameplay scripts. Requests new data fields through Chief of Staff if the data model is insufficient.

### 6. Camera/Game Feel

Owns:

- `Scripts/Camera/`
- Camera objects in `Scenes/Gameplay.unity`
- Per-level `cameraFraming` values in `Data/Levels/`
- Optional VFX timing values if coordinated with Art Placeholder Builder.

Should not move level objects except through approved level data fields.

### 7. QA Reviewer

Owns:

- `Tests/`
- QA reports under its worker folder.

Should not make feature edits unless explicitly assigned a bug-fix task. QA may add tests if the project compiles.

Conflict prevention:

- Only one worker should edit a Unity scene at a time unless Chief of Staff splits ownership by scene and hierarchy root.
- Prefab ownership follows the paths above.
- Shared script API changes must be proposed in worker outbox before another worker depends on them.
- `Assets/_RoadToWorldcup/Data/Levels/` belongs to Level Designer after Gameplay Programmer creates the initial data classes and sample assets.

## 13. Minimal Validation Plan by Worker

### Project Initialization

- Open `/Users/mrk/RoadtoWorldCup` with Unity `2020.3.2f1`.
- Confirm `Assets/`, `Packages/`, and `ProjectSettings/` exist.
- Confirm empty `MainMenu.unity` and `Gameplay.unity` open without compile errors.
- Confirm TextMeshPro essentials imported if UI text is present.

### Gameplay Programmer

- Editor compiles with no C# errors.
- Press Play in `Gameplay.unity` and Level 1 loads from `LevelDefinition`.
- Mouse hold freezes active behavior and charges power.
- Mouse release launches ball.
- Valid receiver advances active player.
- Opponent collision, out of bounds, stopped short, and missed pass trigger fail.
- #10 receive triggers auto-shot and win.

### Art Placeholder Builder

- All required prefabs exist at the specified paths.
- Friendly, opponent, ball, goal, rings, field, and stadium are readable from gameplay camera.
- Materials are brand-safe and do not contain real logos or protected tournament marks.
- Prefabs have expected anchors/colliders for gameplay wiring.

### UI/UX Builder

- Main menu fits 9:16 at `1080 x 1920`.
- Play loads gameplay.
- Placeholder buttons open a Coming Soon panel.
- HUD shows level label, tutorial, goal panel, pause, and power UI.
- Win/fail overlays block gameplay input and expose correct buttons.

### Level Designer

- Three `LevelDefinition` assets exist.
- Level 1 completes in two passes.
- Level 2 completes in three passes with two meaningful opponent blockers.
- Level 3 completes in four passes with lateral timing on the first active player.
- Field bounds and receiver chains match the GDD.

### Camera/Game Feel

- Gameplay remains readable in portrait 9:16.
- Active player, ball, #10, goal, and likely next receiver are visible when each pass begins.
- Aim arrow and dashed path do not hide the ball or receiver.
- No camera shake during aiming.

### QA Reviewer

- Run a clean compile check.
- Play from Main Menu through Level 1, Level 2, and Level 3.
- Verify Retry, Next, Menu, Pause, and fail reasons.
- Verify input is ignored during ball travel, auto-shot, win, fail, and pause.
- Report device/aspect issues for at least `1080 x 1920` and one narrower portrait aspect if possible.

## 14. Risks and Blockers

Active blockers:

- The project is not initialized as a Unity project. No implementation can be validated in Unity until `Assets/`, `Packages/`, and `ProjectSettings/` exist.
- Render pipeline, package manifest, and TextMeshPro availability are unknown until initialization.
- Reference image filenames are unusual and should be read from the actual paths documented by Chief of Staff.

Technical risks:

- Parallel scene edits can create Unity YAML merge conflicts. Assign scene ownership carefully.
- 3D physics may produce unreliable pass results if colliders, trigger layers, and ball speed are not standardized early.
- Path preview can become misleading if it does not use the same aim direction and power assumptions as `PassController`.
- World-space rings and UI can be hidden by camera angle if prefab anchors are not tested from the 70-degree view.
- Menu reference includes real-world visual cues; implementation must use generic, brand-safe kits, signage, trophy, and character design.
- Unity `2020.3.2f1` is old enough that package version mismatches may occur if workers add modern packages. Keep package additions minimal.

Open implementation decisions:

- Whether to include `Bootstrap.unity` in the first implementation pass or start directly from `MainMenu.unity`.
- Whether path preview is simple straight/dashed line or uses basic collision projection. For the vertical slice, a straight dashed preview is acceptable.
- Whether ball movement uses Rigidbody velocity or scripted interpolation. Recommendation: scripted movement with trigger checks for deterministic prototype behavior, unless Gameplay Programmer prefers Rigidbody and validates collision reliability.

## 15. Recommended Next Tasks

1. Initialize the Unity project at `/Users/mrk/RoadtoWorldCup` using Unity `2020.3.2f1` and the 3D template.
2. Create the `Assets/_RoadToWorldcup/` folder structure and empty scenes.
3. Add TextMeshPro and Unity UI baseline.
4. Gameplay Programmer creates data classes, gameplay state machine, input reader, ball/pass controllers, and a rough Level 1 test loop.
5. Art Placeholder Builder creates readable prefabs and materials at the specified paths.
6. UI/UX Builder creates Main Menu and Gameplay HUD prefabs using TMP.
7. Level Designer creates Level 1-3 data assets.
8. Camera/Game Feel tunes portrait framing and pass readability.
9. QA Reviewer performs compile and playthrough validation.
