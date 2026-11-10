using Godot;
using System;
using System.Collections.Generic;
using System.IO;
namespace TiltLabyrinth;
public sealed class SaveManager
{
    public int HighestUnlocked { get; private set; }=1;
    public int CurrentLevel { get; private set; }=1;
    public int CurrentSeed { get; private set; }=1;
    public bool RecoveredCorruptSave { get; private set; }
    public Dictionary<int,double> BestTimes { get; }=new();
    private readonly string path;
    public SaveManager(string path="user://progress.cfg") {this.path=path;}
    public void Load()
    {
        HighestUnlocked=1;CurrentLevel=1;CurrentSeed=1;BestTimes.Clear();RecoveredCorruptSave=false;
        using var c=new ConfigFile();Error error=c.Load(path);
        if(error==Error.FileNotFound)return;
        try
        {
            if(error!=Error.Ok)throw new FormatException("Cannot parse save");
            int highest=(int)c.GetValue("progress","highest",1),level=(int)c.GetValue("progress","level",1),seed=(int)c.GetValue("progress","seed",1);
            if(highest<1||highest>10000||level<1||level>highest||seed<1)throw new FormatException("Invalid progress bounds");
            HighestUnlocked=highest;CurrentLevel=level;CurrentSeed=seed;
            if(c.HasSection("best"))foreach(string key in c.GetSectionKeys("best"))
            {
                double time=(double)c.GetValue("best",key);
                if(int.TryParse(key,out int n) && n>0 && n<=10000 && double.IsFinite(time) && time>0 && time<86400)BestTimes[n]=time;
            }
        }
        catch {HighestUnlocked=1;CurrentLevel=1;CurrentSeed=1;BestTimes.Clear();RecoveredCorruptSave=true;}
    }
    public void Select(int level,int seed) {CurrentLevel=Math.Clamp(level,1,HighestUnlocked);CurrentSeed=Math.Max(1,seed);Save();}
    public void Complete(int level,double time)
    {
        HighestUnlocked=Math.Min(10000,Math.Max(HighestUnlocked,level+1));
        if(double.IsFinite(time) && time>0 && (!BestTimes.TryGetValue(level,out double old)||time<old))BestTimes[level]=time;
        Save();
    }
    public void Save()
    {
        using var c=new ConfigFile();c.SetValue("progress","version",1);c.SetValue("progress","highest",HighestUnlocked);c.SetValue("progress","level",CurrentLevel);c.SetValue("progress","seed",CurrentSeed);
        foreach(var pair in BestTimes)c.SetValue("best",pair.Key.ToString(),pair.Value);
        string temp=path+".tmp";
        if(c.Save(temp)!=Error.Ok){GD.PushWarning("Could not save progress");return;}
        try {File.Move(ProjectSettings.GlobalizePath(temp),ProjectSettings.GlobalizePath(path),true);}
        catch(Exception e){GD.PushWarning("Could not replace progress save: "+e.Message);}
    }
}
