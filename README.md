# The King: Road to Champion

`The King: Road to Champion` is a portrait mobile football passing game. Guide the ball through your team, collect gems, and set up #10 for the finish.

## Unity Version

Open and test this project with **Unity 6000.4.10f1**.

Do not use Unity 2020.3.2f1 for this project. The Main Menu layout and current runtime UI work should be verified in Unity 6000.4.10f1.

Recommended editor path on this machine:

```text
/Applications/Unity/Hub/Editor/6000.4.10f1/Unity.app
```

## Testing Main Menu

Use a portrait mobile Game view, preferably **1080 x 1920** or another 9:16 portrait size.

The Main Menu is generated at runtime by:

```text
Assets/_RoadToWorldcup/Scripts/UI/GeneratedMenuController.cs
```

The menu background and button/logo sprites are loaded from:

```text
Assets/_RoadToWorldcup/Resources/MainMenuBackground.png
Assets/_RoadToWorldcup/Resources/MainMenuUI/
```

When adjusting the Main Menu, test in Play Mode with Unity 6000.4.10f1 and compare against the reference screenshot before finishing.

## Art Style Guide

The King: Road to Champion uses a bright toy-football mobile style: blocky characters, saturated stadium lighting, readable arcade UI, and big glossy buttons.

### Visual Pillars

- **Theme:** celebratory World Cup stadium, confetti, fireworks, green pitch, trophy gold, electric blue UI.
- **Characters:** blocky toy proportions with simple shapes, bold jersey colors, clean silhouettes, and high contrast details.
- **Mood:** cheerful, premium, energetic. Avoid muted realistic simulation UI.
- **Camera:** portrait-first mobile framing. Keep important UI inside 1080 x 1920 safe areas.

### Main Menu UI

- Match `Assets/_RoadToWorldcup/Resources/MainMenuReference.png`.
- Use the sprite set in `Assets/_RoadToWorldcup/Resources/MainMenuUI/` for menu logo, currency pills, and main/side buttons.
- Main actions stack near the bottom: green `PLAY`, blue `TOURNAMENT`, gold `CUSTOMIZE`, purple `MISSIONS`.
- Side utility buttons stay on the left with icon-first square art: `DAILY REWARD`, `SPIN`, `SHOP`, `SETTINGS`.
- Currency pills stay top-right with dark backing, bright coin/gem icon, white value, and green plus button.

### Gameplay UI

- Use dark translucent navy panels with bright cyan/blue outline and soft black shadow.
- Buttons should feel like chunky mobile game buttons: rounded rectangle, saturated fill, darker lower border, top shine, bold white uppercase text with black shadow/outline.
- Primary action color is green. Secondary action color is blue. Caution/reward accents use trophy gold. Neutral actions use dark navy/charcoal.
- HUD cards should be compact and readable over the pitch, not flat floating text.

### Popups

- Popups use a full-screen dark scrim plus a centered dark navy card.
- Card styling: rounded panel, cyan outline, soft shadow, subtle top shine.
- Titles use trophy gold or white uppercase with shadow.
- Popup buttons use the same gameplay button style and spacing.

### Implementation Notes

- Runtime menu UI lives in `Assets/_RoadToWorldcup/Scripts/UI/GeneratedMenuController.cs`.
- Runtime gameplay HUD, buttons, and popups live in `Assets/_RoadToWorldcup/Scripts/Gameplay/RoadToWorldcupGame.cs`.
- Keep generated UI on the 1080 x 1920 reference resolution unless the entire layout system is changed.
