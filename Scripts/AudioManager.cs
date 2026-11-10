using Godot;

namespace TiltLabyrinth;

public partial class AudioManager : Node

{

    public AudioSettings Settings { get; } = new();

    private AudioStreamPlayer music=null!,ambience=null!,impact=null!,victory=null!,fall=null!,checkpoint=null!,explosion=null!,explosion2=null!;

    private AudioStreamPlayer3D rolling=null!;

    private float rollGain;

    private float materialPitch=1;
    private ulong nextShock;
    private bool useSecondExplosion;
    private AudioStreamPlayer[] wallZaps=System.Array.Empty<AudioStreamPlayer>();
    private int shockIndex;

    public override void _Ready()

    {

        Settings.Load();

        music=Player("Music/zen.wav","Music",true); ambience=Player("Ambience/soundreality-metallic-ambience-593615.mp3","Ambience",true);

        impact=Player("Effects/impact.wav","Effects",false); victory=Player("Effects/victory.wav","Effects",false);

        fall=Player("Effects/fall.wav","Effects",false);checkpoint=Player("Effects/checkpoint.wav","Effects",false);explosion=Player("Effects/universfield-epic-cinematic-explosion-454857.mp3","Effects",false);explosion2=Player("Effects/dragon-studio-explosion-sfx-405455.mp3","Effects",false);wallZaps=new[]{Player("Effects/shock.mp3","Effects",false),Player("Effects/freesound_community-electrical-shock-zap-106412.mp3","Effects",false),Player("Effects/freesound_community-electricity-sound-6066.mp3","Effects",false),Player("Effects/freesound_community-electric_zap_001-6374.mp3","Effects",false)};

        Apply();

    }

    private static AudioStream Stream(string path,bool loop)
    {
        var stream=GD.Load<AudioStream>("res://Audio/"+path);
        if(loop && stream is AudioStreamWav wav){wav.LoopMode=AudioStreamWav.LoopModeEnum.Forward;wav.LoopBegin=0;wav.LoopEnd=wav.Data.Length/(wav.Stereo?4:2);}
        if(loop && stream is AudioStreamMP3 mp3){mp3.Loop=true;mp3.LoopOffset=0;}
        return stream;
    }

    private AudioStreamPlayer Player(string path,string bus,bool loop) { var a=new AudioStreamPlayer {Stream=Stream(path,loop),Bus=bus}; AddChild(a); if(loop)a.Play(); return a; }

    public void Attach(BallController ball) { rolling=new AudioStreamPlayer3D {Stream=Stream("Effects/krnbeatz-tectonic-passage-418836.mp3",true),Bus="Effects",VolumeDb=-80,UnitSize=30,MaxDistance=100}; ball.AddChild(rolling); rolling.Play(); ball.Impact+=Hit;ball.WallImpact+=WallShock; }

    public void Apply() { Set("Music",Settings.Music); Set("Ambience",Settings.Ambience); Set("Effects",Settings.Effects); AudioServer.SetBusMute(0,Settings.Mute); }

    private static void Set(string bus,float value) => AudioServer.SetBusVolumeDb(AudioServer.GetBusIndex(bus),value<0.001f?-80:Mathf.LinearToDb(value));

    public void Roll(float speed,double dt) { rollGain=Mathf.Lerp(rollGain,Mathf.Clamp(speed/12,0,1),1-Mathf.Exp(-(float)dt/0.12f)); rolling.VolumeDb=rollGain<0.005f?-80:Mathf.LinearToDb(rollGain*0.6f); rolling.PitchScale=0.75f+rollGain*0.6f; }

    private void Hit(float strength) { impact.VolumeDb=Mathf.LinearToDb(Mathf.Clamp(strength/8,0.04f,0.85f)); impact.PitchScale=materialPitch*(float)GD.RandRange(0.95,1.05); impact.Play(); }

    public override void _ExitTree()

    {

        foreach(var a in new[]{music,ambience,impact,victory,fall,checkpoint,explosion,explosion2}) { if(a!=null) { a.Stop(); a.Stream=null; } }

        foreach(var zap in wallZaps){zap.Stop();zap.Stream=null;}
        if(rolling!=null && GodotObject.IsInstanceValid(rolling)) { rolling.Stop();rolling.Stream=null; }

    }

    public void SetTheme(int level)=>materialPitch=((level-1)/3)%3==0?.72f:((level-1)/3)%3==1?1f:.55f;

    private void WallShock(float strength)
    {
        impact.Stop(); // Electrified walls use the supplied shock rather than a clink.
        ulong now=Time.GetTicksMsec();if(now<nextShock)return;nextShock=now+250;
        foreach(var sound in wallZaps)sound.Stop();
        var zap=wallZaps[shockIndex];shockIndex=(shockIndex+1)%wallZaps.Length;
        zap.VolumeDb=Mathf.LinearToDb(Mathf.Clamp(.35f+strength/16,.4f,.9f));zap.Play();
    }
    public void Explosion()
    {
        explosion.Stop();explosion2.Stop();var blast=useSecondExplosion?explosion2:explosion;useSecondExplosion=!useSecondExplosion;
        blast.VolumeDb=-9;blast.Play();
    }
    public void Fall() {fall.VolumeDb=-12;fall.Play();}

    public void Heal() {checkpoint.VolumeDb=-9;checkpoint.Play();}

    public void Win() => victory.Play();

    public void Suspend(bool value) { foreach(var a in new[]{music,ambience,impact,victory,fall,checkpoint,explosion,explosion2}) a.StreamPaused=value; foreach(var zap in wallZaps)zap.StreamPaused=value;if(rolling!=null)rolling.StreamPaused=value; }

}

