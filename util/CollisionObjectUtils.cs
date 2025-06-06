using System.Collections.Generic;

public static class CollisionObjectUtils
{
    /// <summary>
    /// Checks if the entire CollisionObject2D (all shapes) is above the playable area.
    /// Returns true only if all collision shapes are above the playable area's top edge.
    /// </summary>
    public static bool IsAbovePlayableArea(this CollisionObject2D collisionObject)
    {
        var playableAreaTop = ShapeGame.PlayableArea.Position.Y;

        foreach (var rect in collisionObject.GetCollisionRects())
        {
            if (rect.End.Y >= playableAreaTop)
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
    public static bool IsBelowPlayableArea(this CollisionObject2D collisionObject)
    {
        var playableAreaBottom = ShapeGame.PlayableArea.End.Y;

        foreach (var rect in collisionObject.GetCollisionRects())
        {
            if (rect.Position.Y <= playableAreaBottom)
            {
                return false;
            }
        }

        return true;
    }
    
    /// <summary>
    /// Checks if the entire CollisionObject2D is outside the playable area (above OR below).
    /// More efficient than calling IsAbovePlayableArea() || IsBelowPlayableArea() separately.
    /// </summary>
    public static bool IsOutsidePlayableArea(this CollisionObject2D collisionObject)
    {
        var playableAreaTop = ShapeGame.PlayableArea.Position.Y;
        var playableAreaBottom = ShapeGame.PlayableArea.End.Y;
        var allAbove = true;
        var allBelow = true;
        
        foreach (var rect in collisionObject.GetCollisionRects())
        {
            if (rect.End.Y >= playableAreaTop)
            {
                allAbove = false;
            }

            if (rect.Position.Y <= playableAreaBottom)
            {
                allBelow = false;
            }

            if (!allAbove && !allBelow)
            {
                return false;
            }
        }

        return allAbove || allBelow;
    }

    public static bool GetCollisionLayerValue(this CollisionObject2D collisionObject, CollisionLayers collisionLayer)
    {
        return collisionObject.GetCollisionLayerValue((int)collisionLayer);
    }

    private static IEnumerable<Rect2> GetCollisionRects(this CollisionObject2D collisionObject)
    {
        foreach (var shapeOwnerId in collisionObject.GetShapeOwners())
        {
            var shapeOwnerUid = (uint)shapeOwnerId;
            for (var i = 0; i < collisionObject.ShapeOwnerGetShapeCount(shapeOwnerUid); i++)
            {
                var shape = collisionObject.ShapeOwnerGetShape(shapeOwnerUid, i);
                var transform = collisionObject.ShapeOwnerGetTransform(shapeOwnerUid);
                var rect = transform * shape.GetRect();
                rect.Position += collisionObject.GlobalPosition;
                yield return rect;
            }
        }
    }
}