using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
namespace TiltLabyrinth;
public static class MazeGenerator
{
    public static MazeGrid Generate(int level,int seed,bool hazards=true)
    {
        var d=DifficultyConfig.ForLevel(level);var g=new MazeGrid(d.Columns,d.Rows,seed,level);var rng=new Random(seed);
        g.StartCell=(g.Rows-1)*g.Columns;var visited=new HashSet<int>{g.StartCell};var stack=new Stack<int>();stack.Push(g.StartCell);
        while(stack.Count>0)
        {
            int cell=stack.Peek();var choices=g.Neighbors(cell).Where(n=>!visited.Contains(n)).ToArray();
            if(choices.Length==0) {stack.Pop();continue;}
            int next=choices[rng.Next(choices.Length)];g.Open(cell,next);visited.Add(next);stack.Push(next);
        }
        for(int i=0;i<d.LoopCount;i++)
        {
            int c=rng.Next(g.Cells.Length);var closed=g.Neighbors(c).Where(n=>!g.Neighbors(c,true).Contains(n)).ToArray();
            if(closed.Length>0)g.Open(c,closed[rng.Next(closed.Length)]);
        }
        // Later levels occasionally contain a small open courtyard.
        if(level>=5) {int x=rng.Next(g.Columns-1),z=rng.Next(g.Rows-1),a=z*g.Columns+x;g.Open(a,a+1);g.Open(a,a+g.Columns);g.Open(a+1,a+1+g.Columns);g.Open(a+g.Columns,a+g.Columns+1);}
        int[] parents=Parents(g,g.StartCell);int[] distance=new int[g.Cells.Length];
        for(int i=0;i<distance.Length;i++){int c=i;while(c!=g.StartCell && parents[c]>=0){distance[i]++;c=parents[c];}}
        g.GoalCell=Enumerable.Range(0,g.Cells.Length).OrderByDescending(c=>distance[c]).ThenBy(c=>c).First();
        for(int c=g.GoalCell;c!=-1;c=parents[c])g.Route.Add(c);g.Route.Reverse();
        if(!hazards)return g;
        var route=g.Route.ToHashSet();var shuffled=Enumerable.Range(0,g.Cells.Length).OrderBy(_=>rng.Next()).ToList();
        foreach(int cell in shuffled)
            if(g.Holes.Count<d.HoleCount && !route.Contains(cell) && g.Center(cell).DistanceTo(g.Center(g.StartCell))>MazeGrid.CellSize && g.Center(cell).DistanceTo(g.Center(g.GoalCell))>MazeGrid.CellSize)
                {
                // Distinct small, medium and large apertures; capture and floor geometry
                // both use this radius, while the safe route remains untouched.
                float[] radii={1.75f,2.15f,2.7f};
                float radius=radii[(g.Holes.Count+(int)((uint)seed%3))%radii.Length];
                g.Holes.Add(new(cell,g.Center(cell),radius));
            }
        var holeCells=g.Holes.Select(h=>h.Cell).ToHashSet();
        foreach(int cell in shuffled)
        {
            if(cell==g.StartCell || cell==g.GoalCell || holeCells.Contains(cell))continue;
            bool bumper=g.Obstacles.Count(o=>o.Bumper)<d.Bumpers;
            if(!bumper && g.Obstacles.Count(o=>!o.Bumper)>=d.Barriers)break;
            Vector2 offset=new(rng.Next(2)==0?-2.25f:2.25f,rng.Next(2)==0?-2.4f:2.4f);
            g.Obstacles.Add(new(cell,g.Center(cell)+offset,bumper?new Vector2(.55f,.55f):new Vector2(.65f,.9f),bumper));
        }
        if(level>=5 && g.Route.Count>=12)
        {
            foreach(int index in level>=8 && g.Route.Count>=24?new[]{g.Route.Count/3,g.Route.Count*2/3}:new[]{g.Route.Count/2})
            {
                int cell=g.Route[index];g.Obstacles.RemoveAll(o=>o.Cell==cell);g.HealingCells.Add(cell);
            }
        }
        return g;
    }
    public static int[] Parents(MazeGrid g,int start)
    {
        int[] parent=Enumerable.Repeat(-2,g.Cells.Length).ToArray();parent[start]=-1;var q=new Queue<int>();q.Enqueue(start);
        while(q.Count>0){int c=q.Dequeue();foreach(int n in g.Neighbors(c,true))if(parent[n]==-2){parent[n]=c;q.Enqueue(n);}}
        return parent;
    }
}
