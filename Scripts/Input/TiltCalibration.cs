using Godot;
namespace TiltLabyrinth;
// Rotate the full gravity vector into the neutral frame; subtracting two axes
// alone loses sensitivity when the player holds the phone at an angle.
public sealed class TiltCalibration
{
    public bool Collecting { get; private set; }
    public bool Ready { get; private set; }
    public string Message { get; private set; } = "Hold comfortably, then Calibrate";
    private Vector3 sum;
    private float time;
    private int count;
    private Quaternion correction = Quaternion.Identity;
    public void Begin() { sum=Vector3.Zero; time=0; count=0; Collecting=true; Message="Hold still for calibration…"; }
    public void Sample(Vector3 gravity, float dt)
    {
        if (!Collecting) return;
        sum += gravity.Normalized(); time += dt; count++;
        if (time < 0.5f) return;
        Collecting=false;
        if (count < 10 || sum.Length()/count < 0.995f) { Message="Phone moved. Calibrate again."; return; }
        correction = new Quaternion(sum.Normalized(), Vector3.Back * -1);
        Ready=true; Message="Calibrated • gently tilt to roll";
    }
    public Vector3 Transform(Vector3 gravity) => correction * gravity;
}
