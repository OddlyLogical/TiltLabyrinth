using Godot;
namespace TiltLabyrinth;
public partial class AndroidTiltInput : Node
{
    [Export] public float Sensitivity { get; set; } = 6.5f;
    [Export] public float DeadZone { get; set; } = 0.015f;
    [Export] public float FilterSeconds { get; set; } = 0.035f;
    [Export] public bool InvertX { get; set; }
    [Export] public bool InvertZ { get; set; }
    public TiltCalibration Calibration { get; } = new();
    public Vector2 Tilt { get; private set; }
    public bool Available { get; private set; }
    public string Status => !OS.HasFeature("android") ? "Android sensors required • no desktop controls" : !Available ? "Waiting for gravity / accelerometer sensor" : Calibration.Message;
    private Vector3 filtered;
    private bool seeded;
    public override void _PhysicsProcess(double delta)
    {
        float dt=(float)delta;
        if (!OS.HasFeature("android")) { Tilt=Vector2.Zero; return; }
        Vector3 raw=Input.GetGravity();
        if (!Valid(raw)) raw=Input.GetAccelerometer();
        Available=Valid(raw);
        if (!Available) { Tilt=Vector2.Zero; seeded=false; return; }
        if (!seeded) { filtered=raw; seeded=true; }
        filtered=filtered.Lerp(raw,1-Mathf.Exp(-dt/Mathf.Max(0.01f,FilterSeconds)));
        Calibration.Sample(raw,dt);
        if (!Calibration.Ready || Calibration.Collecting) { Tilt=Vector2.Zero; return; }
        Vector3 g=Calibration.Transform(filtered.Normalized());
        // Godot remaps Android sensor axes to display orientation. Screen +Y is
        // upward; world +Z points down-screen with this fixed camera.
        Vector2 v=new(g.X*(InvertX?-1:1),-g.Y*(InvertZ?-1:1));
        float len=v.Length();
        Tilt=len<=DeadZone ? Vector2.Zero : v/len*Mathf.Clamp((len-DeadZone)/(1-DeadZone)*Sensitivity,0,1);
    }
    private static bool Valid(Vector3 v) => v.IsFinite() && v.LengthSquared()>16 && v.LengthSquared()<225;
}
