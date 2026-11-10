# Version 0.2 baseline inspection

Baseline 0.1.9 builds with zero warnings/errors, .NET SDK 10.0.403 / net9.0 / Godot.NET.Sdk 4.7.2. Godot 4.7.2 .NET and Android SDK/templates/JDK configured; Pixel 7a detected by adb. Backup: work/TiltLabyrinth-before-v02.zip in Codex chat workspace.

Preserve: gravity/accelerometer display-mapped sensor input, full-vector calibration, sensitivity 6.5, dead zone .015, 35ms filter; radius 1.1, mass 1, acceleration 27, speed cap 12, linear/angular damping .4/.25, friction .7, bounce .04, CCD. Fixed orthographic camera, Android-only export ARM64, four audio buses and user audio.cfg.

Baseline defects relevant to the brief: hard-coded baffles/holes; forced fall capture disables collision and damps velocity near square openings; no progression/checkpoints/progress save; no circular openings; static global goal event; several historical docs. v0.2 will replace only level geometry/lifecycle and hole-specific capture logic. No sensor/calibration changes or force/damping retuning. Genuine circular openings with a generous aperture replace forced lip capture so grazing a rim is safe.
