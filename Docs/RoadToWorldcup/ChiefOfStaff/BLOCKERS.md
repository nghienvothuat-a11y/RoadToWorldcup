# Road To Worldcup - Blockers

## Active Blockers

| ID | Severity | Description | Owner | Next Action |
| --- | --- | --- | --- | --- |
| BLK-002 | Low | Reference image filenames do not match the expected names in the request. | Chief of Staff | Keep the actual paths in every worker prompt; normalize only in a dedicated cleanup task. |
| BLK-004 | Medium | Victory VFX previously logged `Setting the duration while system is still playing` four times per win sequence. | Chief of Staff | TASK-015 patch is in review; reload scripts, trigger victory, and confirm no fresh warning. |
| BLK-005 | P0 for TestFlight, not a gameplay blocker | Full Xcode is not the active developer directory, so an iOS archive/upload cannot yet be validated. | User / Unity Architect | Switch `xcode-select` to `/Applications/Xcode.app/Contents/Developer`, then verify `xcodebuild` and `xcrun --sdk iphoneos`. |
| BLK-006 | P0 for TestFlight | Bundle identifier, Apple Team ID, provisioning/signing, build number, and app icons are not configured for App Store Connect. | User / Unity Architect | Decide registered bundle ID and Apple Team; configure PlayerSettings/signing before iOS archive. |
| BLK-007 | P1 for device performance | Level 30 creates over one thousand primitive renderers/colliders and rebuilds ring geometry every frame. | Unity Architect / Gameplay Programmer | After gameplay feel pass, assign an optimization task to remove unused colliders, reduce shadows, and cache ring geometry. |
| BLK-008 | Resolved | Computer Use clicks did not activate the runtime Play button in Unity Game View after reload; UI audit also found overlap around the current-level label and Play button. | UI / UX Builder | TASK-017 removed the overlap, restored visible controls, and Chief of Staff verified Play enters Gameplay. |

## Resolved Blockers

- BLK-001: Unity project baseline now exists and has been migrated to `6000.4.10f1`.
- BLK-003: Unity opens through Hub with an active license and Play Mode works.
