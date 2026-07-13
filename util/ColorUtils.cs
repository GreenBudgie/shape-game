public static class ColorUtils
{

    public static Color AsTransparent(this Color color)
    {
        return new Color(color.R, color.G, color.B, 0);
    }
    
    public static Color AsOpaque(this Color color)
    {
        return new Color(color.R, color.G, color.B, 1);
    }
    
}