public readonly struct ShakeStrength(float strength, float magnitude, float duration)
{

    private const float StrengthDelta = 0.05f;
    private const float MagnitudeDelta = 0.002f;
    private const float DurationDelta = 0.05f;

    public static readonly ShakeStrength Low = new(strength: 0.3f, magnitude: 0.01f, duration: 0.5f);
    public static readonly ShakeStrength Medium = new(strength: 0.4f, magnitude: 0.0175f, duration: 0.75f);
    public static readonly ShakeStrength High = new(strength: 0.5f, magnitude: 0.02f, duration: 1f);

    public float Strength()
    {
        return (float)GD.RandRange(strength - StrengthDelta, strength + StrengthDelta);
    }
    
    public float Magnitude()
    {
        return (float)GD.RandRange(magnitude - MagnitudeDelta, magnitude + MagnitudeDelta);
    }
    
    public float Duration()
    {
        return (float)GD.RandRange(duration - DurationDelta, duration + DurationDelta);
    }
    
}