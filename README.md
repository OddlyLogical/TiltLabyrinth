# Tilt Labyrinth 0.2.17

An offline Android tilt labyrinth for Google Pixel 7a. This updates the existing Godot 4.7.2 .NET / C# project in `D:\Repos\TiltLabyrinth`; it is not a replacement project.

Guide the stone ball from the blue start to the green EXIT target. Only phone tilt controls movement. Hold comfortably and tap **Calibrate**, keeping still for half a second. Walls, floor and camera stay fixed during each level.

## Latest update (0.2.17)

- Rough, mottled stone ball replaces polished steel; metallic seam removed. Size and physics retained.
- Rolling sound now uses the user-supplied krnbeatz-tectonic-passage-418836.mp3 with speed-responsive volume/pitch and looping.

- User-supplied rock rolling MP3 replaces synthesized rolling audio, looping with speed-responsive volume/pitch and fading when stopped.

- Ball radius increased from 1.1 to 1.21 (10% larger); hazard clearance revalidated.
- Continuous electric wall contact repeats shocks every 0.5 seconds, including sound, haptics and health damage. Moving away stops shocks.

- Small, medium and large holes (diameters 3.5, 4.3 and 5.4 world units), with matching floor openings and capture zones.
- Wider 12.4-unit passages and tighter horizontal framing use more of the landscape display without cropping the maze.
- Textured stone, brushed metal and reflective glass, with a stationary workbench, frame and fittings around the board.
- Bomb-shaped obstacles with fuses; flashes, fire, sparks, smoke and expanding shockwaves scale with explosive size.

## What changed

- Seeded depth-first mazes with loops, branching routes, turns, dead ends and occasional courtyards. Replay and Restart preserve the seed; Next Level creates a new layout.
- Six difficulty tiers, progressively denser layouts and more static hazards; ball physics/speed do not change with difficulty.
- Glass (levels 1–3), brushed metal (4–6), stone (7–9), then repeating three-level theme groups. Local procedural shaders, shared meshes/materials and batched floor/walls.
- Real circular floor openings, visible rims/shafts and physical falling. Grazing a rim is safe; falls pause the timer, play a gentle sound and respawn at the level start after a short delay.
- Static bumpers and raised barriers; green healing circles on longer levels, adding 25 health points once per run.
- Explicit Loading/Ready/Playing/Paused/Falling/Respawning/LevelComplete states. Completion shows elapsed time, falls, best time, Next Level and Replay Level.
- Saved highest unlocked level, current level/seed and best times, plus existing audio preferences and new wall-impact vibration preference.
- Wall-impact haptics, requested during this implementation: strength-scaled short pulses with threshold/cooldown, Android VIBRATE permission and a toggle in Audio.

## Preserved baseline

`AndroidTiltInput.cs` and `TiltCalibration.cs` are byte-for-byte unchanged from the backup. Ball radius 1.1, mass 1, acceleration 27, speed cap 12, damping .4/.25, friction .7, bounce .04 and CCD are retained. The old square-hole forced-capture code was replaced because it disabled rim collisions and violated the v0.2 requirement for genuine circular falling. Reset now accepts the current level/checkpoint position. Existing music, ambience, rolling/impact/victory assets, buses, sliders, mute and local preferences remain.

## Difficulty

| Levels | Grid | Maximum requested holes | Bumpers / barriers |
|---|---|---:|---:|
| 1–2 | 5 × 3 | 0 | 0–1 / 0 |
| 3–4 | 6 × 4 | 1 | 2 / 1 |
| 5–7 | 7 × 4 | 2 | 3 / 2 |
| 8–10 | 8 × 5 | 3 | 4 / 3 |
| 11–15 | 9 × 5 | 5 | 5 / 4 |
| 16+ | 10 × 6 | 7 | 6 / 5 |

The brief's square grid sizes were tunable suggestions. Rectangular grids and 9.6 × 7.2 world-unit cells suit the Pixel's landscape screen and the existing large ball. Counts are upper placement targets: safety takes priority. A validated centerline route remains hazard-free; risky side branches and alternative routes contain holes. Side obstacles narrow passages without blocking the guaranteed route. No moving obstacles were added.

## Build and play

Open `project.godot` in Godot 4.7.2 .NET. Compile `dotnet build TiltLabyrinth.sln`. Android preset exports ARM64 with Mobile/Vulkan rendering, landscape orientation, no internet permission and the package `com.tiltlabyrinth.game`.

Debug APK: `Builds/TiltLabyrinth-v0.2.9-debug.apk`.

See [BUILD](Docs/BUILD.md), [ANDROID](Docs/ANDROID.md), [CONTROLS](Docs/CONTROLS.md), [VALIDATION](Docs/VALIDATION.md), [ARCHITECTURE](Docs/ARCHITECTURE.md), and [baseline inspection](Docs/BASELINE-V02.md).

No keyboard movement, desktop gameplay controls, virtual joystick, GDScript gameplay or network dependency. Windows is used for development and automated tests only.

## Electric walls, proximity bombs and countdown (0.2.1)

Each level/replay starts at 100% health. Wall impacts cost 10 percentage points, with a 0.45-second shock cooldown. Floor and ordinary rim contacts cause no electrical damage. Cyan animated wall strips mark live walls. Explosive obstacles use an invisible proximity boundary; moving within 0.15 units of surface-to-ball clearance detonates the obstacle once, costing 25 health. Exploded obstacles reset only on level replay/restart.

At zero health, Game Over stops the run. Countdown allowance is max(120 seconds, route cells × 8 seconds); expiry also ends the run. Pause, calibration and falling do not consume time. Hole/checkpoint respawns retain health and remaining time. Restart refills health/time and keeps the seed; Next Level also starts full.

0.2.2 automatically retries initial calibration after motion and keeps the player rigid body awake so it responds after idle. Hold still briefly; no repeated Calibrate taps are needed after a rejected initial sample.

## Healing circles (0.2.3)

Checkpoints are replaced by green circles with a white medical cross. Entering one below 100% adds 25 health points, capped at 100%, plays a soft chime, and dims the used circle. Each circle works once per run; touching it at full health does not consume it. Restart/Replay restores all circles. They never change the respawn location. All hole falls return to the level start, preserving health, remaining time and maze seed.

0.2.4 uses the user-supplied Audio/Effects/shock.mp3 for electrified wall impacts. A 250ms retrigger cooldown avoids sound spam; ordinary collisions retain the original clink. Shock volume follows impact strength and the Effects/master controls.

0.2.5 loops the user-supplied soundreality-metallic-ambience-593615.mp3 on the Ambience bus. Ambience volume, master mute and pause apply, and playback continues across level transitions.

0.2.6 alternates shock.mp3 and freesound_community-electrical-shock-zap-106412.mp3 on accepted electric wall hits, through Effects with the existing 250ms cooldown.

0.2.7 uses universfield-epic-cinematic-explosion-454857.mp3 for bomb detonations through Effects, preserving mute/volume/pause behavior.

Bombs alternate the user-supplied Universfield cinematic explosion and Dragon Studio explosion, through the Effects bus. Both obey Effects volume, master mute and pause.

Explosive proximity rings are hidden; their existing detonation boundary is unchanged.

0.2.8 makes the exit a larger gold illuminated bullseye on a dark disk with a clear EXIT label. Green medical-cross circles remain healing pickups. Goal capture and collisions are unchanged.

0.2.9 adds a spinning, shrinking visual sink and expanding ripples for real hole falls. Reaching the green EXIT target starts a 1.3-second swirling drop before completion. Ball collisions/forces and actual hole falling stay unchanged; only the visual child animates. Camera, level timer and health are held during the exit sequence. Restart restores the full ball visual.

0.2.10 adds invisible danger zones to black holes. When ball-to-rim clearance reaches 0.15 units, the hole captures the ball, pulls the visual inward and plays the animated fall. No permanent danger ring is shown. This supersedes the earlier rim-grazing-only behavior. Safe route validation includes the expanded capture radius.

0.2.11 replaces the wood theme with glass in the repeating theme rotation: glass 1–3, metal 4–6, stone 7–9. Glass uses reflective blue translucent floor and walls; friction and collisions are unchanged. Healing circles add 25 health, capped at 100, once per run, and remain unused at full health.

0.2.12 makes the health meter green with a white percentage and cycles all four supplied electric wall-hit sounds, including electricity-sound-6066 and electric_zap_001-6374. Effects/master volume, mute, pause and retrigger cooldown still apply.

0.2.13 replaces plain obstacle meshes with dark round/oval bomb casings, brass fuse caps, bent rope fuses and glowing ember tips. Existing colliders and invisible proximity triggers are unchanged.
