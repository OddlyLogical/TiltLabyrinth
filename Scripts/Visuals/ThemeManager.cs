namespace TiltLabyrinth;
public static class ThemeManager
{
    private static readonly System.Collections.Generic.Dictionary<int,MaterialLibrary> materials=new();
    public static MaterialLibrary Create(int level)
    {
        int style=((level-1)/3)%3;
        if(!materials.TryGetValue(style,out var library)){library=new MaterialLibrary(level);materials.Add(style,library);}
        return library;
    }
}
