using Godot;
using System.Collections.Generic;
using System.Linq;
namespace TiltLabyrinth;
public partial class MazeBuilder : Node3D
{
    public MazeGrid Grid { get; set; }=null!;
    public MaterialLibrary Materials { get; private set; }=null!;
    public BallController? Player { get; set; }
    public List<StaticBumper> Obstacles { get; }=new();
    public List<HealingCircle> HealingCircles { get; }=new();
    public override void _Ready()
    {
        Grid??=MazeGenerator.Generate(1,1);MazeValidator.Validate(Grid);Materials=ThemeManager.Create(Grid.Level);
        AddChild(new BoardScenery {Grid=Grid});
        var floorBody=new StaticBody3D {CollisionLayer=1,CollisionMask=2,PhysicsMaterialOverride=new PhysicsMaterial {Friction=.55f,Bounce=.03f}};AddChild(floorBody);
        var floorTransforms=new List<Transform3D>();var floorShape=new BoxShape3D {Size=new(MazeGrid.CellWidth,.5f,MazeGrid.CellSize)};
        var holes=Grid.Holes.Select(h=>h.Cell).ToHashSet();
        for(int c=0;c<Grid.Cells.Length;c++)
        {
            if(holes.Contains(c))continue;Vector2 point=Grid.Center(c);Vector3 position=new(point.X,-.25f,point.Y);
            floorBody.AddChild(new CollisionShape3D {Shape=floorShape,Position=position});
            floorTransforms.Add(new Transform3D(Basis.Identity.Scaled(floorShape.Size),position));
        }
        Batch(floorTransforms,Materials.Floor);
        foreach(var hole in Grid.Holes)AddChild(new HoleHazard {Spec=hole,Materials=Materials,Player=Player});
        var wallBody=new StaticBody3D {CollisionLayer=1,CollisionMask=2,PhysicsMaterialOverride=new PhysicsMaterial {Friction=.55f,Bounce=.03f}};AddChild(wallBody);wallBody.AddToGroup("electrified_walls");
        var walls=new List<Transform3D>();var posts=new HashSet<(int,int)>();
        void Segment(int x,int z,bool horizontal)
        {
            float cell=MazeGrid.CellSize,t=MazeGrid.WallThickness;
            Vector3 size=horizontal?new(MazeGrid.CellWidth-t,3,t):new(t,3,cell-t);
            Vector3 center=new((x+(horizontal?.5f:0))*MazeGrid.CellWidth-Grid.Width/2,1.5f,(z+(horizontal?0:.5f))*cell-Grid.Depth/2);
            walls.Add(new Transform3D(Basis.Identity.Scaled(size),center));wallBody.AddChild(new CollisionShape3D {Shape=new BoxShape3D {Size=size},Position=center});
            posts.Add((x,z));posts.Add((x+(horizontal?1:0),z+(horizontal?0:1)));
        }
        for(int c=0;c<Grid.Cells.Length;c++)
        {
            int x=c%Grid.Columns,z=c/Grid.Columns;var w=Grid.Cells[c].Walls;
            if(w.HasFlag(CellWalls.North))Segment(x,z,true);
            if(w.HasFlag(CellWalls.West))Segment(x,z,false);
            if(z==Grid.Rows-1 && w.HasFlag(CellWalls.South))Segment(x,z+1,true);
            if(x==Grid.Columns-1 && w.HasFlag(CellWalls.East))Segment(x+1,z,false);
        }
        var postShape=new BoxShape3D {Size=new(MazeGrid.WallThickness,3,MazeGrid.WallThickness)};
        foreach(var (x,z) in posts)
        {
            Vector3 center=new(x*MazeGrid.CellWidth-Grid.Width/2,1.5f,z*MazeGrid.CellSize-Grid.Depth/2);
            walls.Add(new Transform3D(Basis.Identity.Scaled(postShape.Size),center));wallBody.AddChild(new CollisionShape3D {Shape=postShape,Position=center});
        }
        Batch(walls,Materials.Wall);
        var traces=new List<Transform3D>();
        foreach(var wall in walls)
        {
            Vector3 size=wall.Basis.Scale;size.Y=.07f;
            traces.Add(new Transform3D(Basis.Identity.Scaled(size),new Vector3(wall.Origin.X,3.04f,wall.Origin.Z)));
        }
        var electric=new ShaderMaterial {Shader=new Shader {Code=@"
            shader_type spatial;
            render_mode unshaded;
            varying vec3 pos;
            void vertex(){pos=(MODEL_MATRIX*vec4(VERTEX,1.0)).xyz;}
            void fragment(){float wave=.75+.25*sin(TIME*10.0+pos.x*2.0+pos.z*2.0);ALBEDO=vec3(.12,.8,1.0)*wave;EMISSION=vec3(.1,.75,1.0)*wave;}
        "}};
        Batch(traces,electric);
        foreach(var obstacle in Grid.Obstacles){var bumper=new StaticBumper {Spec=obstacle,Material=Materials.Bumper,Player=Player};Obstacles.Add(bumper);AddChild(bumper);}
        Marker(Grid.Center(Grid.StartCell),new Color(.3f,.65f,.8f),1.4f);
        Vector2 exit=Grid.Center(Grid.GoalCell);AddChild(new ExitMarker {Position=new(exit.X,0,exit.Y)});
        Vector2 goal=Grid.Center(Grid.GoalCell);var trigger=new GoalTrigger {Position=new(goal.X,.08f,goal.Y),CollisionLayer=0,CollisionMask=2};
        trigger.AddChild(new CollisionShape3D {Shape=new CylinderShape3D {Radius=1.6f,Height=3}});AddChild(trigger);
        foreach(int c in Grid.HealingCells){Vector2 point=Grid.Center(c);var check=new HealingCircle {Position=new(point.X,0,point.Y)};HealingCircles.Add(check);AddChild(check);}
    }
    private void Batch(List<Transform3D> transforms,Material material)
    {
        if(transforms.Count==0)return;
        var multi=new MultiMesh {TransformFormat=MultiMesh.TransformFormatEnum.Transform3D,Mesh=new BoxMesh(),InstanceCount=transforms.Count};
        for(int i=0;i<transforms.Count;i++)multi.SetInstanceTransform(i,transforms[i]);
        AddChild(new MultiMeshInstance3D {Multimesh=multi,MaterialOverride=material,CastShadow=GeometryInstance3D.ShadowCastingSetting.Off});
    }
    private void Marker(Vector2 pos,Color color,float radius)=>AddChild(new MeshInstance3D {Position=new(pos.X,.025f,pos.Y),Mesh=new CylinderMesh {TopRadius=radius,BottomRadius=radius,Height=.025f},MaterialOverride=new StandardMaterial3D {AlbedoColor=color,EmissionEnabled=true,Emission=color*.18f}});
}
