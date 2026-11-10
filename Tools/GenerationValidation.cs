using Godot;
using System;
namespace TiltLabyrinth;
public partial class GenerationValidation : Node
{
    public override void _Ready()
    {
        try
        {
            foreach(int level in new[]{1,3,5,8,11,16})
            {
                for(int seed=1;seed<=100;seed++)
                {
                    MazeValidator.Validate(MazeGenerator.Generate(level,seed,false));
                    var g=MazeGenerator.Generate(level,seed);MazeValidator.Validate(g);
                    if(g.Fingerprint()!=MazeGenerator.Generate(level,seed).Fingerprint())throw new Exception("Nondeterministic seed");
                }
                GD.Print($"PASS: level {level} tier, 100 seeds; connectivity, swept-ball clearance, hazard/healing safety, deterministic regeneration");
            }
            GD.Print("GENERATION VALIDATION PASSED: 600 seeded layouts");GetTree().Quit();
        }
        catch(Exception e){GD.PushError(e.ToString());GetTree().Quit(1);}
    }
}
