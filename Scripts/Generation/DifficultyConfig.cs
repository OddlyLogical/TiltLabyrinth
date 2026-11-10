namespace TiltLabyrinth;
public readonly record struct DifficultyConfig(int Columns,int Rows,int HoleCount,int Bumpers,int Barriers,int LoopCount)
{
    // Landscape variants of the brief's tunable density targets. Radius and
    // force/speed limits stay identical at every difficulty.
    public static DifficultyConfig ForLevel(int level)=>level switch
    {
        <=2=>new(5,3,0,level==2?1:0,0,1),
        <=4=>new(6,4,1,2,1,2),
        <=7=>new(7,4,2,3,2,3),
        <=10=>new(8,5,3,4,3,4),
        <=15=>new(9,5,5,5,4,5),
        _=>new(10,6,7,6,5,6)
    };
}
