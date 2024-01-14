using System;
using System.Collections.Generic;
using Godot;

namespace ShapeGame.Common;

/// <summary>
/// A character that can move and detect collisions using ShapeCast2D.
/// This approach guarantees that the node will detect collision at any move speed.
/// Useful for extremely fast moving objects or at low fps. 
/// Can collide with any CollisionObject2D.
/// </summary>
public abstract partial class ShapeCastCharacterBody2D : CharacterBody2D
{

    private ShapeCast2D _shapeCast;

    public override void _Ready()
    {
        _shapeCast = new ShapeCast2D();
        _shapeCast.Shape = GetShape();
        _shapeCast.Enabled = false;
        _shapeCast.CollideWithAreas = true;
        _shapeCast.CollisionMask = GetCustomCollisionMask();
        _shapeCast.TargetPosition = Vector2.Zero;
        AddChild(_shapeCast);
    }

    /// <summary>
    /// Override this method to provide custom collision mask for the shape cast.
    /// Will use the same collision mask if not overriden.
    /// </summary>
    /// <returns>A collision mask</returns>
    protected virtual uint GetCustomCollisionMask()
    {
        return CollisionMask;
    }

    /// <summary>
    /// The shape that is used for shape cast collision detection.
    /// Usually the same as the node collision shape, but it is possible to override this logic.
    /// </summary>
    protected virtual Shape2D GetShape()
    {
        foreach (var childNode in GetChildren())
        {
            if (childNode is CollisionPolygon2D collisionPolygon)
            {
                var shape = new ConvexPolygonShape2D();
                shape.Points = collisionPolygon.Polygon;
                return shape;
            }

            if (childNode is CollisionShape2D collisionShape)
            {
                return collisionShape.Shape;
            }
        }

        throw new Exception("No shape is configured on Area2D");
    }

    /// <summary>
    /// Called when some moving area collides with CollisionObject2D.
    /// </summary>
    /// <param name="collider">An object with which the area collided</param>
    protected virtual void OnCollide(CollisionObject2D collider)
    {
        
    }

    /// <summary>
    /// Returns all collisions with another CollisionObject2Ds if this character is to be moved by specified vector.
    /// </summary>
    /// <returns>A list of collisions</returns>
    public List<CollisionObject2D> GetCollisions(Vector2 moveVector)
    {
        _shapeCast.TargetPosition = moveVector.Rotated(-Rotation);
        _shapeCast.ForceShapecastUpdate();
        var collisions = new List<CollisionObject2D>();
        if (!_shapeCast.IsColliding()) 
        {
            return collisions;
        }
        for (var i = 0; i < _shapeCast.GetCollisionCount(); i++)
        {
            var collider = _shapeCast.GetCollider(i);
            if (collider is not CollisionObject2D collisionObject)
            {
                continue;
            }
            collisions.Add(collisionObject);
        }
        return collisions;
    }

    private void Collide(CollisionObject2D collider)
    {
        //TODO Remove double collisions
        OnCollide(collider);
        
    }

}