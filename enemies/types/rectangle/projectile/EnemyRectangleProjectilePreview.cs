using Godot;
using System;

public partial class EnemyRectangleProjectilePreview : Sprite2D
{

    private static readonly PackedScene Scene = GD.Load<PackedScene>("uid://cx5i17e1gcnmb");

    private EnemyRectangle _owner = null!;
    
    public static EnemyRectangleProjectilePreview Create(EnemyRectangle owner)
    {
        var node = Scene.Instantiate<EnemyRectangleProjectilePreview>();
        node._owner = owner;
        node.Scale = Vector2.Zero;
        return node;
    }
    
    public override void _Ready()
    {
        var tween = CreateTween()
            .SetParallel()
            .SetTrans(Tween.TransitionType.Quad)
            .SetEase(Tween.EaseType.InOut);

        tween.TweenProperty(this, ScaleProperty, Vector2.One, EnemyRectangle.ProjectileChargeTime);
        tween.TweenProperty(this, RotationDegreesProperty, 360, EnemyRectangle.ProjectileChargeTime);
    }

    public void Launch()
    {
        if (!IsInstanceValid(_owner))
        {
            QueueFree(); // TODO
            return;
        }

        var projectile = EnemyRectangleProjectile.Create(_owner);
        projectile.GlobalPosition = GlobalPosition;
        ShapeGame.Instance.AddChild(projectile);
        QueueFree();
    }

}
