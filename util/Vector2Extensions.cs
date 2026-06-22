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
    
}