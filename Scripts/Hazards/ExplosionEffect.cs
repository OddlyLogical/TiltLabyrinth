using Godot;
namespace TiltLabyrinth;
public partial class ExplosionEffect : Node3D
{
    [Export] public float BlastScale { get; set; }=1;
    private float time,life;
    private MeshInstance3D flash=null!,fire=null!,wave=null!;
    private StandardMaterial3D flashMat=null!,fireMat=null!,waveMat=null!,sparkMat=null!,smokeMat=null!;
    private readonly MeshInstance3D[] sparks=new MeshInstance3D[12],smoke=new MeshInstance3D[6];
    private readonly Vector3[] velocity=new Vector3[12],smokeOffset=new Vector3[6];
    private static StandardMaterial3D Glow(Color color)=>new(){AlbedoColor=color,Transparency=BaseMaterial3D.TransparencyEnum.Alpha,ShadingMode=BaseMaterial3D.ShadingModeEnum.Unshaded};
    private MeshInstance3D Part(Mesh mesh,Material material)
    {
        var part=new MeshInstance3D {Mesh=mesh,MaterialOverride=material,CastShadow=GeometryInstance3D.ShadowCastingSetting.Off};AddChild(part);return part;
    }
    public override void _Ready()
    {
        BlastScale=Mathf.Clamp(BlastScale,.5f,2);Scale=Vector3.One*BlastScale;life=1.35f+.15f*(BlastScale-1);
        flashMat=Glow(new Color(1,.95f,.65f,.95f));fireMat=Glow(new Color(1,.32f,.025f,.75f));waveMat=Glow(new Color(1,.7f,.18f,.7f));sparkMat=Glow(new Color(1,.65f,.08f,1));smokeMat=Glow(new Color(.12f,.13f,.15f,0));
        flash=Part(new SphereMesh {Radius=.5f,Height=1,RadialSegments=16,Rings=8},flashMat);flash.Position=new(0,.5f,0);
        fire=Part(new SphereMesh {Radius=.5f,Height=1,RadialSegments=20,Rings=10},fireMat);fire.Position=new(0,.6f,0);
        wave=Part(new TorusMesh {InnerRadius=.48f,OuterRadius=.56f,Rings=48,RingSegments=6},waveMat);wave.Position=new(0,.03f,0);
        var shard=new BoxMesh {Size=new(.07f,.07f,.25f)};var puff=new SphereMesh {Radius=.5f,Height=1,RadialSegments=12,Rings=6};
        for(int i=0;i<sparks.Length;i++)
        {
            float angle=Mathf.Tau*i/sparks.Length;
            sparks[i]=Part(shard,sparkMat);sparks[i].Position=new(0,.45f,0);sparks[i].Rotation=new(.6f*i,angle,.4f*i);
            velocity[i]=new(Mathf.Cos(angle)*(3+i%3),2+i%4*.4f,Mathf.Sin(angle)*(3+i%3));
        }
        for(int i=0;i<smoke.Length;i++)
        {
            float angle=Mathf.Tau*i/smoke.Length;smokeOffset[i]=new(Mathf.Cos(angle)*.7f,.5f+(.1f*i),Mathf.Sin(angle)*.7f);
            smoke[i]=Part(puff,smokeMat);smoke[i].Position=smokeOffset[i];
        }
    }
    public override void _Process(double delta)
    {
        float dt=(float)delta;time+=dt;if(time>=life){QueueFree();return;}
        float f=Mathf.Clamp(time/.14f,0,1);flash.Scale=Vector3.One*Mathf.Lerp(.5f,3.2f,f);flashMat.AlbedoColor=new Color(1,.95f,.65f,(1-f)*.95f);
        float flame=Mathf.Clamp(time/.6f,0,1);fire.Scale=Vector3.One*Mathf.Lerp(.7f,5.1f,Mathf.Sqrt(flame));fireMat.AlbedoColor=new Color(1,Mathf.Lerp(.6f,.12f,flame),.025f,(1-flame)*.75f);
        float ring=Mathf.Clamp(time/.85f,0,1);wave.Scale=new Vector3(1,1,1)*Mathf.Lerp(1,7,ring);waveMat.AlbedoColor=new Color(1,.7f,.18f,(1-ring)*.7f);
        sparkMat.AlbedoColor=new Color(1,.65f,.08f,Mathf.Clamp(1-time/1.05f,0,1));
        for(int i=0;i<sparks.Length;i++)
        {
            velocity[i]+=Vector3.Down*9*dt;sparks[i].Position+=velocity[i]*dt;sparks[i].RotateY(dt*6);
            if(sparks[i].Position.Y<.04f){Vector3 pos=sparks[i].Position;pos.Y=.04f;sparks[i].Position=pos;velocity[i]=new(velocity[i].X*.7f,Mathf.Abs(velocity[i].Y)*.25f,velocity[i].Z*.7f);}
        }
        float haze=Mathf.Clamp((time-.12f)/(life-.12f),0,1);smokeMat.AlbedoColor=new Color(.12f,.13f,.15f,Mathf.Sin(haze*Mathf.Pi)*.32f);
        for(int i=0;i<smoke.Length;i++){smoke[i].Position=smokeOffset[i]+new Vector3(smokeOffset[i].X*haze,.8f*haze,smokeOffset[i].Z*haze);smoke[i].Scale=Vector3.One*Mathf.Lerp(.6f,2,haze);}
    }
}
