using Godot;
using System;
namespace TiltLabyrinth;
public partial class HUDController : CanvasLayer
{
    public event Action? Restart,Calibrate,Pause,SettingsRequested,NextLevel;
    private Label timer=null!,status=null!,message=null!,level=null!,falls=null!;
    private Button pause=null!,next=null!,replay=null!;
    private ProgressBar health=null!;private ColorRect shock=null!;private float shockTime;
    private PanelContainer overlay=null!,audioPanel=null!;
    private MarginContainer safe=null!;
    public override void _Ready()
    {
        var root=new Control {MouseFilter=Control.MouseFilterEnum.Ignore};AddChild(root);root.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
        var theme=new Theme {DefaultFontSize=28};root.Theme=theme;
        var buttonStyle=new StyleBoxFlat {BgColor=new Color(.07f,.11f,.13f,.96f),CornerRadiusTopLeft=10,CornerRadiusTopRight=10,CornerRadiusBottomLeft=10,CornerRadiusBottomRight=10,ContentMarginLeft=18,ContentMarginRight=18};
        theme.SetStylebox("normal","Button",buttonStyle);var pressed=(StyleBoxFlat)buttonStyle.Duplicate();pressed.BgColor=new Color(.18f,.31f,.31f);theme.SetStylebox("pressed","Button",pressed);theme.SetStylebox("hover","Button",pressed);
        safe=new MarginContainer {MouseFilter=Control.MouseFilterEnum.Ignore};root.AddChild(safe);safe.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
        var top=new HBoxContainer {Alignment=BoxContainer.AlignmentMode.Center,SizeFlagsVertical=Control.SizeFlags.ShrinkBegin};top.AddThemeConstantOverride("separation",12);safe.AddChild(top);
        level=new Label();top.AddChild(level);timer=new Label {CustomMinimumSize=new(190,0)};top.AddChild(timer);falls=new Label {CustomMinimumSize=new(100,0)};top.AddChild(falls);
        var healthBox=new VBoxContainer {CustomMinimumSize=new(150,0)};top.AddChild(healthBox);
        health=new ProgressBar {MinValue=0,MaxValue=100,Value=100,ShowPercentage=true,CustomMinimumSize=new(150,34)};
        health.AddThemeStyleboxOverride("background",new StyleBoxFlat {BgColor=new Color(.035f,.1f,.055f),CornerRadiusTopLeft=6,CornerRadiusTopRight=6,CornerRadiusBottomLeft=6,CornerRadiusBottomRight=6});
        health.AddThemeStyleboxOverride("fill",new StyleBoxFlat {BgColor=new Color(.12f,.88f,.3f),CornerRadiusTopLeft=6,CornerRadiusTopRight=6,CornerRadiusBottomLeft=6,CornerRadiusBottomRight=6});
        health.AddThemeColorOverride("font_color",Colors.White);health.AddThemeColorOverride("font_outline_color",new Color(.01f,.1f,.025f));health.AddThemeConstantOverride("outline_size",3);
        healthBox.AddChild(new Label {Text="HEALTH",HorizontalAlignment=HorizontalAlignment.Center});healthBox.AddChild(health);
        Button(top,"Restart",()=>Restart?.Invoke());Button(top,"Calibrate",()=>Calibrate?.Invoke());pause=Button(top,"Pause",()=>Pause?.Invoke());Button(top,"Audio",()=>SettingsRequested?.Invoke());
        status=new Label {HorizontalAlignment=HorizontalAlignment.Center,MouseFilter=Control.MouseFilterEnum.Ignore};root.AddChild(status);status.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.BottomWide);
        overlay=Panel(root,new Vector2(640,390));var column=new VBoxContainer {Alignment=BoxContainer.AlignmentMode.Center};column.AddThemeConstantOverride("separation",16);overlay.AddChild(column);
        message=new Label {HorizontalAlignment=HorizontalAlignment.Center};column.AddChild(message);next=Button(column,"Next Level",()=>NextLevel?.Invoke());replay=Button(column,"Replay Level",()=>Restart?.Invoke());
        audioPanel=Panel(root,new Vector2(620,590));
        shock=new ColorRect {Color=new Color(.15f,.7f,1,.18f),Visible=false,MouseFilter=Control.MouseFilterEnum.Ignore};root.AddChild(shock);shock.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
        GetViewport().SizeChanged+=UpdateSafe;UpdateSafe();
    }
    private static PanelContainer Panel(Control parent,Vector2 size)
    {
        var panel=new PanelContainer {Visible=false,CustomMinimumSize=size};parent.AddChild(panel);panel.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.Center);panel.OffsetLeft=-size.X/2;panel.OffsetRight=size.X/2;panel.OffsetTop=-size.Y/2;panel.OffsetBottom=size.Y/2;
        panel.AddThemeStyleboxOverride("panel",new StyleBoxFlat {BgColor=new Color(.04f,.075f,.085f,.98f),CornerRadiusTopLeft=18,CornerRadiusTopRight=18,CornerRadiusBottomLeft=18,CornerRadiusBottomRight=18,ContentMarginLeft=28,ContentMarginRight=28,ContentMarginTop=24,ContentMarginBottom=24});return panel;
    }
    private void UpdateSafe()
    {
        var view=GetViewport().GetVisibleRect().Size;var window=DisplayServer.WindowGetSize();var area=DisplayServer.GetDisplaySafeArea();
        float sx=view.X/Math.Max(1,window.X),sy=view.Y/Math.Max(1,window.Y);
        bool mobile=OS.HasFeature("android");
        int left=mobile?Math.Max(0,(int)(area.Position.X*sx)):0,right=mobile?Math.Max(0,(int)((window.X-area.End.X)*sx)):0,top=mobile?Math.Max(0,(int)(area.Position.Y*sy)):0,bottom=mobile?Math.Max(0,(int)((window.Y-area.End.Y)*sy)):0;
        safe.AddThemeConstantOverride("margin_left",left+20);safe.AddThemeConstantOverride("margin_right",right+20);safe.AddThemeConstantOverride("margin_top",top+16);
        status.OffsetTop=-58-bottom;status.OffsetBottom=-14-bottom;
    }
    private static Button Button(Container parent,string text,Action action)
    {
        var b=new Button {Text=text,CustomMinimumSize=new(150,76),FocusMode=Control.FocusModeEnum.None};parent.AddChild(b);b.Pressed+=action;return b;
    }
    public void ConfigureAudio(AudioManager audio,Action close)
    {
        var column=new VBoxContainer();column.AddThemeConstantOverride("separation",8);audioPanel.AddChild(column);column.AddChild(new Label {Text="Sound & feel",HorizontalAlignment=HorizontalAlignment.Center});
        Slider(column,"Music",audio.Settings.Music,v=>audio.Settings.Music=v,audio);Slider(column,"Ambience",audio.Settings.Ambience,v=>audio.Settings.Ambience=v,audio);Slider(column,"Effects",audio.Settings.Effects,v=>audio.Settings.Effects=v,audio);
        var mute=new CheckButton {Text="Master mute",ButtonPressed=audio.Settings.Mute,CustomMinimumSize=new(0,64)};column.AddChild(mute);mute.Toggled+=v=>{audio.Settings.Mute=v;audio.Apply();audio.Settings.Save();};
        var haptic=new CheckButton {Text="Wall impact vibration",ButtonPressed=audio.Settings.Haptics,CustomMinimumSize=new(0,64)};column.AddChild(haptic);haptic.Toggled+=v=>{audio.Settings.Haptics=v;audio.Settings.Save();};Button(column,"Done",close);
    }
    private static void Slider(Container parent,string name,float value,Action<float> assign,AudioManager audio)
    {
        parent.AddChild(new Label {Text=name});var slider=new HSlider {MinValue=0,MaxValue=1,Step=.01,Value=value,CustomMinimumSize=new(540,42)};parent.AddChild(slider);slider.ValueChanged+=v=>{assign((float)v);audio.Apply();audio.Settings.Save();};
    }
    public void SetLevel(int number,string theme) {level.Text=$"Level {number} · {theme}";ClearVictory();}
    public void ShowAudio(bool show)=>audioPanel.Visible=show;
    public void Update(double elapsed,string text,bool paused,int fallCount=0,double? remaining=null) {timer.Text="Time "+TimeText(remaining??elapsed);status.Text=text;pause.Text=paused?"Resume":"Pause";falls.Text=$"Falls {fallCount}";}
    public void Complete(double elapsed,int fallCount=0,double best=0,bool canAdvance=true)
    {
        next.Visible=true;replay.Text="Replay Level";message.Text=$"Level Complete!\n{TimeText(elapsed)}  ·  {fallCount} falls"+(best>0?$"\nBest {TimeText(best)}":"");next.Disabled=!canAdvance;overlay.Visible=true;
    }
    public void SetHealth(int percent)=>health.Value=percent;
    public void Shock(){shockTime=.16f;shock.Visible=true;}
    public override void _Process(double delta){if(shockTime<=0)return;shockTime-=(float)delta;shock.Visible=shockTime>0;}
    public void ShowGameOver(double elapsed,int fallCount,string reason="Health depleted"){next.Visible=false;replay.Text="Restart";message.Text=$"Game Over\n{reason}\n{TimeText(elapsed)} · {fallCount} falls";overlay.Visible=true;}
    public void ClearVictory()=>overlay.Visible=false;
    private static string TimeText(double t)=>$"{(int)t/60:00}:{t%60:00.0}";
}
