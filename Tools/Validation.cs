using Godot;
using System;
using System.Linq;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
namespace TiltLabyrinth;
public partial class Validation : Node3D
{
    private int checks;
    private void Check(bool pass,string message){if(!pass)throw new Exception(message);checks++;GD.Print("PASS: "+message);}
    private async Task Frames(int frames){for(int i=0;i<frames;i++)await ToSignal(GetTree(),SceneTree.SignalName.PhysicsFrame);}
    private static T Child<T>(Node root) where T:Node=>root.GetChildren().OfType<T>().First();
    private static Button FindButton(Node root,string text)
    {
        if(root is Button b && b.Text==text)return b;
        foreach(Node child in root.GetChildren()){var found=FindButton(child,text);if(found!=null)return found;}
        return null!;
    }
    private static void Press(Node root,string text)=>FindButton(root,text).EmitSignal(Button.SignalName.Pressed);
    public override async void _Ready()
    {
        string testPath="user://validation-progress.cfg";GameManager? game=null;
        try
        {
            var glass=ThemeManager.Create(1);var glassAgain=ThemeManager.Create(10);
            Check(glass.Name=="Glass" && glassAgain.Name=="Glass" && glass.Wall.Transparency==BaseMaterial3D.TransparencyEnum.AlphaDepthPrePass && glass.Floor is StandardMaterial3D gf && gf.Transparency==BaseMaterial3D.TransparencyEnum.AlphaDepthPrePass,"glass replaces wood in repeating theme rotation");
            var calibration=new TiltCalibration();calibration.Begin();Vector3 neutral=new Vector3(0,-4,-9).Normalized();
            for(int i=0;i<32;i++)calibration.Sample(neutral*9.81f,1f/60);
            Check(calibration.Ready && calibration.Transform(neutral).DistanceTo(Vector3.Forward)<.001f,"angled neutral calibration");
            Check(calibration.Transform(neutral.Rotated(Vector3.Up,.15f)).DistanceTo(Vector3.Forward)>.1f,"calibration preserves directional response");
            calibration.Begin();for(int i=0;i<32;i++)calibration.Sample((i%2==0?Vector3.Left:Vector3.Right)*9.81f,1f/60);
            Check(!calibration.Collecting && calibration.Message.Contains("moved"),"moving calibration rejected");
            string physical=ProjectSettings.GlobalizePath(testPath);if(File.Exists(physical))File.Delete(physical);
            var save=new SaveManager(testPath);save.Load();Check(save.CurrentLevel==1 && save.HighestUnlocked==1,"missing save defaults safely");
            save.Complete(4,40);save.Select(5,7);save.Complete(5,35);save.Complete(5,50);
            var loaded=new SaveManager(testPath);loaded.Load();Check(loaded.HighestUnlocked==6 && loaded.CurrentLevel==5 && loaded.CurrentSeed==7 && loaded.BestTimes[5]==35,"progress and best time persistence");
            File.WriteAllText(physical,"[broken save!!!");loaded.Load();Check(loaded.RecoveredCorruptSave && loaded.CurrentLevel==1,"corrupt save recovery");
            File.Delete(physical);save=new SaveManager(testPath);save.Load();save.Complete(4,40);
            int seed=Enumerable.Range(1,100).First(s=>MazeGenerator.Generate(5,s).HealingCells.Count>0 && MazeGenerator.Generate(5,s).Holes.Count>0);save.Select(5,seed);
            game=new GameManager {SavePath=testPath};AddChild(game);await Frames(6);
            var tilt=Child<AndroidTiltInput>(game);var ball=Child<BallController>(game);var board=Child<MazeBuilder>(game);var camera=Child<Camera3D>(game);var hud=Child<HUDController>(game);
            // Sensor availability injection exists only in this separate test scene;
            // gameplay has no desktop inputs and sensor code remains untouched.
            typeof(AndroidTiltInput).GetProperty(nameof(AndroidTiltInput.Available))!.SetValue(tilt,true);
            tilt.Calibration.Begin();for(int i=0;i<32;i++)tilt.Calibration.Sample((i%2==0?Vector3.Left:Vector3.Right)*9.81f,1f/60);
            await Frames(32);Check(tilt.Calibration.Collecting && game.State==GameState.Ready,"failed initial calibration retries automatically");
            for(int i=0;i<32;i++)tilt.Calibration.Sample(Vector3.Forward*9.81f,1f/60);
            await Frames(6);Check(game.State==GameState.Playing,"Ready enters Playing after calibration and reset");
            Check(game.Health==100,"level begins at 100 percent health");
            Transform3D cameraStart=camera.Transform;
            Vector3 start=ball.Position;ball.ApplyCentralImpulse(new Vector3(2,0,0));await Frames(8);float speed=ball.HorizontalSpeed;
            Check(ball.Position.DistanceTo(start)>.05f && speed>.05f && ball.AngularVelocity.Length()>.01f,"physical momentum and visible rolling rotation");await Frames(50);Check(ball.HorizontalSpeed<speed,"neutral deceleration retained");
            Check(!GetTree().QuitOnGoBack,"Android Back pauses rather than automatically quitting");
            double beforePause=game.Elapsed;Press(hud,"Pause");Vector3 pausePosition=ball.Position;await Frames(20);
            Check(game.State==GameState.Paused && game.Elapsed==beforePause && ball.Position.DistanceTo(pausePosition)<.001f,"pause freezes timer and physics");Press(hud,"Resume");await Frames(4);
            // A closed outer boundary at the start cell blocks a full-speed impact.
            Vector3 edge=board.Grid.Spawn(board.Grid.StartCell);edge.X=-board.Grid.Width/2+1.35f;ball.Position=edge;ball.LinearVelocity=new Vector3(-12,0,0);await Frames(25);
            Check(ball.Position.X>-board.Grid.Width/2+1.2f,"matched boundary collision contains ball at speed cap");
            Check(game.Health==90,$"electric wall impact removes ten percent health once (actual {game.Health})");
            int healthAfterHit=game.Health;
            tilt.SetPhysicsProcess(false);
            typeof(AndroidTiltInput).GetProperty(nameof(AndroidTiltInput.Tilt))!.SetValue(tilt,new Vector2(-1,0));
            await Frames(75);
            Check(game.Health<healthAfterHit-10,"sustained wall contact repeats electrical damage without a new impact");
            ball.Position=board.Grid.Spawn(board.Grid.StartCell);ball.LinearVelocity=Vector3.Zero;
            typeof(AndroidTiltInput).GetProperty(nameof(AndroidTiltInput.Tilt))!.SetValue(tilt,Vector2.Zero);
            await Frames(35);int releasedHealth=game.Health;await Frames(35);
            Check(game.Health==releasedHealth,"moving away from wall stops electrical damage");tilt.SetPhysicsProcess(true);
            typeof(GameManager).GetProperty(nameof(GameManager.Health))!.SetValue(game,60);
            var healing=board.HealingCircles[0];Vector3 healingPosition=new(healing.GlobalPosition.X,ball.Radius+.04f,healing.GlobalPosition.Z);ball.Position=healingPosition;ball.LinearVelocity=Vector3.Zero;ball.AngularVelocity=Vector3.Zero;ball.Sleeping=false;await Frames(8);
            Check(healing.IsConsumed && game.Health==85,"green healing circle adds 25 health points");
            edge=board.Grid.Spawn(board.Grid.StartCell);edge.X=-board.Grid.Width/2+1.35f;ball.Position=edge;ball.LinearVelocity=new Vector3(-12,0,0);ball.Sleeping=false;await Frames(45);
            ball.Position=healingPosition;ball.LinearVelocity=Vector3.Zero;ball.Sleeping=false;await Frames(8);
            Check(game.Health==75,"used healing circle cannot restore health repeatedly");healthAfterHit=game.Health;
            HoleSpec hole=board.Grid.Holes[0];
            // A rim graze remains supported, with collisions fully enabled.
            ball.Position=new Vector3(hole.Center.X+hole.Radius+ball.Radius+.4f,ball.Radius+.1f,hole.Center.Y);ball.LinearVelocity=Vector3.Zero;await Frames(35);
            Check(!ball.Falling && ball.CollisionLayer==2 && ball.CollisionMask==1,"outside black-hole danger zone remains safe");
            ball.Position=new Vector3(hole.Center.X+hole.Radius+ball.Radius+.1f,ball.Radius+.04f,hole.Center.Y);ball.LinearVelocity=Vector3.Zero;ball.AngularVelocity=Vector3.Zero;ball.Sleeping=false;
            for(int i=0;i<100 && game.State!=GameState.Falling;i++)await Frames(1);
            Check(game.State==GameState.Falling && ball.Falling && ball.Position.Y>ball.Radius*.4f,"black-hole danger zone captures before rim contact");
            Vector3 fallDisplay=ball.DisplayPosition;double fallTime=game.Elapsed;await Frames(10);Check(game.Elapsed==fallTime,"falling pauses timer");
            Check(ball.DropAnimating && ball.DisplayPosition.DistanceTo(fallDisplay)>.01f,"hole fall visibly spins and sinks before respawn");
            await Frames(150);Check(game.Falls==1 && game.State==GameState.Playing && ball.Position.DistanceTo(board.Grid.Spawn(board.Grid.StartCell))<.2f && !ball.Falling,"hole fall returns to level start with cleared motion");
            Check(game.Health==healthAfterHit,"fall respawn preserves remaining health");
            Check(board.Grid.Seed==seed && camera.Transform.IsEqualApprox(cameraStart),"seed, geometry and camera remain fixed across fall");
            string fingerprint=board.Grid.Fingerprint();Press(hud,"Restart");await Frames(8);board=Child<MazeBuilder>(game);
            Check(game.Falls==0 && game.Elapsed<1 && board.Grid.Fingerprint()==fingerprint && !board.HealingCircles[0].IsConsumed,"Restart replenishes healing circles with identical seed/layout");
            var freshHealing=board.HealingCircles[0];ball.Position=new(freshHealing.GlobalPosition.X,ball.Radius+.04f,freshHealing.GlobalPosition.Z);ball.LinearVelocity=Vector3.Zero;ball.Sleeping=false;await Frames(8);
            Check(!freshHealing.IsConsumed && game.Health==100,"full-health contact leaves healing circle available");
            Vector2 goal=board.Grid.Center(board.Grid.GoalCell);ball.Position=new(goal.X-1.25f,ball.Radius+.04f,goal.Y);ball.LinearVelocity=Vector3.Zero;ball.Sleeping=false;await Frames(6);
            Check(game.State==GameState.Playing,"goal edge contact does not complete level");ball.Position=new(goal.X,ball.Radius+.04f,goal.Y);await Frames(8);
            Check(game.State==GameState.Exiting && ball.DropAnimating && !ball.Active,"goal starts exit drop before completion");
            double exitTime=game.Elapsed;Vector3 exitDisplay=ball.DisplayPosition;await Frames(20);
            Check(game.Elapsed==exitTime && ball.DisplayPosition.Y<exitDisplay.Y && camera.Transform.IsEqualApprox(cameraStart),"exit drop sinks while timer and camera remain fixed");await Frames(85);
            Check(game.State==GameState.LevelComplete && game.Levels.Save.HighestUnlocked>=6,"exit drop completes and saves progress once");
            Press(hud,"Replay Level");await Frames(8);Check(game.Levels.Current.Fingerprint()==fingerprint && game.State==GameState.Playing,"Replay Level retains seed and calibration");
            board=Child<MazeBuilder>(game);goal=board.Grid.Center(board.Grid.GoalCell);ball.Position=new(goal.X,ball.Radius+.04f,goal.Y);ball.LinearVelocity=Vector3.Zero;ball.Sleeping=false;await Frames(100);Press(hud,"Next Level");await Frames(8);
            Check(game.Levels.Current.Level==6 && game.Levels.Current.Seed!=seed && game.State==GameState.Playing && tilt.Calibration.Ready,"Next Level advances seed and preserves calibration");
            Check(game.Health==100,"next level restores full health");
            board=Child<MazeBuilder>(game);
            for(int hit=0;hit<12 && game.State==GameState.Playing;hit++)
            {
                Vector3 wallHit=board.Grid.Spawn(board.Grid.StartCell);wallHit.X=-board.Grid.Width/2+1.35f;
                ball.Position=wallHit;ball.LinearVelocity=new Vector3(-12,0,0);ball.AngularVelocity=Vector3.Zero;ball.Sleeping=false;
                await Frames(45);
            }
            Check(game.Health==0 && game.State==GameState.GameOver && !ball.Active,"repeated electric hits end the run at zero health");
            double lostTime=game.Elapsed;await Frames(20);Check(game.Elapsed==lostTime,"Game Over stops timer");
            var lossPanel=hud.GetChildren()[0].GetChildren().OfType<PanelContainer>().First();
            FindButton(lossPanel,"Restart").EmitSignal(Button.SignalName.Pressed);await Frames(8);
            Check(game.Health==100 && game.State==GameState.Playing && !ball.Freeze && !ball.Falling,"Game Over panel Restart restores playable full-health run");
            board=Child<MazeBuilder>(game);var bomb=board.Obstacles.First(o=>o.Spec.Bumper);
            Vector2 inward=(board.Grid.Center(bomb.Spec.Cell)-bomb.Spec.Center).Normalized();
            Vector2 near=bomb.Spec.Center+inward*(bomb.Spec.HalfSize.X+ball.Radius+.08f);
            ball.Position=new(near.X,ball.Radius+.04f,near.Y);ball.LinearVelocity=Vector3.Zero;ball.AngularVelocity=Vector3.Zero;ball.Sleeping=false;
            await Frames(8);Check(bomb.HasExploded && game.Health==75,"nearby obstacle explodes once and removes 25 percent health");
            await Frames(12);Check(game.Health==75,"destroyed obstacle cannot explode repeatedly");
            typeof(GameManager).GetProperty(nameof(GameManager.TimeLimit))!.SetValue(game,game.Elapsed+.05);
            await Frames(20);Check(game.State==GameState.GameOver && game.RemainingTime==0,"countdown expiry ends run");
            Press(hud,"Restart");await Frames(8);Check(game.State==GameState.Playing && game.Health==100 && game.RemainingTime>110,"Restart restores countdown and health");
            tilt.SetPhysicsProcess(false);typeof(AndroidTiltInput).GetProperty(nameof(AndroidTiltInput.Tilt))!.SetValue(tilt,Vector2.Zero);
            await Frames(150);Check(!ball.Sleeping && !ball.CanSleep,"player ball stays awake while idle");
            Vector3 idlePosition=ball.Position;typeof(AndroidTiltInput).GetProperty(nameof(AndroidTiltInput.Tilt))!.SetValue(tilt,new Vector2(.3f,0));await Frames(15);
            Check(ball.Position.DistanceTo(idlePosition)>.05f && ball.HorizontalSpeed>.1f,"tilt force moves ball after prolonged idle");
            typeof(AndroidTiltInput).GetProperty(nameof(AndroidTiltInput.Tilt))!.SetValue(tilt,Vector2.Zero);tilt.SetPhysicsProcess(true);
            // Choose a known healing layout rather than depending on the random Next Level seed.
            typeof(GameManager).GetMethod("BuildLevel",BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(game,new object[]{MazeGenerator.Generate(5,seed)});
            await Frames(8);board=Child<MazeBuilder>(game);
            typeof(GameManager).GetProperty(nameof(GameManager.Health))!.SetValue(game,90);
            var cappedHeal=board.HealingCircles[0];ball.Position=new(cappedHeal.Position.X,ball.Radius+.04f,cappedHeal.Position.Z);ball.LinearVelocity=Vector3.Zero;ball.Sleeping=false;await Frames(8);
            Check(game.Health==100 && cappedHeal.IsConsumed,"healing caps at 100 percent");
            var weak=HapticFeedback.Response(.1f);var medium=HapticFeedback.Response(2);var strong=HapticFeedback.Response(8);
            Check(weak.Duration==0 && strong.Duration>medium.Duration && strong.Amplitude>medium.Amplitude,"haptic threshold and impact scaling");
            var audio=Child<AudioManager>(game);float music=audio.Settings.Music;bool originalHaptics=audio.Settings.Haptics;audio.Settings.Music=.37f;audio.Settings.Haptics=false;audio.Settings.Save();var audioReload=new AudioSettings();audioReload.Load();Check(Mathf.IsEqualApprox(audioReload.Music,.37f)&&!audioReload.Haptics,"audio and haptic preference persistence");audio.Settings.Music=music;audio.Settings.Haptics=originalHaptics;audio.Settings.Save();audio.Apply();
            foreach(string clip in new[]{"fall","checkpoint","impact","victory","roll","explosion"})Check(GD.Load<AudioStreamWav>($"res://Audio/Effects/{clip}.wav")!=null,$"audio resource {clip} resolves");
            var rollingPlayer=ball.GetChildren().OfType<AudioStreamPlayer3D>().First(a=>a.Bus=="Effects");
            Check(rollingPlayer.Stream is AudioStreamMP3 rock && rock.ResourcePath.EndsWith("krnbeatz-tectonic-passage-418836.mp3") && rock.Loop && rock.GetLength()>0,"supplied rock rolling MP3 imports and loops on Effects bus");
            var suppliedShock=GD.Load<AudioStreamMP3>("res://Audio/Effects/shock.mp3");
            Check(suppliedShock!=null && suppliedShock.GetLength()>0 && audio.GetChildren().OfType<AudioStreamPlayer>().Any(a=>a.Bus=="Effects" && a.Stream is AudioStreamMP3),"user shock MP3 imports and plays through Effects bus");
            var background=audio.GetChildren().OfType<AudioStreamPlayer>().First(a=>a.Bus=="Ambience");
            Check(background.Stream is AudioStreamMP3 ambient && ambient.Loop && ambient.GetLength()>0 && background.Playing,"supplied MP3 ambience loops continuously across level transitions");
            var secondZap=GD.Load<AudioStreamMP3>("res://Audio/Effects/freesound_community-electrical-shock-zap-106412.mp3");
            Check(secondZap!=null && secondZap.GetLength()>0 && audio.GetChildren().OfType<AudioStreamPlayer>().Any(a=>a.Bus=="Effects" && a.Stream==secondZap),"second supplied wall zap imports on Effects bus");
            var suppliedExplosion=GD.Load<AudioStreamMP3>("res://Audio/Effects/universfield-epic-cinematic-explosion-454857.mp3");
            Check(suppliedExplosion!=null && suppliedExplosion.GetLength()>0 && audio.GetChildren().OfType<AudioStreamPlayer>().Any(a=>a.Bus=="Effects" && a.Stream==suppliedExplosion),"supplied bomb explosion MP3 imports on Effects bus");
            var secondExplosion=GD.Load<AudioStreamMP3>("res://Audio/Effects/dragon-studio-explosion-sfx-405455.mp3");
            Check(secondExplosion!=null && secondExplosion.GetLength()>0 && audio.GetChildren().OfType<AudioStreamPlayer>().Any(a=>a.Bus=="Effects" && a.Stream==secondExplosion),"second supplied bomb explosion imports on Effects bus");
            foreach(string name in new[]{"freesound_community-electricity-sound-6066.mp3","freesound_community-electric_zap_001-6374.mp3"})
            {
                var extraZap=GD.Load<AudioStreamMP3>("res://Audio/Effects/"+name);
                Check(extraZap!=null && extraZap.GetLength()>0 && audio.GetChildren().OfType<AudioStreamPlayer>().Any(a=>a.Bus=="Effects" && a.Stream==extraZap),"new electricity sound imports on Effects: "+name);
            }
            GD.Print($"INTEGRATION VALIDATION PASSED: {checks} checks");
            game.QueueFree();await Frames(3);File.Delete(physical);GetTree().Quit();
        }
        catch(Exception e){GD.PushError(e.ToString());GetTree().Paused=false;game?.QueueFree();await Frames(3);GetTree().Quit(1);}
    }
}
