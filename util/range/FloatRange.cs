public readonly record struct FloatRange(float Min, float Max)
{
    public FloatRange(float value) : this(value, value)
    {
    }

    public float Random()
    {
        return (float)GD.RandRange(Min, Max);
    }
    
    public static implicit operator FloatRange(float from) => new(from);
}