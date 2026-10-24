# ant-game — portrait mobile ant-farm toy, Unity 6000.6 (2D URP)

## Goal right now
v1 private playtest build per `docs/superpowers/specs/2026-10-24-ant-farm-design.md`; build plan in
`docs/superpowers/plans/2026-10-24-ant-farm-v1.md`. Feel first, placeholder art. No SDKs (ads, analytics, IAP).

## Layout
- `Assets/_Game/Scripts/Runtime/` → `Game.Runtime.asmdef`
  - `Core/` economy (TuningConfig, Wallet, UpgradeBook, GrowthTiers) · `World/` DirtGrid, GridSearch · `Sim/` Colony, ants
  - `Services/` save/load · `Controls/` gestures, brush, camera, InputRouter · `View/` grid/ant/base rendering
  - `UI/` HUD, mode switcher, upgrade panel · `Juice/` feedback (listens to events, never referenced by gameplay)
  - `Audio/` sfxr synth (ported from first-hyper-casual) · `Debugging/` stats + hidden debug panel
- `Assets/_Game/Scripts/Editor/` → `Game.Editor.asmdef` (editor tools only)
- `Assets/_Game/Config/` → ScriptableObject assets (TuningConfig, JuiceConfig, SfxPresets)
- `Assets/_Game/Scenes/Game.unity` → the single gameplay scene
- `Assets/Tests/EditMode|PlayMode/` → test asmdefs (reference `Game.Runtime`)
- `Assets/Plugins/iOS/Haptics.mm` → iOS haptics bridge. `Assets/Settings/` → URP + Input System assets from the template.
- `tools/unity/*.sh` → CLI helpers: compile, EditMode tests, PlayMode tests.

## Architecture
- Game rules are plain C# (`Core`, `World`, `Sim`, `Services`, gesture parsing) driven by EditMode tests.
  MonoBehaviours only render, route input, and play feedback. `GameBootstrap` owns and ticks the model.
- Tunables live in `_Game/Config/*.asset`; never hardcode gameplay numbers. `GameBootstrap` clones TuningConfig at
  runtime so the debug panel never edits the asset.
- Grid: one tile = one world unit, map centred on the origin (`GridCoords`).
- Input: Input System EnhancedTouch. Modes Kill (default) / Dirt / View; two fingers always pan + pinch.

## Code conventions
- Namespace `Game.<Folder>` (e.g. `Game.Sim`, `Game.Controls`). One MonoBehaviour per file.
- `[SerializeField] private` over public fields.
- No `GameObject.Find` / `FindObjectOfType` at runtime; no allocations in `Update` (no LINQ, no string concat).
- New scripts go in the correct asmdef folder — never in `Assets/` root.

## Tool routing (MUST follow)
| Task | Route |
|---|---|
| Write/edit C# | Edit files directly, then `tools/unity/compile.sh` (or `unity command recompile` → `recompile_status`) and `unity command console --level error` before saying done |
| Scenes, GameObjects, components, prefabs, assets | Live Editor via `unity command <name>`; arbitrary C# via `unity command eval_file --file <scratch.cs>` (body-only: no `using`, fully qualify types). NEVER hand-edit `.unity` / `.prefab` / `.asset` / `.meta` YAML |
| Packages | `unity command package_add` / `package_remove`, then wait for domain reload. Never edit `packages-lock.json` |
| Tests | `tools/unity/edit-tests.sh [Fixture]`, `tools/unity/playmode.sh`. Editor closed: `unity test . --mode EditMode` |
| Build | Editor open: `unity command build` + `build_status`. Editor closed: `unity build` |
| Visual check | `unity command capture_game_view` / `capture_scene_view`, or unity-mcp `Unity_Camera_Capture` |

- Always pass `--caller plugin --skill <skill>` on `unity command` calls; `--format json` when parsing.
- After package/asmdef changes the Editor domain-reloads; poll `unity command editor_status` until it answers.

## Testing (overrides global 80% coverage / TDD rules for this repo)
- EditMode tests for pure logic: economy, grid, search, colony sim, gestures, save/load, stats.
- One PlayMode smoke test: load Game scene → colony runs and earns dirt → no errors.
- No tests for game feel, tweens, VFX, UI look — the developer verifies those by playing.

## Don'ts
- Don't add packages or SDKs without asking. Don't change ProjectSettings unless asked.
- Don't commit `Library/`, `Temp/`, `Logs/`, `UserSettings/`, builds, `.superpowers/`.
- Binary art/audio/fonts go through Git LFS (patterns in `.gitattributes`).
