using Godot;
namespace TiltLabyrinth;
public partial class DropRipple : Node3D
{
    public bool Exit { get; set; }
    private float time;
    private StandardMaterial3D material=null!;
    private readonly MeshInstance3D[] rings=new MeshInstance3D[2];
    public override void _Ready()
    {
        material=new(){AlbedoColor=Exit?new Color(.2f,1,.55f,.65f):new Color(1,.65f,.18f,.65f),Transparency=BaseMaterial3D.TransparencyEnum.Alpha,ShadingMode=BaseMaterial3D.ShadingModeEnum.Unshaded};
        for(int i=0;i<2;i++){rings[i]=new MeshInstance3D {Position=new(0,.1f+i*.015f,0),Mesh=new TorusMesh {InnerRadius=1.55f,OuterRadius=1.65f,Rings=48,RingSegments=6},MaterialOverride=material,CastShadow=GeometryInstance3D.ShadowCastingSetting.Off};AddChild(rings[i]);}
    }
    public override void _Process(double delta)
    {
        time+=(float)delta;float t=time/1.3f;if(t>=1){QueueFree();return;}
        for(int i=0;i<2;i++){float scale=1+Mathf.PosMod(t+i*.4f,1)*.5f;rings[i].Scale=new(scale,1,scale);}
        Color color=material.AlbedoColor;color.A=(1-t)*.65f;material.AlbedoColor=color;
    }
}
