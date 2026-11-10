using Godot;
namespace TiltLabyrinth;
public partial class BoardScenery : Node3D
{
    public MazeGrid Grid { get; set; }=null!;
    private void Box(Vector3 position,Vector3 size,Material material)
    {
        AddChild(new MeshInstance3D {Position=position,Mesh=new BoxMesh {Size=size},MaterialOverride=material});
    }
    public override void _Ready()
    {
        var desk=MaterialLibrary.Stone(new Color(.045f,.055f,.062f),new Color(.16f,.18f,.18f),415,.07f);
        var frame=MaterialLibrary.Metal(new Color(.16f,.19f,.21f),416);
        var screw=MaterialLibrary.Metal(new Color(.54f,.58f,.6f),417);
        var slot=new StandardMaterial3D {AlbedoColor=new Color(.015f,.02f,.025f),Roughness=.85f};
        Box(new(0,-4.6f,0),new(Grid.Width+90,1,Grid.Depth+70),desk);
        foreach(float side in new[]{-1f,1f})
        {
            Box(new(side*(Grid.Width/2+.65f),-.35f,0),new(.9f,.8f,Grid.Depth+2.2f),frame);
            Box(new(0,-.35f,side*(Grid.Depth/2+.65f)),new(Grid.Width+.4f,.8f,.9f),frame);
        }
        foreach(float x in new[]{-1f,1f})foreach(float z in new[]{-1f,1f})
        {
            Vector3 point=new(x*(Grid.Width/2+.65f),0,z*(Grid.Depth/2+.65f));
            AddChild(new MeshInstance3D {Position=point+Vector3.Down*2.1f,Mesh=new CylinderMesh {TopRadius=.28f,BottomRadius=.36f,Height=3.7f},MaterialOverride=frame});
            AddChild(new MeshInstance3D {Position=point+Vector3.Up*.09f,Mesh=new CylinderMesh {TopRadius=.26f,BottomRadius=.26f,Height=.08f},MaterialOverride=screw});
            Box(point+Vector3.Up*.135f,new(.33f,.015f,.055f),slot);
            Box(point+Vector3.Up*.135f,new(.055f,.015f,.33f),slot);
        }
    }
}
