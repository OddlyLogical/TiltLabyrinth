using Godot;
using System.Collections.Generic;
namespace TiltLabyrinth;
public partial class HoleHazard : Node3D
{
    public const int Segments=64;
    public const float DangerMargin=.15f;
    public BallController? Player { get; set; }
    public override void _PhysicsProcess(double delta)
    {
        var ball=Player;
        if(ball==null || !ball.Active || ball.Falling || ball.ResetPending || ball.Position.Y>ball.Radius+.5f)return;
        Vector2 offset=new(ball.Position.X-Spec.Center.X,ball.Position.Z-Spec.Center.Y);
        float danger=Spec.Radius+ball.Radius+DangerMargin;
        if(offset.LengthSquared()<=danger*danger)ball.CaptureByHole();
    }
    public HoleSpec Spec { get; set; }
    public MaterialLibrary Materials { get; set; }=null!;
    public override void _Ready()
    {
        Position=new(Spec.Center.X,0,Spec.Center.Y);
        var vertices=new List<Vector3>();Vector2 half=new(MazeGrid.CellWidth/2,MazeGrid.CellSize/2);
        var angles=new System.Collections.Generic.SortedSet<float>();
        for(int i=0;i<Segments;i++)angles.Add(Mathf.Tau*i/Segments);
        float corner=Mathf.Atan2(half.Y,half.X);angles.Add(corner);angles.Add(Mathf.Pi-corner);angles.Add(Mathf.Pi+corner);angles.Add(Mathf.Tau-corner);
        float[] steps=System.Linq.Enumerable.ToArray(angles);
        for(int i=0;i<steps.Length;i++)
        {
            float a=steps[i],b=i+1<steps.Length?steps[i+1]:Mathf.Tau;
            Vector2 da=new(Mathf.Cos(a),Mathf.Sin(a)),db=new(Mathf.Cos(b),Mathf.Sin(b));
            Vector2 ia=da*Spec.Radius,ib=db*Spec.Radius;
            Vector2 oa=da*Mathf.Min(half.X/Mathf.Max(.00001f,Mathf.Abs(da.X)),half.Y/Mathf.Max(.00001f,Mathf.Abs(da.Y))),ob=db*Mathf.Min(half.X/Mathf.Max(.00001f,Mathf.Abs(db.X)),half.Y/Mathf.Max(.00001f,Mathf.Abs(db.Y)));
            vertices.AddRange(new[]{V(ia),V(oa),V(ob),V(ia),V(ob),V(ib)});
        }
        var arrays=new Godot.Collections.Array();arrays.Resize((int)Mesh.ArrayType.Max);arrays[(int)Mesh.ArrayType.Vertex]=vertices.ToArray();
        var normals=new Vector3[vertices.Count];System.Array.Fill(normals,Vector3.Up);arrays[(int)Mesh.ArrayType.Normal]=normals;
        var mesh=new ArrayMesh();mesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles,arrays);
        AddChild(new MeshInstance3D {Mesh=mesh,MaterialOverride=Materials.Floor});
        var body=new StaticBody3D {CollisionLayer=1,CollisionMask=2,PhysicsMaterialOverride=new PhysicsMaterial {Friction=.55f,Bounce=.03f}};
        body.AddChild(new CollisionShape3D {Shape=new ConcavePolygonShape3D {Data=vertices.ToArray(),BackfaceCollision=true}});AddChild(body);
        AddChild(new MeshInstance3D {Position=new(0,.015f,0),Mesh=new TorusMesh {InnerRadius=Spec.Radius,OuterRadius=Spec.Radius+.12f,Rings=64,RingSegments=8},MaterialOverride=Materials.Rim});
        // Open shaft walls with a recessed base: nothing closes the aperture.
        var shaft=new List<Vector3>();
        for(int i=0;i<Segments;i++)
        {
            float a=Mathf.Tau*i/Segments,b=Mathf.Tau*(i+1)/Segments;
            Vector3 topA=new(Mathf.Cos(a)*Spec.Radius,0,Mathf.Sin(a)*Spec.Radius),topB=new(Mathf.Cos(b)*Spec.Radius,0,Mathf.Sin(b)*Spec.Radius);
            shaft.AddRange(new[]{topA,topB,topB+Vector3.Down*3,topA,topB+Vector3.Down*3,topA+Vector3.Down*3});
        }
        var inner=new StandardMaterial3D {AlbedoColor=new Color(.055f,.04f,.025f),CullMode=BaseMaterial3D.CullModeEnum.Disabled,Roughness=1};
        var sideArray=new Godot.Collections.Array();sideArray.Resize((int)Mesh.ArrayType.Max);sideArray[(int)Mesh.ArrayType.Vertex]=shaft.ToArray();
        var sideMesh=new ArrayMesh();sideMesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles,sideArray);AddChild(new MeshInstance3D {Mesh=sideMesh,MaterialOverride=inner});
        AddChild(new MeshInstance3D {Position=new(0,-3.2f,0),Mesh=new CylinderMesh {TopRadius=Spec.Radius,BottomRadius=Spec.Radius,Height=.1f},MaterialOverride=new StandardMaterial3D {AlbedoColor=new Color(.004f,.006f,.008f),ShadingMode=BaseMaterial3D.ShadingModeEnum.Unshaded}});
    }
    private static Vector3 V(Vector2 v)=>new(v.X,0,v.Y);
}
