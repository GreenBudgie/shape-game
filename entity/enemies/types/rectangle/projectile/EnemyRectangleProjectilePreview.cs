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
            .SetTrans(Tween.TransitionType.Quint)
            .SetEase(Tween.EaseType.Out);

        tween.TweenProperty(this, ScaleProperty, Vector2.One, EnemyRectangle.ProjectileChargeTime);
        tween.TweenProperty(this, RotationDegreesProperty, 360, EnemyRectangle.ProjectileChargeTime);

        _owner.Connect(Enemy.SignalName.Destroyed, Callable.From(Remove));
    }

    public void Launch()
    {
        if (!IsInstanceValid(_owner))
        {
            return;
        }

        var projectile = EnemyRectangleProjectile.Create(_owner);
        projectile.GlobalPosition = GlobalPosition;
        ShapeGame.Instance.AddChild(projectile);
        QueueFree();
    }

    private void Remove()
    {
        DissolveEffect.Dissolve(this, this, 0.25f);
    }

}
