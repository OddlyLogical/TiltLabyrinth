using Godot;
using System;
namespace TiltLabyrinth;
public partial class GoalTrigger : Area3D
{
    public static event Action<BallController>? Reached;
    [Export] public float CaptureRadius { get; set; }=.55f;
    private BallController? candidate;
    public override void _Ready()
    {
        BodyEntered+=body=>{if(body is BallController ball)candidate=ball;};
        BodyExited+=body=>{if(body==candidate)candidate=null;};
    }
    public override void _PhysicsProcess(double delta)
    {
        var ball=candidate;if(ball==null || !ball.Active || ball.Falling)return;
        Vector3 offset=ball.GlobalPosition-GlobalPosition;
        if(new Vector2(offset.X,offset.Z).LengthSquared()<=CaptureRadius*CaptureRadius && offset.Y>0 && offset.Y<2.5f)Reached?.Invoke(ball);
    }
}
