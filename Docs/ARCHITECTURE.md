# Version 0.2 architecture

## Generation and geometry

`MazeGrid.cs` stores cell wall masks and level placements. `MazeGenerator.cs` uses seeded DFS, a small number of loop openings and later courtyards, then a BFS route to a far goal. Hazards are placed after the route. `MazeValidator.cs` checks connectivity, symmetric closed boundaries, start/goal, analytic wall/turn clearance, sampled swept-ball clearance around expanded hazards, safe hole bounds and checkpoint reachability. Circular holes live off the guaranteed safe route; obstacles may narrow side space but keep the route passable.

`DifficultyConfig.cs` increases density gradually with fixed ball tuning. `LevelManager.cs` owns deterministic replay and fresh next-level seeds. `MazeBuilder.cs` extends the original scene root to create matching collision/render geometry. Wall segments stop at shared posts so joins have no overlapping top faces. Floors/walls use batched MultiMeshes; floor colliders share simple shapes. Each hole uses a tessellated rectangle-to-circle ring, matching concave floor collider, torus rim, open shaft and recessed base. The circular aperture is physically empty.

`StaticBumper.cs` uses cylinders and matched cylinder colliders, or box barriers and matched box colliders. `HealingCircle.cs` restores health once per run, dims after use and remains available at full health. ThemeManager/MaterialLibrary provide glass, brushed metal and stone procedural materials without external assets.

## Physics and state

The existing RigidBody3D/force model and all tuning remain. BallController only gained configurable reset positions and replaced old forced square-hole capture with below-floor falling detection. No collision masks are removed for a hole; grazing tests keep normal contacts. GameManager owns the explicit GameState transitions, camera fit only at level creation, timer, pause, fall count, delay, start respawn and completion. Ready waits for the physics reset to finish, avoiding the earlier restart race. GoalTrigger caches area entry and checks the center without per-frame overlap-array allocation. HUD text updates at 10Hz.

AndroidTiltInput and TiltCalibration are byte-identical to the v0.1.9 rollback backup. AudioManager preserves continuously running music/ambience and speed-scaled rolling; adds local fall/checkpoint sounds and a modest theme pitch change for impacts. HapticFeedback subscribes to existing intensity events; light contact is ignored, pulses scale from 12–38ms / .16–.65 amplitude with a 160ms cooldown. User can disable vibration independently of audio mute.

SaveManager handles bounded/corrupt progress and atomic replacement. AudioSettings extends the existing audio.cfg with Haptics, defaulting on for old saves. There are no network or account dependencies.

0.2.11 replaces the wood theme with glass in the repeating theme rotation: glass 1–3, metal 4–6, stone 7–9. Glass uses reflective blue translucent floor and walls; friction and collisions are unchanged. Healing circles add 25 health, capped at 100, once per run, and remain unused at full health.
