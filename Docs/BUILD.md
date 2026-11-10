# Build version 0.2

Validated November 8, 2026 with Godot `4.7.2.stable.mono.official.ed1daf0bf`, .NET SDK 10.0.403 targeting net9.0, installed Android SDK/build-tools 36.0.0, matching Mono export templates and the configured JDK 21.0.8. No missing build dependencies remain on this PC.

```powershell
Set-Location D:\Repos\TiltLabyrinth
dotnet build TiltLabyrinth.sln --configuration Debug
$godot = 'D:\Downloads\Godot_v4.7.2-stable_mono_win64\Godot_v4.7.2-stable_mono_win64\Godot_v4.7.2-stable_mono_win64_console.exe'
& $godot --headless --editor --path . --import --quit
& $godot --headless --path . res://Tools/GenerationValidation.tscn
& $godot --headless --path . res://Tools/Validation.tscn
& $godot --headless --path . --export-debug Android Builds/TiltLabyrinth-v0.2.9-debug.apk
```

Compilation passed after generator, progression, visual materials, hazards, lifecycle/UI/audio/persistence and final integration phases. Latest build has zero warnings/errors. Tests are C# Godot scenes in Tools; export excludes Tools/Docs resources. Test-only sensor availability injection never adds desktop movement input to the game.

The ignored local `NuGet.Config` points to this editor's `GodotSharp\Tools\nupkgs`. Portable sources omit this machine-specific file; obtain Godot.NET.Sdk 4.7.2 from NuGet or configure that same folder in a matching installed editor as a NuGet package source. No signing credentials are included.

Regenerate audio: `py -3 Tools/GenerateAudio.py` and `py -3 Tools/GenerateFeedback.py`, then reimport in Godot. All audio is original mathematical synthesis.

Developer diagnostics: the debug APK logs level seed and periodic FPS/physics/draw counts. The camera only reframes in BuildLevel, never while playing or respawning.

## Healing circles (0.2.3)

Checkpoints are replaced by green circles with a white medical cross. Entering one below 100% restores health completely, plays a soft chime, and dims the used circle. Each circle works once per run; touching it at full health does not consume it. Restart/Replay restores all circles. They never change the respawn location. All hole falls return to the level start, preserving health, remaining time and maze seed.
