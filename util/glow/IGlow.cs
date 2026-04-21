/// <summary>
/// Common API for glow implementations (Glow, GlowWrapper).
/// </summary>
public interface IGlow
{
    
    public static readonly NodePath StrengthPath = "Strength";
    public static readonly NodePath RadiusPath = "Radius";
    
    Color Color { get; set; }
    float Radius { get; set; }
    float Strength { get; set; }
    void TurnOff();
    
}
