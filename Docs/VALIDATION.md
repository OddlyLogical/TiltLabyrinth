# Version 0.2 validation — November 8, 2026

## Automated checks actually run

- Baseline 0.1.9 and all implementation phase compilations succeeded. Final Debug build: **0 warnings, 0 errors**, Godot.NET.Sdk 4.7.2 / net9.0.
- GenerationValidation: **600 seeds**, 100 in each of six difficulty tiers (levels 1, 3, 5, 8, 11, 16). Every seed checked both without hazards and with final placements, plus exact regeneration. Total 1,200 clearance/connectivity layout validations. Checks cover boundary closure and symmetry, valid start/goal, swept-ball route clearance, obstacle bounds, safe circular openings and reachable checkpoints.
- Integration Validation: **30 checks** covering angled calibration, directional response, moving calibration rejection, missing/corrupt saves, progress/best-time persistence, Ready→Playing, momentum/rotation, deceleration, Android Back handling, pause, full-speed boundary collision, real checkpoint entry, safe hole-rim graze, physical drop, timer pause, delayed checkpoint respawn, fixed camera/seed, restart reset, goal edge/center behavior, replay, next level with preserved calibration, haptic scaling, preference persistence and all effect resources.
- Sensor/calibration source restored byte-for-byte from the backup and unchanged; ball tuning retained. Ball changes limited to level-aware reset and documented removal of forced hole capture.
- Android debug export signed and verified successfully; installed and launched on Pixel 7a. Package includes ARM64, Mobile/Vulkan and VIBRATE permission, with no internet permission.

## Physical Pixel 7a observations

Connected device identified as Pixel_7a. Installed APK launched in landscape. Screenshots reviewed at level 1 (wood), 5 (metal) and 8 (stone); board, HUD, hazards, checkpoints and steel ball visible. First visual pass revealed dark hole-cell shading; triangle winding was corrected before the final package.

Brief debug samples on the Pixel reported **60 FPS** at levels 1, 5 and 8, physics about **4.2–4.8ms**, and **15–34 draw calls**. These are short samples, not sustained thermal/battery benchmarks. A first expert-level sample was interrupted by Back closing the app; QuitOnGoBack is now disabled so Back pauses. The final expert sample completed: level 16 reported **60 FPS**, **6.94ms physics**, **54 draw calls**. A final level-5 sample reported **60 FPS**, **2.80ms physics**, **29 draws**. Corrected circular floor shading was visually reviewed in screenshots at levels 5 and 16.

Haptic requests from actual wall contacts were present in Android's vibrator history with granted VIBRATE permission, but initially marked **ignored_for_settings** because global vibration was off. The phone's global vibration setting was then observed enabled. After enabling the phone setting, later Android vibrator history showed the game’s actual wall-hit pulses as **finished**, including 23–38ms requested pulses. This verifies OS execution; subjective strength/comfort still needs the user’s feedback.

Temporary later-level save data used for device inspection was backed up and restored; normal progress and existing audio preferences were retained.

## Remaining manual verification

Complete full start-to-goal runs across all tiers on the phone; assess small-tilt feel, fall/respawn fairness at speed, all four axis directions, repeated calibration, audio mix and focus interruptions (calls/competing audio), checkpoint/next-level touch flow, vibration comfort, and sustained thermal performance. Automated headless checks do not count as physical playtests. No claim is made that these manual checks all passed.

## Diagnostics and limitations

- The deliberate malformed-save test prints a ConfigFile parse error before safe recovery; the test passes.
- Some forced headless shutdowns report native audio/resource instances still in use, as the baseline did. No gameplay crash was observed from these shutdown diagnostics.
- The engine's command-line exporter emits `EditorSettings not instantiated ... shutdown_adb_on_exit` after successful signing/verification. APK generation and installation succeed.
- No moving hazards; square-grid suggestions adapted to landscape rectangular grids. Guaranteed route avoids holes; holes make risky branches hazardous rather than unavoidable. Themes keep physical friction unchanged.

Logs and original rollback ZIP are retained under this chat workspace's `work/`. The portable source archive omits credentials, generated caches and machine-specific NuGet.Config.

## Final package

Sensor/calibration files verified byte-identical to the rollback ZIP. APK contents verified ARM64-only, VIBRATE present and INTERNET absent. APK signed/verified, installed and launched; current level/seed restored after test-level sampling. No compile errors or shader errors in final checks. Short device samples do not establish sustained performance.

## 0.2.1 gameplay extension

Electric wall classification uses collider group identity so rotating ball coordinates do not misclassify wall contacts as floor contacts. Contact impulse is included to detect first-frame/full-speed impacts. Existing forces, mass, damping, radius and speed unchanged. Added health, electric traces/shock flash, GameOver, proximity obstacle blast state/effect/audio, countdown, and loss/restart handling. Integration now checks initial/full health, exact wall damage, respawn health retention, zero-health loss, stopped timer, actual loss-panel restart, once-only explosion damage, countdown expiry/refill, and the explosion audio resource.

Final 0.2.1 checks: 42 integration checks passed; 600 seeds again validated before/after hazards. Android export signed/verified; installed and launched on Pixel 7a. Physical shock/blast/countdown feel still needs playtesting.

## 0.2.2 movement fix

Phone screenshot showed an initial failed calibration leaving Ready indefinitely. Ready now retries initial calibration every 0.4 seconds after failure, using the unchanged sensor/calibration algorithm. Status explicitly requests a comfortable still hold. Player ball CanSleep is disabled to keep tilt force integration responsive after idle. No force/speed/radius retuning. Regression covers failed-calibration retry, 150 idle frames without sleeping, and actual tilt-force movement after idle. All 45 integration checks pass; build has zero warnings/errors.

## 0.2.3 healing-circle replacement

Replaced Checkpoint with HealingCircle and renamed generation placement data to HealingCells. Geometry/placement/seed stay unchanged. Healing callback restores health to 100 only while playing and damaged; consumes once and changes visual state, with the former gentle checkpoint chime reused. Full-health overlap remains available. Respawn location remains level start. Integration tests cover full heal, no repeat heal, unused circles after Restart, full-health preservation, and post-fall return to start with health retained.

Final 0.2.3 result: 47 integration checks passed and all 600 seeded layouts passed clearance/connectivity validation before and after hazards. Build: zero compiler warnings/errors.

## 0.2.4 supplied wall-shock audio

shock.mp3 is loaded as AudioStreamMP3 on the Effects bus. Audio loading now accepts AudioStream while preserving WAV loop setup. WallImpact plays the supplied MP3 and suppresses the ordinary clink for electrical contacts, with a 250ms retrigger cooldown. Master mute/Effects slider and pause handling include shock playback. MP3 resource/length/bus regression and all 48 integration checks pass. Build has zero warnings/errors. Listening/mix balance remains for user confirmation.

## 0.2.5 metallic background ambience

The supplied soundreality-metallic-ambience-593615.mp3 replaces the wind loop on Ambience. AudioStreamMP3 looping is enabled with offset zero, before playback. Existing volume, mute and pause apply; the persistent AudioManager does not restart it on level changes. Import/build and all 49 integration checks pass, including looping/background playback after level transitions. Original MP3 left intact. Actual end-to-start audio smoothness and listening balance require user feedback.

## 0.2.6 second wall-zap sound

Added the user-provided freesound_community-electrical-shock-zap-106412.mp3 alongside shock.mp3. Accepted wall shocks alternate players, retain intensity-based volume, suppress ordinary clinks, and obey the shared 250ms cooldown, Effects/master controls and pause. Both MP3 resources import; build has no compiler warnings/errors; all 50 integration checks pass. The build also retains the 0.2.5 metallic ambience loop. Phone was disconnected during preparation; installation remains pending.

## 0.2.7 explosion sounds and hidden boundary

Bombs now alternate Universfield and Dragon Studio user MP3 explosion clips. Both import onto Effects and obey the existing volume/mute/pause controls. All 52 integration checks pass, including once-only detonation and proximity damage. Removed only the decorative TorusMesh danger marker from StaticBumper; the entire proximity-detection function was verified text-identical before/after this visual change. The old marker edge remains the invisible trigger boundary.

## 0.2.8 distinctive exit

Visual-only exit replacement: larger dark disk, gold luminous double target rings/center and camera-facing EXIT label. Healing circles stay green. GoalTrigger and capture distance unchanged. Compilation passes without warnings/errors; main-scene headless load resolves the new marker and label. Device appearance to be checked after installing the APK.

## 0.2.9 animated hole and exit drops

Ball rendering moved under a visual child, animated with spiral motion, shrinking/spinning and eased vertical sink. Genuine falling still comes from rigid-body penetration through the physical opening. Respawn waits for visual completion. Exiting state freezes controls/timer and waits for the goal drop before saving and showing completion; no duplicate goal events. Green EXIT marker remains labeled and distinct from medical-cross healing circles. Added visual-progress and fixed-camera/timer regression checks. All 55 integration checks and compilation pass. Physical-device animation quality requires user feedback.

## 0.2.10 black-hole proximity danger

HoleHazard checks center distance against aperture radius + ball radius + 0.15. Capture releases collision support, sets falling once and uses a brief inward-pull phase before visual sinking. Reset restores collision masks and full visuals. Outside-zone and inside-zone regressions pass as part of all 55 integration checks. Validator now explicitly accounts for the capture boundary plus margin. This is an intentional user-requested change from natural rim-only falling.

## 0.2.11 glass theme and +25 healing

Wood replaced by Glass with translucent reflective floor/wall materials. Level rotation/data/layouts remain otherwise unchanged. Healing adds 25 instead of restoring all health, capped at 100; full-health contact still preserves pickup. Updated tests cover 60→85, 90→100, no repeat use and Glass material rotation. Build has no warnings/errors; all 57 integration checks pass.

0.2.11 APK signed/verified, installed and launched. Glass level 1 screenshot visually reviewed on Pixel 7a: translucent blue floor/walls and electric rim visible, no wood texture. Brief sample: 60 FPS, 3.34ms physics, 23 draw calls. Original phone progress restored after preview.

## 0.2.12 green health and four electric sounds

Green ProgressBar fill and dark green track with outlined white percentage. AudioManager uses a four-player wall-zap rotation with shared cooldown; all four stop/pause/clean up consistently. Two new user MP3 imports and Effects-bus routing verified. Build: zero warnings/errors. All 59 integration checks pass.

## 0.2.13 recognizable bomb visuals

Visual-only replacement of cylinder/box obstacle meshes with bomb casing, cap, fuse and ember meshes. Collision shapes, damage, proximity formulas and explosion state unchanged. Compile and main-scene load pass. No new behavior tests added for this reversible visual change. Phone appearance/performance checked after install when connected.

## Version 0.2.14

Build passed with zero warnings/errors. After varying hole radii and widening cells, all 600 seeded generation layouts passed connectivity, physical route clearance, hazard safety and determinism checks. Integration validation: 59 checks passed. Visual update includes bomb-size-scaled explosion effects, procedural surface textures and static board scenery.

## Version 0.2.15

61 integration checks passed, including sustained stationary wall contact and stopping damage after release. All 600 seeded layouts passed with the larger ball and variable hole diameters. Build: zero warnings/errors. APK installed successfully on the connected Pixel 7a.

## Version 0.2.16

Build passed with zero warnings/errors. 62 integration checks passed, including imported rock rolling MP3, loop enabled and Effects bus routing.

## Version 0.2.17

Build passed with zero warnings/errors. 62 integration checks passed, including supplied tectonic rolling MP3 loop and Effects bus routing. Stone material uses local procedural color/normal textures with no metal seam; physics unchanged.
