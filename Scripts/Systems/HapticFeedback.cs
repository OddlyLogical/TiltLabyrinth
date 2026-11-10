using Godot;
namespace TiltLabyrinth;
public sealed class HapticFeedback
{
    private readonly AudioSettings settings;
    private ulong lastImpact;
    public HapticFeedback(AudioSettings settings){this.settings=settings;}
    public static (int Duration,float Amplitude) Response(float strength)
    {
        float t=Mathf.Clamp((strength-.8f)/8,0,1);
        return strength<.8f?(0,0):((int)Mathf.Lerp(12,38,t),Mathf.Lerp(.16f,.65f,t));
    }
    public void OnImpact(float strength)
    {
        if(!settings.Haptics || !OS.HasFeature("android"))return;
        ulong now=Time.GetTicksMsec();if(now-lastImpact<160)return;
        var response=Response(strength);if(response.Duration==0)return;
        lastImpact=now;Input.VibrateHandheld(response.Duration,response.Amplitude);
    }
}
