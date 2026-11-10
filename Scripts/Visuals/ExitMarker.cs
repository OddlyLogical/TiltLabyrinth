using Godot;
namespace TiltLabyrinth;
public partial class ExitMarker : Node3D
{
    public override void _Ready()
    {
        var dark=new StandardMaterial3D {AlbedoColor=new Color(.055f,.07f,.09f),Roughness=.65f};
        var gold=new StandardMaterial3D {AlbedoColor=new Color(.18f,1,.48f),EmissionEnabled=true,Emission=new Color(.04f,.75f,.2f),ShadingMode=BaseMaterial3D.ShadingModeEnum.Unshaded};
        AddChild(new MeshInstance3D {Position=new(0,.035f,0),Mesh=new CylinderMesh {TopRadius=1.95f,BottomRadius=1.95f,Height=.035f},MaterialOverride=dark});
        AddChild(new MeshInstance3D {Position=new(0,.065f,0),Mesh=new TorusMesh {InnerRadius=1.72f,OuterRadius=1.94f,Rings=64,RingSegments=8},MaterialOverride=gold});
        AddChild(new MeshInstance3D {Position=new(0,.075f,0),Mesh=new TorusMesh {InnerRadius=.65f,OuterRadius=.77f,Rings=40,RingSegments=6},MaterialOverride=gold});
        AddChild(new MeshInstance3D {Position=new(0,.08f,0),Mesh=new CylinderMesh {TopRadius=.28f,BottomRadius=.28f,Height=.025f},MaterialOverride=gold});
        AddChild(new Label3D {Name="ExitLabel",Text="EXIT",Position=new(0,.25f,-2.55f),FontSize=90,PixelSize=.012f,Modulate=new Color(.45f,1,.65f),OutlineSize=12,OutlineModulate=new Color(.015f,.025f,.035f),Billboard=BaseMaterial3D.BillboardModeEnum.Enabled,NoDepthTest=true});
    }
}
