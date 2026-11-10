using Godot;
using System;
namespace TiltLabyrinth;
public partial class BallController : RigidBody3D
{
    [Export] public float Radius { get; set; } = MazeGrid.BallRadius;
    [Export] public float MaximumAcceleration { get; set; } = 27;
    [Export] public float MaximumSpeed { get; set; } = 12;
    public AndroidTiltInput? InputSource { get; set; }
    public bool Active { get; set; }
    public bool Falling { get; private set; }
    public float HorizontalSpeed => new Vector2(LinearVelocity.X,LinearVelocity.Z).Length();
    public event Action<float>? Impact;
    public event Action<float>? WallImpact;
    private Node3D visuals=null!;
    private Vector3 dropStart,dropTarget;
    private float dropTime,dropDuration,pullPhase;
    public bool DropAnimating { get; private set; }
    public Vector3 DisplayPosition=>visuals.GlobalPosition;
    private float cooldown,wallShockTime;
    private Vector3 previousVelocity;
    private bool resetPending;
    private Vector3 spawn;
    public bool ResetPending=>resetPending;
    public void ConfigureLevel(Vector3 start) {spawn=start;ResetBall(start);}
    public override void _Ready()
    {
        Mass=1; LinearDamp=0.4f; AngularDamp=0.25f; ContinuousCd=true;CanSleep=false;
        ContactMonitor=true; MaxContactsReported=8; CollisionLayer=2; CollisionMask=1;
        PhysicsMaterialOverride=new PhysicsMaterial {Friction=0.7f,Bounce=0.04f};
        visuals=new Node3D {Name="BallVisuals"};AddChild(visuals);
        AddChild(new CollisionShape3D {Shape=new SphereShape3D {Radius=Radius}});
        visuals.AddChild(new MeshInstance3D {Mesh=new SphereMesh {Radius=Radius,Height=Radius*2,RadialSegments=48,Rings=24},MaterialOverride=GD.Load<Material>("res://Materials/BallMaterial.tres")});

    }
    public void ResetBall(Vector3? location=null)
    {
        if(location.HasValue)spawn=location.Value;
        DropAnimating=false;if(visuals!=null){visuals.Transform=Transform3D.Identity;visuals.Visible=true;}
        wallShockTime=0;Active=false;Falling=false;CollisionLayer=2;CollisionMask=1;Freeze=false;Sleeping=false;resetPending=true;
    }
    public void CaptureByHole()
    {
        if(!Active || Falling)return;
        Falling=true;
        // The requested danger zone captures before physical rim contact.
        SetDeferred(CollisionObject3D.PropertyName.CollisionMask,0);
        SetDeferred(CollisionObject3D.PropertyName.CollisionLayer,0);
    }
    public void BeginDropVisual(Vector3 center,bool exit)
    {
        dropStart=GlobalPosition;dropTarget=new(center.X,dropStart.Y,center.Z);dropTime=0;
        pullPhase=!exit && new Vector2(dropStart.X-center.X,dropStart.Z-center.Z).Length()>1?.35f:0;
        dropDuration=exit?1.3f:pullPhase>0?1.4f:1.15f;DropAnimating=true;
    }
    public override void _Process(double delta)
    {
        if(!DropAnimating)return;
        dropTime+=(float)delta;float t=Mathf.Clamp(dropTime/dropDuration,0,1),sink=Mathf.Clamp((t-pullPhase)/(1-pullPhase),0,1),ease=sink*sink;
        Vector3 point=dropStart.Lerp(dropTarget,Mathf.SmoothStep(0,1,pullPhase>0?Mathf.Clamp(t/pullPhase,0,1):t));float orbit=.25f*Mathf.Sin(Mathf.Pi*t)*(1-t);
        point.X+=Mathf.Cos(t*Mathf.Tau*1.6f)*orbit;point.Z+=Mathf.Sin(t*Mathf.Tau*1.6f)*orbit;
        point.Y=dropStart.Y-(Radius*2+1)*ease;
        visuals.GlobalPosition=point;visuals.Scale=Vector3.One*(1-.8f*ease);visuals.RotateY((float)delta*9);
        if(t>=1){DropAnimating=false;visuals.Visible=false;}
    }
    public override void _IntegrateForces(PhysicsDirectBodyState3D state)
    {
        if(resetPending) { state.Transform=new Transform3D(Basis.Identity,spawn); state.LinearVelocity=Vector3.Zero; state.AngularVelocity=Vector3.Zero; previousVelocity=Vector3.Zero; resetPending=false; return; }
        cooldown-=state.Step;wallShockTime-=state.Step;
        if(!Active) { state.LinearVelocity=Vector3.Zero; state.AngularVelocity=Vector3.Zero; return; }
        // Natural falling is detected below the floor; a requested hole
        // danger zone may already have captured the ball before rim contact.
        if(state.Transform.Origin.Y<Radius*.4f)Falling=true;
        if(Falling)return;
        Vector2 tilt=InputSource?.Tilt??Vector2.Zero;
        state.ApplyCentralForce(new Vector3(tilt.X,0,tilt.Y)*MaximumAcceleration*Mass);
        Vector3 v=state.LinearVelocity; Vector2 horizontal=new(v.X,v.Z);
        if(horizontal.Length()>MaximumSpeed) { horizontal=horizontal.Normalized()*MaximumSpeed; state.LinearVelocity=new(horizontal.X,v.Y,horizontal.Y); }
        float strength=0,wallStrength=0;bool touchingWall=false;
        for(int i=0;i<state.GetContactCount();i++)
        {
            // Collider identity reliably separates electrified walls from floor
            // seams and bumpers even while the sphere's basis is rotating.
            bool electric=state.GetContactColliderObject(i) is Node node && node.IsInGroup("electrified_walls");
            touchingWall|=electric;
            Vector3 normal=state.GetContactLocalNormal(i);
            if(electric || Mathf.Abs(normal.Y)<.5f)
            {
                float hit=Mathf.Max(-previousVelocity.Dot(normal),state.GetContactImpulse(i).Length()/Mass);
                strength=Mathf.Max(strength,hit);if(electric)wallStrength=Mathf.Max(wallStrength,hit);
            }
        }
        if(strength>.35f && cooldown<=0){Impact?.Invoke(strength);cooldown=.12f;}
        // Electricity depends on contact, not impact velocity. A stationary ball
        // pressed against a wall keeps receiving a pulse every half second.
        if(touchingWall && wallShockTime<=0){WallImpact?.Invoke(Mathf.Max(2,wallStrength));wallShockTime=.5f;}
        previousVelocity=state.LinearVelocity;
    }
}
