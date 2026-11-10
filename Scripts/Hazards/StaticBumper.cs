using Godot;
namespace TiltLabyrinth;
public partial class StaticBumper : StaticBody3D
{
    public event System.Action<StaticBumper>? Exploded;
    public BallController? Player { get; set; }
    public bool HasExploded { get; private set; }
    public ObstacleSpec Spec { get; set; }
    public Material Material { get; set; }=null!;
    public override void _PhysicsProcess(double delta)
    {
        var ball=Player;
        if(HasExploded || ball==null || !ball.Active || ball.Falling || ball.GlobalPosition.Y>ball.Radius+.5f)return;
        Vector2 offset=new(ball.GlobalPosition.X-Spec.Center.X,ball.GlobalPosition.Z-Spec.Center.Y);
        Vector2 outside=offset.Abs()-Spec.HalfSize;
        float distance=Spec.Bumper?Mathf.Max(0,offset.Length()-Spec.HalfSize.X):new Vector2(Mathf.Max(0,outside.X),Mathf.Max(0,outside.Y)).Length();
        if(distance>ball.Radius+.15f)return;
        HasExploded=true;SetDeferred(CollisionObject3D.PropertyName.CollisionLayer,0);SetDeferred(CollisionObject3D.PropertyName.CollisionMask,0);
        foreach(Node child in GetChildren())if(child is MeshInstance3D mesh)mesh.Visible=false;
        GetParent().AddChild(new ExplosionEffect {Position=new(Spec.Center.X,.08f,Spec.Center.Y),BlastScale=Mathf.Sqrt(Spec.HalfSize.X*Spec.HalfSize.Y)/.55f});Exploded?.Invoke(this);
    }
    private void BuildBombVisual()
    {
        float radius=Spec.HalfSize.X;
        var casing=new StandardMaterial3D {AlbedoColor=new Color(.025f,.03f,.04f),Metallic=.45f,Roughness=.32f};
        var cap=new StandardMaterial3D {AlbedoColor=new Color(.48f,.34f,.13f),Metallic=.7f,Roughness=.4f};
        var fuse=new StandardMaterial3D {AlbedoColor=new Color(.78f,.56f,.26f),Roughness=.95f};
        var ember=new StandardMaterial3D {AlbedoColor=new Color(1,.48f,.045f),EmissionEnabled=true,Emission=new Color(1,.25f,.01f),ShadingMode=BaseMaterial3D.ShadingModeEnum.Unshaded};
        AddChild(new MeshInstance3D {Position=new(0,radius-.7f,0),Scale=new(1,1,Spec.HalfSize.Y/radius),Mesh=new SphereMesh {Radius=radius,Height=radius*2,RadialSegments=24,Rings=12},MaterialOverride=casing});
        float top=radius*2-.7f;
        AddChild(new MeshInstance3D {Position=new(0,top+.04f,0),Mesh=new CylinderMesh {TopRadius=.19f,BottomRadius=.23f,Height=.2f},MaterialOverride=cap});
        Vector3 a=new(0,top+.14f,0),b=new(.28f,top+.4f,-.05f),tip=new(.65f,top+.5f,-.12f);
        FusePiece(a,b,fuse);FusePiece(b,tip,fuse);
        AddChild(new MeshInstance3D {Position=tip,Mesh=new SphereMesh {Radius=.11f,Height=.22f,RadialSegments=12,Rings=6},MaterialOverride=ember});
        AddChild(new MeshInstance3D {Position=tip,RotationDegrees=new(0,25,0),Mesh=new BoxMesh {Size=new(.42f,.025f,.045f)},MaterialOverride=ember});
        AddChild(new MeshInstance3D {Position=tip,RotationDegrees=new(0,115,0),Mesh=new BoxMesh {Size=new(.36f,.025f,.045f)},MaterialOverride=ember});
    }
    private void FusePiece(Vector3 from,Vector3 to,Material material)
    {
        Vector3 delta=to-from;
        AddChild(new MeshInstance3D {Position=(from+to)/2,Quaternion=new Quaternion(Vector3.Up,delta.Normalized()),Mesh=new CylinderMesh {TopRadius=.065f,BottomRadius=.065f,Height=delta.Length()},MaterialOverride=material});
    }
    public override void _Ready()
    {
        Position=new(Spec.Center.X,.7f,Spec.Center.Y);CollisionLayer=1;CollisionMask=2;PhysicsMaterialOverride=new PhysicsMaterial {Friction=.55f,Bounce=.08f};
        if(Spec.Bumper)
        {
            BuildBombVisual();
            AddChild(new CollisionShape3D {Shape=new CylinderShape3D {Radius=Spec.HalfSize.X,Height=1.4f}});
        }
        else
        {
            Vector3 size=new(Spec.HalfSize.X*2,1.4f,Spec.HalfSize.Y*2);
            BuildBombVisual();AddChild(new CollisionShape3D {Shape=new BoxShape3D {Size=size}});
        }
    }
}
