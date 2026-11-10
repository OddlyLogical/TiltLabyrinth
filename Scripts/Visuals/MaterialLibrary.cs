using Godot;
namespace TiltLabyrinth;
public sealed class MaterialLibrary
{
    public string Name { get; }
    public Material Floor { get; }
    public StandardMaterial3D Wall { get; }
    public StandardMaterial3D Bumper { get; }
    public StandardMaterial3D Rim { get; }=new(){AlbedoColor=new Color(.64f,.4f,.19f),Metallic=.55f,Roughness=.4f};
    private static FastNoiseLite Noise(int seed,float frequency)=>new(){Seed=seed,NoiseType=FastNoiseLite.NoiseTypeEnum.Perlin,Frequency=frequency,FractalOctaves=4};
    public static StandardMaterial3D Stone(Color dark,Color light,int seed,float tiling=.12f)
    {
        var noise=Noise(seed,.035f);
        var color=new NoiseTexture2D {Width=256,Height=256,Seamless=true,Noise=noise,ColorRamp=new Gradient {Colors=new[]{dark,light}}};
        var normal=new NoiseTexture2D {Width=256,Height=256,Seamless=true,Noise=noise,AsNormalMap=true,BumpStrength=1.2f};
        return new(){AlbedoTexture=color,NormalEnabled=true,NormalTexture=normal,NormalScale=.32f,Roughness=.78f,Uv1Triplanar=true,Uv1WorldTriplanar=true,Uv1Scale=Vector3.One*tiling,TextureFilter=BaseMaterial3D.TextureFilterEnum.LinearWithMipmapsAnisotropic};
    }
    public static StandardMaterial3D Metal(Color tint,int seed)
    {
        const int n=256;var random=new System.Random(seed);byte[] pixels=new byte[n*n*3];
        for(int y=0;y<n;y++)
        {
            float brush=.8f+(float)random.NextDouble()*.15f;
            for(int x=0;x<n;x++){byte value=(byte)(255*Mathf.Clamp(brush+((float)random.NextDouble()-.5f)*.045f,0,1));int i=(y*n+x)*3;pixels[i]=value;pixels[i+1]=value;pixels[i+2]=value;}
        }
        using var image=Image.CreateFromData(n,n,false,Image.Format.Rgb8,pixels);image.GenerateMipmaps();
        return new(){AlbedoColor=tint,AlbedoTexture=ImageTexture.CreateFromImage(image),Metallic=.82f,Roughness=.3f,Uv1Triplanar=true,Uv1WorldTriplanar=true,Uv1Scale=new(.16f,.16f,.16f),TextureFilter=BaseMaterial3D.TextureFilterEnum.LinearWithMipmapsAnisotropic};
    }
    public MaterialLibrary(int level)
    {
        int style=((level-1)/3)%3;Name=style==0?"Glass":style==1?"Brushed metal":"Stone";
        if(style==0)
        {
            var frost=new NoiseTexture2D {Width=256,Height=256,Seamless=true,Noise=Noise(52,.12f),AsNormalMap=true,BumpStrength=.3f};
            Floor=new StandardMaterial3D {AlbedoColor=new Color(.45f,.7f,.8f,.42f),Transparency=BaseMaterial3D.TransparencyEnum.AlphaDepthPrePass,Roughness=.075f,Metallic=.03f,MetallicSpecular=.85f,NormalEnabled=true,NormalTexture=frost,NormalScale=.035f,Uv1Triplanar=true,Uv1WorldTriplanar=true,Uv1Scale=Vector3.One*.2f};
            Wall=new(){AlbedoColor=new Color(.58f,.86f,.95f,.45f),Transparency=BaseMaterial3D.TransparencyEnum.AlphaDepthPrePass,Roughness=.1f,Metallic=.04f,MetallicSpecular=.85f};
        }
        else if(style==1)
        {
            Floor=Metal(new Color(.44f,.51f,.57f),74);Wall=Metal(new Color(.72f,.77f,.8f),75);
        }
        else
        {
            Floor=Stone(new Color(.2f,.23f,.25f),new Color(.46f,.47f,.43f),111);
            Wall=Stone(new Color(.41f,.43f,.42f),new Color(.65f,.64f,.59f),112,.23f);
        }
        Bumper=new(){AlbedoColor=new Color(.28f,.35f,.39f),Metallic=.5f,Roughness=.35f};
    }
}
