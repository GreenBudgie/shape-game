/// <summary>
/// Common API for glow implementations (Glow, GlowWrapper).
/// </summary>
public interface IGlow
{
    
    public static readonly NodePath ColorProperty = "Color";
    public static readonly NodePath StrengthProperty = "Strength";
    public static readonly NodePath RadiusProperty = "Radius";
    
    Color Color { get; set; }
    float Radius { get; set; }
    float Strength { get; set; }
    void TurnOff();
    
}
