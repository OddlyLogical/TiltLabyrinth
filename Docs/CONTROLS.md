# Controls, levels and feedback

- Hold the Pixel face-up in landscape, tap Calibrate and remain still for half a second. Calibration is retained through replay, respawn and next level in the current session.
- Tilt left/right and lower the near/far edge to roll. Neutral decelerates; reverse tilt counters momentum. Physics tuning stays constant at every level.
- Blue disk: start. Green illuminated bullseye marked EXIT: goal; the ball's center must reach it. Copper rings: actual circular holes. Green circles with a white cross restore health.
- Falling pauses the timer and increments Falls once. After the physical drop and a short delay, the ball returns to the level start with zero velocity. The layout and seed stay unchanged.
- Restart or Replay Level resets the run, timer, fall count and checkpoint activations without changing the seed. Next Level creates a fresh layout after victory.
- Pause/Resume holds physics, timer and audio. Android Back pauses the game. App focus loss/backgrounding also pauses; resume explicitly on return.
- Audio adjusts Music, Ambience and Effects independently, Master mute, and Wall impact vibration. Preferences save locally. Music is not restarted on level transitions.

Sensor settings are unchanged: sensitivity 6.5, dead zone .015, 35ms low-pass filter, gravity with accelerometer fallback. No touchscreen directions or desktop movement controls.

Save files: `user://progress.cfg` (highest unlocked level, current level/seed, best times) and `user://audio.cfg` (audio/haptic preferences). Missing/corrupt progress falls back safely to level 1. Progress writes use temporary files and replacement. Replay is deterministic; initial level/seed can be restored for debugging from this save file. Runtime logs also print the seed.

## Electric walls, proximity bombs and countdown (0.2.1)

Each level/replay starts at 100% health. Wall impacts cost 10 percentage points, with a 0.45-second shock cooldown. Floor and ordinary rim contacts cause no electrical damage. Cyan animated wall strips mark live walls. Orange rings mark obstacle danger; moving within 0.15 units of surface-to-ball clearance detonates the obstacle once, costing 25 health. Exploded obstacles reset only on level replay/restart.

At zero health, Game Over stops the run. Countdown allowance is max(120 seconds, route cells × 8 seconds); expiry also ends the run. Pause, calibration and falling do not consume time. Hole/checkpoint respawns retain health and remaining time. Restart refills health/time and keeps the seed; Next Level also starts full.

## Healing circles (0.2.3)

Checkpoints are replaced by green circles with a white medical cross. Entering one below 100% adds 25 health points, capped at 100%, plays a soft chime, and dims the used circle. Each circle works once per run; touching it at full health does not consume it. Restart/Replay restores all circles. They never change the respawn location. All hole falls return to the level start, preserving health, remaining time and maze seed.

0.2.8 makes the exit a larger gold illuminated bullseye on a dark disk with a clear EXIT label. Green medical-cross circles remain healing pickups. Goal capture and collisions are unchanged.

0.2.9 adds a spinning, shrinking visual sink and expanding ripples for real hole falls. Reaching the green EXIT target starts a 1.3-second swirling drop before completion. Ball collisions/forces and actual hole falling stay unchanged; only the visual child animates. Camera, level timer and health are held during the exit sequence. Restart restores the full ball visual.

0.2.10 adds invisible danger zones to black holes. When ball-to-rim clearance reaches 0.15 units, the hole captures the ball, pulls the visual inward and plays the animated fall. No permanent danger ring is shown. This supersedes the earlier rim-grazing-only behavior. Safe route validation includes the expanded capture radius.

0.2.11 replaces the wood theme with glass in the repeating theme rotation: glass 1–3, metal 4–6, stone 7–9. Glass uses reflective blue translucent floor and walls; friction and collisions are unchanged. Healing circles add 25 health, capped at 100, once per run, and remain unused at full health.
