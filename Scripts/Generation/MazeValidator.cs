using Godot;
using System;
using System.Linq;
namespace TiltLabyrinth;
public static class MazeValidator
{
    public static void Validate(MazeGrid g)
    {
        if(MazeGenerator.Parents(g,g.StartCell).Any(p=>p==-2))throw new InvalidOperationException("Disconnected maze");
        if(g.Route.Count<3 || g.Route[0]!=g.StartCell || g.Route[^1]!=g.GoalCell)throw new InvalidOperationException("Invalid route");
        for(int c=0;c<g.Cells.Length;c++)
        {
            int x=c%g.Columns,z=c/g.Columns;CellWalls w=g.Cells[c].Walls;
            if((x==0&&!w.HasFlag(CellWalls.West))||(z==0&&!w.HasFlag(CellWalls.North))||(x==g.Columns-1&&!w.HasFlag(CellWalls.East))||(z==g.Rows-1&&!w.HasFlag(CellWalls.South)))throw new InvalidOperationException("Open boundary");
            foreach(int n in g.Neighbors(c,true))if(!g.Neighbors(n,true).Contains(c))throw new InvalidOperationException("Asymmetric wall");
        }
        float clearance=MazeGrid.BallRadius+.25f;
        if((MazeGrid.CellSize-MazeGrid.WallThickness)/2<clearance)throw new InvalidOperationException("Corridor too narrow for ball");
        foreach(var hole in g.Holes)
        {
            if(hole.Radius<MazeGrid.BallRadius+.5f || hole.Radius+.12f>MazeGrid.CellSize/2-MazeGrid.WallThickness)throw new InvalidOperationException("Unsafe hole geometry");
            if(hole.Cell==g.StartCell || hole.Cell==g.GoalCell || g.HealingCells.Contains(hole.Cell))throw new InvalidOperationException("Hole on safe marker");
        }
        foreach(var o in g.Obstacles)
        {
            Vector2 delta=o.Center-g.Center(o.Cell);
            if(Mathf.Abs(delta.X)+o.HalfSize.X>MazeGrid.CellSize/2-MazeGrid.WallThickness/2 || Mathf.Abs(delta.Y)+o.HalfSize.Y>MazeGrid.CellSize/2-MazeGrid.WallThickness/2)throw new InvalidOperationException("Obstacle overlaps wall");
        }
        // Validate a continuous swept-ball route, including turns. Corridor
        // centers have analytic wall clearance; sample expanded hazard geometry.
        for(int i=0;i<g.Route.Count-1;i++)
        {
            if(!g.Neighbors(g.Route[i],true).Contains(g.Route[i+1]))throw new InvalidOperationException("Route crosses wall");
            Vector2 a=g.Center(g.Route[i]),b=g.Center(g.Route[i+1]);
            for(int s=0;s<=24;s++)
            {
                Vector2 point=a.Lerp(b,s/24f);
                if(g.Holes.Any(h=>point.DistanceTo(h.Center)<Mathf.Max(h.Radius+clearance,h.Radius+MazeGrid.BallRadius+HoleHazard.DangerMargin+.1f)))throw new InvalidOperationException("Hole blocks physical route");
                foreach(var o in g.Obstacles)
                {
                    Vector2 d=(point-o.Center).Abs()-o.HalfSize;
                    if(new Vector2(Mathf.Max(0,d.X),Mathf.Max(0,d.Y)).Length()<clearance)throw new InvalidOperationException("Obstacle blocks physical route");
                }
            }
        }
        foreach(int c in g.HealingCells)if(!g.Route.Contains(c) || g.Holes.Any(h=>h.Cell==c) || g.Obstacles.Any(o=>o.Cell==c))throw new InvalidOperationException("Unsafe healing circle");
    }
}
