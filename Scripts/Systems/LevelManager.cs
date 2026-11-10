using System;
using System.Security.Cryptography;
namespace TiltLabyrinth;
public sealed class LevelManager
{
    public SaveManager Save { get; }
    public MazeGrid Current { get; private set; }=null!;
    public LevelManager(SaveManager save){Save=save;}
    public MazeGrid LoadCurrent()=>Generate(Save.CurrentLevel,Save.CurrentSeed);
    public MazeGrid Replay()=>Generate(Current.Level,Current.Seed);
    public MazeGrid Next()=>Generate(Math.Min(10000,Current.Level+1),RandomNumberGenerator.GetInt32(1,int.MaxValue));
    private MazeGrid Generate(int level,int seed)
    {
        var candidate=MazeGenerator.Generate(level,seed);MazeValidator.Validate(candidate);
        Current=candidate;Save.Select(level,seed);return Current;
    }
}
