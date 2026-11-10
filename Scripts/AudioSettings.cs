using Godot;
namespace TiltLabyrinth;
public sealed class AudioSettings
{
    public float Music=0.25f, Ambience=0.35f, Effects=0.65f;
    public bool Mute;
    public bool Haptics=true;
    private const string Path="user://audio.cfg";
    public void Load() { using var c=new ConfigFile(); if(c.Load(Path)!=Error.Ok)return; Music=Read(c,"Music",Music); Ambience=Read(c,"Ambience",Ambience); Effects=Read(c,"Effects",Effects); Mute=(bool)c.GetValue("audio","Mute",false); Haptics=(bool)c.GetValue("audio","Haptics",true); }
    private static float Read(ConfigFile c,string key,float fallback) { float v=(float)c.GetValue("audio",key,fallback); return float.IsFinite(v)?Mathf.Clamp(v,0,1):fallback; }
    public void Save() { using var c=new ConfigFile(); c.SetValue("audio","Music",Music); c.SetValue("audio","Ambience",Ambience); c.SetValue("audio","Effects",Effects); c.SetValue("audio","Mute",Mute); c.SetValue("audio","Haptics",Haptics); if(c.Save(Path)!=Error.Ok) GD.PushWarning("Could not save audio preferences"); }
}
