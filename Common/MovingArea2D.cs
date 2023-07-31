using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Godot;

namespace ShapeGame.Common;

/// <summary>
/// A shape that can move and detect collisions based on its chape
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

    protected virtual void OnCollide(MovingArea2D initiator, CollisionObject2D collider)
    {
        
    }

    /// <summary>
    /// Moves this node by the specified vector and checks whether there is a collision with another CollisionObject2D.
    /// Emits OnCollide when collision have happened.
    /// </summary>
    /// <returns>A list of registered colliders</returns>
    public List<CollisionObject2D> MoveAndCollide(Vector2 moveVector)
    {
        _shapeCast.TargetPosition = moveVector;
        _shapeCast.ForceShapecastUpdate();
        var collisions = new List<CollisionObject2D>();
        if (_shapeCast.IsColliding())
        {
            for (var i = 0; i < _shapeCast.GetCollisionCount(); i++)
            {
                var collider = _shapeCast.GetCollider(i);
                if (collider is CollisionObject2D collisionObject)
                {
                    OnCollide(this, collisionObject);
                    collisions.Add(collisionObject);
                }
            }
        }
        Position += moveVector;
        return collisions;
    }

}