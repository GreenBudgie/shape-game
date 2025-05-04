using System.Linq;

public static class CollisionObjectUtils
{
    /// <summary>
    /// Checks if the entire CollisionObject2D (all shapes) is above the playable area.
    /// Returns true only if all collision shapes are above the playable area's top edge.
    /// </summary>
    public static bool IsAboveScreen(this CollisionObject2D collisionObject)
    {
        var playableAreaTop = ShapeGame.PlayableArea.Position.Y;

        foreach (var shape in collisionObject.GetCollisionShapes())
        {
            var aabb = shape.GlobalTransform * shape.Shape.GetRect();
            if (aabb.Position.Y < playableAreaTop)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Checks if the entire CollisionObject2D (all shapes) is below the playable area.
    /// Returns true only if all collision shapes are below the playable area's bottom edge.
    /// </summary>
    public static bool IsBelowScreen(this CollisionObject2D collisionObject)
    {
        var playableAreaBottom = ShapeGame.PlayableArea.End.Y;

        foreach (var shape in collisionObject.GetCollisionShapes())
        {
            var aabb = shape.GlobalTransform * shape.Shape.GetRect();
            if (aabb.End.Y <= playableAreaBottom)
            {
                return false;
            }
        }

        return true;
    }

    private static CollisionShape2D[] GetCollisionShapes(this CollisionObject2D collisionObject)
    {
        return collisionObject.GetChildren()
            .OfType<CollisionShape2D>()
            .Where(shape => shape.Shape != null)
            .ToArray();
    }
}