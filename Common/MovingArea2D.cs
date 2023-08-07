using System;
using System.Collections.Generic;
using Godot;

namespace ShapeGame.Common;

/// <summary>
/// A shape that can move and detect collisions using ShapeCast2D.
/// This approach guarantees that the node will detect collision at any move speed.
/// Useful for extremely fast moving objects or at low fps. 
/// Can collide with any CollisionObject2D.
/// </summary>
public abstract partial class MovingArea2D : Area2D
{

    private ShapeCast2D _shapeCast;

    public override void _Ready()
    {
        _shapeCast = new ShapeCast2D();
        _shapeCast.Shape = GetShape();
        _shapeCast.Enabled = false;
        _shapeCast.CollideWithAreas = true;
        _shapeCast.CollisionMask = CollisionMask;
        _shapeCast.TargetPosition = Vector2.Zero;
        AddChild(_shapeCast);
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
    /// Note that if two MovingArea2Ds collide, this method might be called twice if
    /// each shape registered collision with one another.
    /// </summary>
    /// <param name="collider">An object with which the area collided</param>
    protected virtual void OnCollide(CollisionObject2D collider)
    {
        
    }

    /// <summary>
    /// Moves this node by the specified vector and checks whether there is a collision with another CollisionObject2D.
    /// Emits OnCollide when collision has happened.
    /// </summary>
    /// <returns>A list of registered collisions</returns>
    public List<CollisionObject2D> MoveAndCollide(Vector2 moveVector)
    {
        _shapeCast.TargetPosition = moveVector.Rotated(-Rotation);
        _shapeCast.ForceShapecastUpdate();
        var collisions = new List<CollisionObject2D>();
        if (_shapeCast.IsColliding())
        {
            for (var i = 0; i < _shapeCast.GetCollisionCount(); i++)
            {
                var collider = _shapeCast.GetCollider(i);
                if (collider is not CollisionObject2D collisionObject)
                {
                    continue;
                }
                OnCollide(collisionObject);
                if (collisionObject is MovingArea2D movingArea)
                {
                    movingArea.OnCollide(this);
                }
                collisions.Add(collisionObject);
            }
        }
        Position += moveVector;
        return collisions;
    }

}