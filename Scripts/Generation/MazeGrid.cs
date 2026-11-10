using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
namespace TiltLabyrinth;
[Flags] public enum CellWalls { North=1, East=2, South=4, West=8, All=15 }
public sealed class MazeCell { public CellWalls Walls=CellWalls.All; }
public readonly record struct HoleSpec(int Cell, Vector2 Center, float Radius);
public readonly record struct ObstacleSpec(int Cell, Vector2 Center, Vector2 HalfSize, bool Bumper);
public sealed class MazeGrid
{
    public const float CellWidth=12.4f, CellSize=7.2f, WallThickness=0.4f, BallRadius=1.21f;
    public int Columns { get; } public int Rows { get; } public int Seed { get; } public int Level { get; }
    public MazeCell[] Cells { get; }
    public int StartCell { get; set; } public int GoalCell { get; set; }
    public List<int> Route { get; }=new();
    public List<HoleSpec> Holes { get; }=new();
    public List<ObstacleSpec> Obstacles { get; }=new();
    public List<int> HealingCells { get; }=new();
    public float Width=>Columns*CellWidth; public float Depth=>Rows*CellSize;
    public MazeGrid(int columns,int rows,int seed,int level) { Columns=columns;Rows=rows;Seed=seed;Level=level;Cells=Enumerable.Range(0,columns*rows).Select(_=>new MazeCell()).ToArray(); }
    public Vector2 Center(int cell)=>new((cell%Columns-(Columns-1)/2f)*CellWidth,(cell/Columns-(Rows-1)/2f)*CellSize);
    public Vector3 Spawn(int cell) { Vector2 p=Center(cell);return new(p.X,BallRadius+0.04f,p.Y); }
    public IEnumerable<int> Neighbors(int cell,bool openOnly=false)
    {
        int x=cell%Columns,z=cell/Columns;
        if(z>0 && (!openOnly || !Cells[cell].Walls.HasFlag(CellWalls.North)))yield return cell-Columns;
        if(x<Columns-1 && (!openOnly || !Cells[cell].Walls.HasFlag(CellWalls.East)))yield return cell+1;
        if(z<Rows-1 && (!openOnly || !Cells[cell].Walls.HasFlag(CellWalls.South)))yield return cell+Columns;
        if(x>0 && (!openOnly || !Cells[cell].Walls.HasFlag(CellWalls.West)))yield return cell-1;
    }
    public void Open(int a,int b)
    {
        CellWalls w=b==a-Columns?CellWalls.North:b==a+Columns?CellWalls.South:b==a+1?CellWalls.East:CellWalls.West;
        CellWalls opposite=w==CellWalls.North?CellWalls.South:w==CellWalls.South?CellWalls.North:w==CellWalls.East?CellWalls.West:CellWalls.East;
        Cells[a].Walls&=~w;Cells[b].Walls&=~opposite;
    }
    public string Fingerprint()=> $"{Columns},{Rows},{Seed},{Level}:{StartCell},{GoalCell}:"+string.Join(',',Cells.Select(c=>(int)c.Walls))+":"+string.Join(';',Holes)+":"+string.Join(';',Obstacles)+":"+string.Join(',',HealingCells);
}
