public static class RandomUtils
{
    /// <summary>
    /// Generates a random sign: +1 or -1 with equal probability.
    /// </summary>
    /// <returns>+1 if a random float is greater than 0.5, otherwise -1</returns>
    public static int RandomSign()
    {
        return GD.Randf() > 0.5f ? -1 : 1;
    }

    /// <summary>
    /// Generates a random float within a range centered around the specified value.
    /// </summary>
    /// <param name="value">The center value of the range</param>
    /// <param name="delta">The maximum deviation from the center value</param>
    /// <returns>A random float between (value - delta) and (value + delta)</returns>
    public static float DeltaRange(float value, float delta)
    {
        return (float)GD.RandRange(value - delta, value + delta);
    }
    
    /// <summary>
    /// Generates a random float within a range centered around the specified value, 
    /// multiplied by a random sign (+1 or -1).
    /// </summary>
    /// <param name="value">The center value of the range</param>
    /// <param name="delta">The maximum deviation from the center value</param>
    /// <returns>A random float between -(value + delta) and (value + delta), inclusive</returns>
    public static float RandomSignedDeltaRange(float value, float delta)
    {
        return DeltaRange(value, delta) * RandomSign();
    }
}