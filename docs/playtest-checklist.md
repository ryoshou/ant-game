# Ant Farm v1 — Playtest Checklist

From `docs/superpowers/specs/2026-10-24-ant-farm-design.md`. Fill in the results after a session on the phone.

## Install (iOS)

1. Open `Builds/iOS/Unity-iPhone.xcodeproj` in Xcode.
2. Target **Unity-iPhone** → Signing & Capabilities → tick *Automatically manage signing*, pick your Team.
3. Connect and unlock the iPhone, select it as the run destination, press Run.
4. First launch only: on the phone, Settings → General → VPN & Device Management → trust the developer app.

Rebuild after code changes: in Unity, `unity command build --target iOS --outputPath Builds/iOS --options Development`
(or File → Build Profiles → iOS → Build), choose *Append* when asked.

## Debug panel

Hold the dirt counter (top left) for 2 seconds. It shows upgrades bought, ants/cap, FPS, income/s, average trip
length and dirt left on the map; edits base cost / growth / per-level per upgrade (runtime only — use
*Copy tuning JSON* to keep values); and has +1000 dirt, Fill colony, Max colony size, Reset colony.

## Session checks

| # | Check | Target | Result |
|---|---|---|---|
| 1 | Squishing one ant clearly reads as a hit (sound, splat, buzz, tiny shake) | feels good | |
| 2 | Dragging through ~10 ants is punchy, not a wall of noise | feels good | |
| 3 | Painting dirt back feels soft and satisfying; base can't be painted | feels good | |
| 4 | Two fingers pan + pinch-zoom in every mode | works | |
| 5 | Look mode: one finger pans freely | works | |
| 6 | Upgrades bought at 10:00 (debug panel, no debug gifts) | 25–35 | |
| 7 | Tier 3 (red ants, big mound) arrives around 10:00 | ~10:00 | |
| 8 | Colony visibly bigger at 10:00 than at start | yes | |
| 9 | Close the app from the app switcher, reopen: same map, dirt, upgrades, ants | restored | |
| 10 | Paint hint appears once when trips get long (~15 tiles) | once | |
| 11 | Walling ants in with dirt: they dig out toward the base, never freeze | no stuck ants | |

## Frame-rate floor

Debug panel → *Max colony size* → *Fill colony* (45 ants). Squish and paint continuously for 60 s.
Then let the ants spread out, paint a closed ring of dirt around the base (every outside ant is walled off and
starts digging back in), and watch FPS while they tunnel home.

| Device | Min FPS seen | Typical FPS | Notes |
|---|---|---|---|
| iPhone 17 (designer's) | | | |
| iPhone 11 (floor) | | | |
| ~2021 budget Android (floor) | | | Android build support not installed yet |

## Notes / tuning ideas

-
