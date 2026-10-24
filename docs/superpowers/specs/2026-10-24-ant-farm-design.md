# Ant Farm — v1 Design

**Date:** 2026-10-24
**Status:** Approved 2026-10-24
**Platform:** Phone (iOS/Android), built in Unity
**Audience for v1:** a private playtest build for the designer only (TestFlight
/ internal Android track or a direct install). No store listing, privacy
labels, crash reporting, or save-versioning guarantees in v1.

## Intent

A relaxing ant-farm toy with playful messing-with. The player watches a colony
dig out its world and grow, and teases it along the way: squishing ants and
burying their tunnels so they have to dig again. The player is both the
colony's tormentor and its caretaker — they slow it down *and* choose how it
grows.

There is no losing. The colony always recovers and keeps growing. Killing and
burying are teasing, never punishment.

**Why people keep playing (from the designer):**
- Watching the colony evolve as you feed it dirt to eat.
- Seeing it grow and upgrade.
- The feel of killing ants.

## Goals (v1)

- A 10-minute session feels good: squishing and painting dirt are satisfying,
  and the colony is visibly bigger at the end than at the start.
- An hour in looks obviously different from minute one (more ants, bigger and
  different-looking ants, bigger base).

## Non-goals (v1 — later)

- Fighter ants and enemies (fighters have no job until enemies exist).
- Queen as her own unit; more ant types beyond workers.
- Offline progress, multiple maps, monetization, achievements.
- Final art. Placeholder art is fine; feel comes first.

## Anti-goals

- **Stressful or punishing.** No fail state, no timers pressuring the player,
  no losing progress because you killed ants.
- **Mushy interactions.** If the squish or the dirt painting feels flat, v1 has
  failed even if everything else works.

## The game

### World
- One top-down map, a few phone screens wide, viewed from straight above.
- At the start the whole map is solid dirt. The ant base sits in the center.
- The map is a grid of dirt tiles. A dug tile becomes open ground the ants walk
  on — their "roads." *(Grid is my call; tile size left to the builder.)*

### Ants (v1: workers only)
- The base hatches workers over time, up to the colony cap. The queen is
  implied to live inside the base; she is not a visible unit in v1.
- Workers leave the base, dig dirt tiles at the edge of open ground, carry the
  dirt back to the base, and deposit it.
- Deposited dirt is the only currency.
- When no dirt is left on the map, ants idle (wander near the base) until the
  player paints more.
- Which tile an ant chooses to dig next is left to the builder. It should look
  like a colony spreading organically outward, not a lawnmower.

### Player actions
- **Kill (default mode):** tap an ant to squish it, or drag one finger across
  ants to squish every ant the finger passes over. Must feel great — squish
  sound, splat mark that lingers briefly, haptic buzz, tiny screen shake. When
  a drag squishes many ants at once, the feedback should stay punchy rather
  than turning into noise (e.g., stagger or cap sounds/haptics per kill —
  details left to the builder).
  Killed ants are simply gone; the base hatches replacements as usual. Any dirt
  the ant was carrying is lost. *(Lost-dirt is my call.)*
- **Dirt mode:** a UI button toggles it. Tap, or hold and swipe, to paint dirt
  back onto open ground. Unlimited and free. Painting over an ant buries it
  (kills it) — *my call; builder may change if it feels bad.* Dirt can't be
  painted onto the base itself.
- **View mode:** a UI button switches to view mode, where one finger swipes to
  move freely around the map and nothing gets killed or painted.
- **Camera:** two fingers pan and pinch-zoom in any mode, so moving around
  never requires leaving kill or dirt mode. *(My call.)*
- The three modes (kill — default, dirt, view) are picked from one mode
  switcher; the current mode is always clearly shown.

### Upgrades
- The player spends deposited dirt on upgrades they choose, in an upgrade
  panel. Each upgrade gets more expensive per level.
- v1 upgrades (worker stats + colony):
  1. Walk speed
  2. Dig speed
  3. Carry amount
  4. Colony cap (max ants)
  5. Hatch speed
- Starting values, costs, and pacing: see **Economy & pacing** below.

### Visible growth
- At upgrade tiers, workers get a bit bigger and their look changes (color,
  markings, or shape), and the base visibly grows. Tier thresholds are in
  **Economy & pacing**.

### Saving
- The colony (map state, ant count, currency, upgrade levels) is saved and
  restored when the app is closed and reopened. Ant positions don't need to be
  exact on restore.

## Economy & pacing

> All numbers are **starting points for playtesting**, not final. They come
> from a Game Designer pass with a quick 60-minute pacing simulation (greedy
> cheapest-upgrade player, nearest-dirt digging, no kills, no ant crowding —
> real income will run somewhat lower). Expose them as tunable config.

### Unit model
- **Map:** 64 × 64 tiles. At default zoom ~18 tiles across ≈ 3.5 screens wide.
- **Dirt per tile:** 10. A tile becomes tunnel when all 10 are dug out.
- **Base:** pre-dug 5 × 5 tiles in the center; deposit point in its middle.
- **Total starting dirt:** ≈ 40,700.
- **Ant targeting (starting point):** nearest diggable dirt tile touching a
  tunnel. Builder may add randomness so the spread looks organic.
- **Trip time** ≈ 2 × distance ÷ walk speed + carry ÷ dig speed + 0.5 s deposit.

### Upgrades

| Upgrade | Start | Per level | Max lvl | At max | Cost (lvl L → L+1) |
|---|---|---|---|---|---|
| Walk speed | 2.0 tiles/s | +0.2 | 15 | 5.0 tiles/s | 20 × 1.45^L |
| Dig speed | 2.0 dirt/s | +0.4 | 15 | 8.0 dirt/s | 20 × 1.45^L |
| Carry amount | 2 dirt | +1 | 8 | 10 dirt | 40 × 1.70^L |
| Colony cap | 5 ants | +2 | 20 | 45 ants | 50 × 1.50^L |
| Hatch speed | 12 s/ant | ×0.88 | 12 | ≈ 2.6 s/ant | 30 × 1.50^L |

- Walk and dig are cheap, frequent small wins. Carry is the strongest per-ant
  multiplier, so it's priciest. Cap is the most visible growth; 45 keeps the
  screen readable and phones at frame rate. Hatch speed is the "recover from
  my killing" upgrade.

### Starting colony
- 3 workers, cap 5. Fills its cap in ~24 s, so growth shows immediately.
- Killed ants are replaced at the current hatch rate; a kill costs a short
  income dip at most.

### Pacing targets

| Time | Upgrades bought | Ants | Income |
|---|---|---|---|
| 0:12 | 1st (20 dirt) | 3 | ~1.5 dirt/s |
| 1:00 | 4 | 5 | ~2 dirt/s |
| 5:00 | 18 | 9 | ~5 dirt/s |
| **10:00** | **~30** | **15** | **~10 dirt/s** |
| 30:00 | ~50 | 23 | ~16 dirt/s, flattening |
| **60:00** | ~60 (no painting) / ~66 (painting) | 27–37 | 15 / 64 dirt/s |

**Why painting matters.** Trips get long faster than speed upgrades
compensate: by minute 20 the average one-way trip is ~20 tiles and income
flattens. Painting dirt back near the base cuts trips to ~10 tiles and
roughly triples income. So painting is the player's best boost long before the
map runs dry — players paint because they want to, not because they must.
Without painting, the map runs fully dry around minute 55, so a first session
never sees idle ants. Painting right next to the base is the best strategy;
that's intended toy behavior, not an exploit.

### Visual growth tiers
Triggered by **total upgrade levels bought** (max 70). A tier change plays a
short burst and a sound.

| Tier | Total levels | ≈ When | Ant | Base |
|---|---|---|---|---|
| 1 | 0 | start | small, plain | small mound |
| 2 | 10 | ~2:30 | ×1.15 size, darker body | wider mound, 2nd entrance |
| 3 | 30 | ~10:00 | ×1.3 size, new color/markings | big mound, chambers showing |
| 4 | 55 | ~45:00 | ×1.45 size, shiny "elite" look | grand mound with spire |

Tier 3 lands at the end of a 10-minute session on purpose.

Base growth is **visual only**: it stays within the pre-dug 5 × 5 footprint,
with the same single deposit point. The "2nd entrance" and chambers are art,
not gameplay. *(My call — keeps the grid, pathing, and saves unchanged by
tiers.)*

**Performance floor:** 45 ants at 60 fps on a ~2021 budget Android phone and an
iPhone 11. *(My call; adjust when real test devices are known.)*

### Balance risks and knobs
1. **Upgrades come too fast early** (> ~4/min) and stop feeling special.
   Knob: cost growth (walk/dig 1.45 → 1.55 first). Target 25–35 upgrades by
   minute 10.
2. **Mid-session grind (min 15–30)** from long trips, for players who never
   paint. Knobs: walk speed per level (0.2 → 0.3), map size (64 → 56). Show a
   paint hint once the average trip exceeds ~15 tiles.
3. **Too many ants:** clutter, frame drops, fiddly tapping. Knobs: cap per
   level and max cap level; lower if 45 ants drops below 60 fps on low-end
   phones.

## Screen

- Portrait. The map fills the screen.
- Minimal HUD: dirt (currency) counter, mode switcher (kill / dirt / view),
  upgrade button that opens the upgrade panel. Exact layout left to the
  builder.

## Tuning during playtest

- All economy numbers live in editable config in the Unity editor.
- A small hidden in-game debug panel lets the designer tweak the key numbers
  on the phone and see live stats (upgrades bought, ants, income/s, average
  trip length) to compare against the pacing table. *(My call — the designer
  is the only playtester, so on-phone tuning beats analytics.)*

## Left to the builder

- Unity version, render pipeline, project structure, how ants are simulated
  (pathfinding, flow fields, etc.) — must stay smooth with the max colony size.
- Zoom limits; tile pixel size (map is 64 × 64 tiles per Economy & pacing).
- Ant dig-target selection beyond the starting rule, as long as the spread
  looks organic.
- Placeholder art style, sound effects.
- Save format.
