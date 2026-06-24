using System.Collections.Generic;
using System.Linq;

public static class Vector2Extensions
{
    
    public static Vector2 Center(this IEnumerable<Vector2> points)
    {
        var list = points as IReadOnlyCollection<Vector2> ?? points.ToList();

        return list.Count == 0
            ? Vector2.Zero
            : list.Aggregate(Vector2.Zero, (sum, p) => sum + p) / list.Count;
    }

    public static Vector2 BoundsCenter(this IEnumerable<Vector2> points)
    {
        var list = points as IReadOnlyCollection<Vector2> ?? points.ToList();
        if (list.Count == 0) return Vector2.Zero;

        var minX = list.Min(p => p.X);
        var maxX = list.Max(p => p.X);
        var minY = list.Min(p => p.Y);
        var maxY = list.Max(p => p.Y);
        return new Vector2((minX + maxX) / 2f, (minY + maxY) / 2f);
    }

}