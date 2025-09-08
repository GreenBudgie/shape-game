public readonly record struct ShakeStrength(float Strength, float Magnitude, float Duration)
{
    
    public static readonly ShakeStrength Low = new(Strength: 0.1f, Magnitude: 0.1f, Duration: 0.2f);
    public static readonly ShakeStrength Medium = new(Strength: 0.2f, Magnitude: 0.15f, Duration: 0.35f);
    public static readonly ShakeStrength High = new(Strength: 0.3f, Magnitude: 0.2f, Duration: 0.5f);
    
}