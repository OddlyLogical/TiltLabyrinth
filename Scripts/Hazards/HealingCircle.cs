using Godot;
using System;
namespace TiltLabyrinth;
public partial class HealingCircle : Area3D
{
    public Func<HealingCircle,bool>? TryHeal { get; set; }
    public bool IsConsumed { get; private set; }
    private BallController? candidate;
    private StandardMaterial3D material=null!,cross=null!;
    public override void _Ready()
    {
        CollisionLayer=0;CollisionMask=2;
        AddChild(new CollisionShape3D {Shape=new CylinderShape3D {Radius=1.1f,Height=3}});
        material=new(){AlbedoColor=new Color(.12f,.85f,.36f),EmissionEnabled=true,Emission=new Color(.03f,.3f,.08f)};
        cross=new(){AlbedoColor=new Color(.9f,1,.93f),ShadingMode=BaseMaterial3D.ShadingModeEnum.Unshaded};
        AddChild(new MeshInstance3D {Position=new(0,.025f,0),Mesh=new CylinderMesh {TopRadius=1.3f,BottomRadius=1.3f,Height=.03f},MaterialOverride=material});
        AddChild(new MeshInstance3D {Position=new(0,.055f,0),Mesh=new BoxMesh {Size=new(1.1f,.02f,.25f)},MaterialOverride=cross});
        AddChild(new MeshInstance3D {Position=new(0,.055f,0),Mesh=new BoxMesh {Size=new(.25f,.02f,1.1f)},MaterialOverride=cross});
        BodyEntered+=body=>{if(body is BallController ball)candidate=ball;};
        BodyExited+=body=>{if(body==candidate)candidate=null;};
    }
    public override void _PhysicsProcess(double delta)
    {
        if(IsConsumed || candidate==null || !candidate.Active || candidate.Falling)return;
        if(TryHeal?.Invoke(this)!=true)return;
        IsConsumed=true;material.AlbedoColor=new Color(.15f,.25f,.18f);material.Emission=Colors.Black;cross.AlbedoColor=new Color(.3f,.4f,.33f);
        SetDeferred(Area3D.PropertyName.Monitoring,false);
    }
}
