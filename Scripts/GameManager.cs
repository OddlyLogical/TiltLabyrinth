using Godot;
namespace TiltLabyrinth;
public partial class GameManager : Node3D
{
    [Export] public string SavePath { get; set; }="user://progress.cfg";
    public GameState State { get; private set; }=GameState.Loading;
    public LevelManager Levels { get; private set; }=null!;
    public int Falls { get; private set; }
    public int Health { get; private set; }=100;
    [Export] public int WallDamagePercent { get; set; }=10;
    private float shockCooldown;
    public double TimeLimit { get; private set; }
    public double RemainingTime=>System.Math.Max(0,TimeLimit-Elapsed);
    [Export] public float MinimumTimeSeconds { get; set; }=120;
    [Export] public float SecondsPerRouteStep { get; set; }=8;
    public double Elapsed { get; private set; }
    private BallController ball=null!;private AndroidTiltInput tilt=null!;private HUDController hud=null!;private AudioManager audio=null!;private MazeBuilder maze=null!;private Camera3D camera=null!;
    private Vector3 respawn;private float stateTime,feedbackTime,hudTime;private string feedback="";private bool audioOpen,background;
    private GameState resumeState=GameState.Ready;
    public override void _Ready()
    {
        ProcessMode=ProcessModeEnum.Always;GetTree().QuitOnGoBack=false;if(OS.HasFeature("android"))Engine.MaxFps=60;var save=new SaveManager(SavePath);save.Load();Levels=new(save);
        tilt=new AndroidTiltInput();AddChild(tilt);
        ball=GD.Load<PackedScene>("res://Scenes/Ball.tscn").Instantiate<BallController>();ball.ProcessMode=ProcessModeEnum.Pausable;ball.InputSource=tilt;AddChild(ball);
        audio=new AudioManager();AddChild(audio);audio.Attach(ball);var haptic=new HapticFeedback(audio.Settings);ball.Impact+=haptic.OnImpact;ball.WallImpact+=haptic.OnImpact;ball.WallImpact+=OnWallImpact;
        hud=GD.Load<PackedScene>("res://Scenes/HUD.tscn").Instantiate<HUDController>();AddChild(hud);
        hud.Restart+=()=>BuildLevel(Levels.Replay());hud.NextLevel+=()=>{if(State==GameState.LevelComplete)BuildLevel(Levels.Next());};hud.Calibrate+=Calibrate;hud.Pause+=TogglePause;
        hud.SettingsRequested+=()=>{PauseGame();audioOpen=true;hud.ShowAudio(true);};hud.ConfigureAudio(audio,()=>{audioOpen=false;hud.ShowAudio(false);ResumeGame();});
        GoalTrigger.Reached+=OnGoal;
        camera=new Camera3D {Projection=Camera3D.ProjectionType.Orthogonal,KeepAspect=Camera3D.KeepAspectEnum.Height,Current=true};AddChild(camera);var listener=new AudioListener3D();camera.AddChild(listener);listener.MakeCurrent();
        AddChild(new DirectionalLight3D {RotationDegrees=new(-70,-30,0),LightEnergy=1.25f,ShadowEnabled=true});
        var skyMaterial=new ProceduralSkyMaterial {SkyTopColor=new Color(.32f,.43f,.57f),SkyHorizonColor=new Color(.85f,.88f,.86f),GroundBottomColor=new Color(.12f,.14f,.16f),GroundHorizonColor=new Color(.7f,.72f,.7f)};
        AddChild(new WorldEnvironment {Environment=new Godot.Environment {BackgroundMode=Godot.Environment.BGMode.Color,BackgroundColor=new Color(.025f,.045f,.055f),Sky=new Sky {SkyMaterial=skyMaterial},AmbientLightSource=Godot.Environment.AmbientSource.Color,AmbientLightColor=new Color(.72f,.79f,.83f),AmbientLightEnergy=.65f,ReflectedLightSource=Godot.Environment.ReflectionSource.Sky}});
        BuildLevel(Levels.LoadCurrent());Calibrate();
    }
    private float telemetryTime,calibrationRetryTime;
    public override void _Process(double delta)
    {
        if(!OS.HasFeature("android") || !OS.IsDebugBuild())return;
        telemetryTime+=(float)delta;if(telemetryTime<5)return;telemetryTime=0;
        GD.Print($"PERF level={Levels.Current.Level} fps={Performance.GetMonitor(Performance.Monitor.TimeFps):F0} physics_ms={Performance.GetMonitor(Performance.Monitor.TimePhysicsProcess)*1000:F2} draws={Performance.GetMonitor(Performance.Monitor.RenderTotalDrawCallsInFrame):F0}");
    }
    private void SetState(GameState state) {State=state;stateTime=0;}
    private void BuildLevel(MazeGrid grid)
    {
        SetState(GameState.Loading);GetTree().Paused=false;audioOpen=false;hud.ShowAudio(false);ball.Active=false;ball.Freeze=true;
        if(maze!=null){RemoveChild(maze);maze.Free();}
        maze=GD.Load<PackedScene>("res://Scenes/Maze.tscn").Instantiate<MazeBuilder>();maze.Grid=grid;maze.Player=ball;maze.ProcessMode=ProcessModeEnum.Pausable;AddChild(maze);
        foreach(var obstacle in maze.Obstacles)obstacle.Exploded+=OnExplosion;
        foreach(var circle in maze.HealingCircles)circle.TryHeal=OnHealingCircle;
        respawn=grid.Spawn(grid.StartCell);ball.ConfigureLevel(respawn);Elapsed=0;TimeLimit=System.Math.Max(MinimumTimeSeconds,grid.Route.Count*SecondsPerRouteStep);Falls=0;Health=100;shockCooldown=0;feedbackTime=0;hud.SetHealth(Health);
        Vector2 view=GetViewport().GetVisibleRect().Size;float aspect=view.X/Mathf.Max(1,view.Y);
        camera.Size=Mathf.Max((grid.Depth+2)/.78f,(grid.Width+2)/(aspect*.97f));camera.Position=new(0,Mathf.Max(60,grid.Depth*2),3);camera.LookAt(new Vector3(0,0,-.4f));
        hud.SetLevel(grid.Level,maze.Materials.Name);audio.SetTheme(grid.Level);audio.Suspend(false);SetState(GameState.Ready);
        GD.Print($"LEVEL {grid.Level} SEED {grid.Seed} GRID {grid.Columns}x{grid.Rows} ROUTE {grid.Route.Count} THEME {maze.Materials.Name}");
    }
    private void Calibrate() {if(State!=GameState.Ready && State!=GameState.Playing)return;tilt.Calibration.Begin();ball.Active=false;SetState(GameState.Ready);}
    private void PauseGame()
    {
        if(State==GameState.Paused || State==GameState.Loading)return;resumeState=State;State=GameState.Paused;GetTree().Paused=true;audio.Suspend(true);
    }
    private void ResumeGame()
    {
        if(State!=GameState.Paused || background || audioOpen)return;State=resumeState;GetTree().Paused=false;audio.Suspend(false);
    }
    private void TogglePause(){if(State==GameState.Paused)ResumeGame();else PauseGame();}
    public override void _PhysicsProcess(double delta)
    {
        if(ball==null||hud==null)return;
        if(State!=GameState.Paused)stateTime+=(float)delta;
        feedbackTime=Mathf.Max(0,feedbackTime-(float)delta);
        if(State==GameState.Playing)shockCooldown= Mathf.Max(0,shockCooldown-(float)delta);
        switch(State)
        {
            case GameState.Ready:
                ball.Active=false;
                if(tilt.Available && !tilt.Calibration.Ready && !tilt.Calibration.Collecting)
                {
                    calibrationRetryTime+=(float)delta;
                    if(calibrationRetryTime>=.4f){tilt.Calibration.Begin();calibrationRetryTime=0;}
                }
                else calibrationRetryTime=0;
                if(tilt.Available && tilt.Calibration.Ready && !tilt.Calibration.Collecting && !ball.ResetPending)SetState(GameState.Playing);
                break;
            case GameState.Playing:
                if(!tilt.Available || tilt.Calibration.Collecting){ball.Active=false;SetState(GameState.Ready);break;}
                ball.Active=true;
                if(ball.Falling){Falls++;audio.Fall();BeginDrop(false);SetState(GameState.Falling);}else {Elapsed=System.Math.Min(TimeLimit,Elapsed+delta);if(RemainingTime<=0)Lose("Time ran out");}
                break;
            case GameState.Falling:
                ball.Active=true;
                if(ball.Position.Y < -3 || stateTime>2.5f){ball.Active=false;ball.SetDeferred(RigidBody3D.PropertyName.Freeze,true);SetState(GameState.Respawning);}
                break;
            case GameState.Respawning:
                if(stateTime>=.65f && !ball.DropAnimating){ball.ResetBall(respawn);feedback="A fresh breath · try again";feedbackTime=2;SetState(GameState.Ready);}
                break;
            case GameState.Exiting:
                ball.Active=false;
                if(!ball.DropAnimating && stateTime>=1.3f)CompleteLevel();
                break;
            case GameState.LevelComplete:
            case GameState.GameOver:ball.Active=false;break;
        }
        audio.Roll(State==GameState.Playing && !ball.Falling?ball.HorizontalSpeed:0,delta);
        string status=State switch {GameState.Exiting=>"Exit reached · settling in…",GameState.Ready=>!tilt.Available?tilt.Status:!tilt.Calibration.Ready?"Hold phone comfortably and still · calibrating automatically":tilt.Status,GameState.GameOver=>"Run ended · tap Restart",GameState.Paused=>"Paused · tap Resume",GameState.Falling=>"A little tumble…",GameState.Respawning=>"Returning to your safe place",GameState.LevelComplete=>"A quiet moment. Well done.",_=>feedbackTime>0?feedback:tilt.Status};
        hudTime-=(float)delta;if(hudTime<=0){hud.Update(Elapsed,status,State==GameState.Paused,Falls,RemainingTime);hudTime=.1f;}
    }
    private void OnWallImpact(float strength)
    {
        if(State!=GameState.Playing || ball.Falling || strength<=.6f || shockCooldown>0)return;
        shockCooldown=.45f;ApplyDamage(System.Math.Clamp(WallDamagePercent,1,100),"Electric shock");
    }
    private void OnExplosion(StaticBumper obstacle)
    {
        if(State!=GameState.Playing || ball.Falling)return;
        audio.Explosion();
        if(audio.Settings.Haptics && OS.HasFeature("android"))Input.VibrateHandheld(55,.75f);
        ApplyDamage(25,"Obstacle explosion");
    }
    private void ApplyDamage(int amount,string reason)
    {
        Health=System.Math.Max(0,Health-amount);hud.SetHealth(Health);hud.Shock();feedback=$"{reason} · {Health}% health";feedbackTime=1.2f;
        if(Health==0)Lose("Health depleted");
    }
    private void Lose(string reason)
    {
        if(State!=GameState.Playing)return;
        SetState(GameState.GameOver);ball.Active=false;ball.SetDeferred(RigidBody3D.PropertyName.Freeze,true);hud.ShowGameOver(Elapsed,Falls,reason);
    }
    private bool OnHealingCircle(HealingCircle circle)
    {
        if(State!=GameState.Playing || Health>=100)return false;
        int restored=System.Math.Min(25,100-Health);Health+=restored;hud.SetHealth(Health);audio.Heal();feedback=$"+{restored} health · {Health}%";feedbackTime=3;
        return true;
    }
    private void BeginDrop(bool exit)
    {
        Vector2 center=maze.Grid.Center(maze.Grid.GoalCell);
        if(!exit)
        {
            float closest=float.MaxValue;
            foreach(var hole in maze.Grid.Holes)
            {
                float distance=hole.Center.DistanceSquaredTo(new Vector2(ball.Position.X,ball.Position.Z));
                if(distance<closest){closest=distance;center=hole.Center;}
            }
        }
        Vector3 position=new(center.X,0,center.Y);ball.BeginDropVisual(position,exit);
        maze.AddChild(new DropRipple {Position=position,Exit=exit});
    }
    private void OnGoal(BallController body)
    {
        if(body!=ball || State!=GameState.Playing || !body.Active || body.Falling)return;
        SetState(GameState.Exiting);ball.Active=false;ball.SetDeferred(RigidBody3D.PropertyName.Freeze,true);BeginDrop(true);audio.Fall();
    }
    private void CompleteLevel()
    {
        SetState(GameState.LevelComplete);Levels.Save.Complete(Levels.Current.Level,Elapsed);
        Levels.Save.BestTimes.TryGetValue(Levels.Current.Level,out double best);hud.Complete(Elapsed,Falls,best,Levels.Current.Level<10000);audio.Win();
    }
    public override void _Notification(int what)
    {
        if(audio==null||hud==null)return;
        if(what==NotificationApplicationPaused || what==NotificationApplicationFocusOut){background=true;PauseGame();}
        if(what==NotificationApplicationResumed || what==NotificationApplicationFocusIn)background=false;
        if(what==NotificationWMGoBackRequest)PauseGame();
    }
    public override void _ExitTree(){GoalTrigger.Reached-=OnGoal;GetTree().Paused=false;}
}
