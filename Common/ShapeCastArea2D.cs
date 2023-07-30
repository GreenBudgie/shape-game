using System.Collections.Generic;
using Godot;

namespace ShapeGame.Common;

public abstract partial class ShapeCastArea2D : Area2D
{

    private ShapeCast2D _shapeCast;

    public override void _Ready()
    {
        _shapeCast = new ShapeCast2D();
        _shapeCast.Enabled = false;
        _shapeCast.Shape = GetShape();
        _shapeCast.CollideWithAreas = true;
        AddChild(_shapeCast);
    }

    /// <summary>
    /// The shape that is used for shape cast collision detection.
    /// Usually the same as the parent node collision shape.
    /// </summary>
    protected abstract Shape2D GetShape();
    
    protected virtual void OnCollide(Area2D initiator, Area2D collider)
    {
        
    }

    public List<Area2D> MoveAndCollide(Vector2 moveVector)
    {
        _shapeCast.TargetPosition = moveVector;
        _shapeCast.ForceShapecastUpdate();
        var collisions = new List<Area2D>();
        if (_shapeCast.IsColliding())
        {
            for (var i = 0; i < _shapeCast.GetCollisionCount(); i++)
            {
                var collider = _shapeCast.GetCollider(i);
                if (collider is Area2D areaCollider)
                {
                    OnCollide(this, areaCollider);
                    collisions.Add(areaCollider);
                }
            }
        }
        Position += moveVector;
        return collisions;
    }

}