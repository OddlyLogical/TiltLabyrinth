# Android export and installation

APK: `D:\Repos\TiltLabyrinth\Builds\TiltLabyrinth-v0.2.9-debug.apk`

Godot preset: Android / ARM64 only / Mobile renderer / landscape / debug signing / package `com.tiltlabyrinth.game`. Internet permissions disabled. VIBRATE permission enabled for the requested collision haptics. Existing signing identity is reused so `install -r` preserves local audio/progress settings.

## Pixel 7a installation

Enable Developer options and USB debugging, unlock the phone, connect a data-capable USB cable and approve the PC when prompted.

```powershell
$adb = "$env:LOCALAPPDATA\Android\Sdk\platform-tools\adb.exe"
& $adb devices
& $adb install -r D:\Repos\TiltLabyrinth\Builds\TiltLabyrinth-v0.2.9-debug.apk
& $adb shell am start -n com.tiltlabyrinth.game/com.godot.game.GodotAppLauncher
```

The v0.2 debug APK was successfully exported, verified, installed and launched on the connected Pixel 7a. Installation is for local testing, not Play Store release signing.

## Haptics

In the game's Audio menu, enable **Wall impact vibration**. Android must also allow vibration globally. During testing, the Pixel's vibrator service reported `vibrateOn = false` and marked the game's actual wall-hit pulses `ignored_for_settings`. Enabling **Settings → Sound & vibration → Vibration & haptics → Use vibration & haptics** changed the global state to true. App permission was already granted. Later Android history confirmed these game vibration effects finished successfully after the global setting was enabled; comfort/strength is user-dependent.

## Toolchain on another PC

Install matching Godot 4.7.2 .NET and Mono export templates, .NET SDK 9 or later, a compatible JDK (17 recommended; 21 used here), and Android SDK. Set Editor Settings → Export → Android paths to the JDK and SDK. The installed environment now builds successfully; retain net9.0 for the standard export template. See https://docs.godotengine.org/en/4.7/tutorials/export/exporting_for_android.html for baseline package setup.

## Healing circles (0.2.3)

Checkpoints are replaced by green circles with a white medical cross. Entering one below 100% restores health completely, plays a soft chime, and dims the used circle. Each circle works once per run; touching it at full health does not consume it. Restart/Replay restores all circles. They never change the respawn location. All hole falls return to the level start, preserving health, remaining time and maze seed.
