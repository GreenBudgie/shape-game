using System.Collections.Generic;
using System.Linq;

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

    /// <summary>
    /// Selects a random point inside the given rect
    /// </summary>
    public static Vector2 RandomPoint(this Rect2 rect)
    {
        var randomX = (float)GD.RandRange(rect.Position.X, rect.End.X);
        var randomY = (float)GD.RandRange(rect.Position.Y, rect.End.Y);
        return new Vector2(randomX, randomY);
    }
    
    /// <summary>
    /// Generates a random point uniformly distributed within a circle of the given radius.
    /// </summary>
    /// <param name="radius">The radius of the circle (must be non-negative)</param>
    /// <returns>A Vector2 point inside the circle</returns>
    public static Vector2 RandomPointInRadius(float radius)
    {
        if (radius <= 0)
        {
            return Vector2.Zero;
        }

        var theta = GD.Randf() * Tau;
        var r = Sqrt(GD.Randf()) * radius;
        var (sinTheta, cosTheta) = SinCos(theta);
        return new Vector2(r * cosTheta, r * sinTheta);
    }
    
    /// <summary>
    /// Generates a random point uniformly distributed within an annulus (donut shape) defined by minimum and maximum radii.
    /// </summary>
    /// <param name="minRadius">The minimum radius (inner radius, must be non-negative and less than or equal to maxRadius)</param>
    /// <param name="maxRadius">The maximum radius (outer radius, must be non-negative)</param>
    /// <returns>A Vector2 point inside the annulus</returns>
    public static Vector2 RandomPointInRadii(float minRadius, float maxRadius)
    {
        if (maxRadius <= 0 || minRadius > maxRadius)
        {
            return Vector2.Zero;
        }
        
        minRadius = Max(minRadius, 0);

        var theta = GD.Randf() * Tau;
        var minSq = minRadius * minRadius;
        var maxSq = maxRadius * maxRadius;
        var r = Sqrt(GD.Randf() * (maxSq - minSq) + minSq);
        var (sinTheta, cosTheta) = SinCos(theta);
        return new Vector2(r * cosTheta, r * sinTheta);
    }
        
    public static T GetRandom<T>(this IEnumerable<T> enumerable)
    {
        return enumerable.ToList().GetRandom();
    }
    
    public static T GetRandom<T>(this List<T> list)
    {
        var index = GD.RandRange(0, list.Count - 1);
        return list[index];
    }
    
}