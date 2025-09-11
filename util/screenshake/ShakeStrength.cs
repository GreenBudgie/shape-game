public readonly record struct ShakeStrength(float BaseStrength, float BaseMagnitude, float BaseDuration)
{
    private const float StrengthDelta = 0.05f;
    private const float MagnitudeDelta = 0.002f;
    private const float DurationDelta = 0.05f;

    public static readonly ShakeStrength Low = new(BaseStrength: 0.3f, BaseMagnitude: 0.01f, BaseDuration: 0.5f);
    public static readonly ShakeStrength Medium = new(BaseStrength: 0.4f, BaseMagnitude: 0.0175f, BaseDuration: 0.75f);
    public static readonly ShakeStrength High = new(BaseStrength: 0.5f, BaseMagnitude: 0.02f, BaseDuration: 1f);

    public static ShakeStrength FromRatio(float ratio)
    {
        var strength = Lerp(Low.BaseStrength, High.BaseStrength, ratio);
        var magnitude = Lerp(Low.BaseMagnitude, High.BaseMagnitude, ratio);
        var duration = Lerp(Low.BaseDuration, High.BaseDuration, ratio);
        return new ShakeStrength(strength, magnitude, duration);
    }

    public float Strength()
    {
        return (float)GD.RandRange(BaseStrength - StrengthDelta, BaseStrength + StrengthDelta);
    }

    public float Magnitude()
    {
        return (float)GD.RandRange(BaseMagnitude - MagnitudeDelta, BaseMagnitude + MagnitudeDelta);
    }

    public float Duration()
    {
        return (float)GD.RandRange(BaseDuration - DurationDelta, BaseDuration + DurationDelta);
    }
}