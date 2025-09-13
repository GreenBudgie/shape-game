public partial class DamageEffect : Label
{

    private static readonly PackedScene Scene = GD.Load<PackedScene>("uid://vtgnvnodulxr");

    private float _damage;
    
    public static DamageEffect Create(float damage, Enemy enemy)
    {
        var node = Scene.Instantiate<DamageEffect>();
        node.GlobalPosition = enemy.ToGlobal(enemy.AreaRect.RandomPoint());
        node._damage = damage;
        ShapeGame.Instance.AddChild(node);
        return node;
    }

    public override void _Ready()
    {
        Text = RoundToInt(_damage).ToString();
        const float maxRotationDelta = 30;
        RotationDegrees = RandomUtils.DeltaRange(0, maxRotationDelta / 2);
        
        const float initDuration = 0.15f;
        const float duration = 0.75f;
        
        const float minRadius = 10;
        const float maxRadius = 40;

        var positionTween = CreateTween()
            .SetTrans(Tween.TransitionType.Quad)
            .SetEase(Tween.EaseType.Out);
        
        var finalPosition = RandomUtils.RandomPointInRadii(minRadius, maxRadius);
        var finalRotation = RandomUtils.DeltaRange(RotationDegrees, maxRotationDelta / 2);
        positionTween.TweenProperty(this, PositionProperty, finalPosition, duration + initDuration)
            .AsRelative();
        positionTween.Parallel().TweenProperty(this, RotationDegreesProperty, finalRotation, duration + initDuration)
            .SetEase(Tween.EaseType.Out);
        
        Modulate = Colors.Transparent;
        Scale = Vector2.Zero;
        
        var modulateTween = CreateTween().SetTrans(Tween.TransitionType.Quad);

        modulateTween.TweenProperty(this, ModulateProperty, Colors.White, initDuration)
            .SetEase(Tween.EaseType.Out);
        modulateTween.Parallel().TweenProperty(this, ScaleProperty, Vector2.One, initDuration)
            .SetEase(Tween.EaseType.Out);

        modulateTween.TweenProperty(this, ModulateProperty, Colors.Transparent, duration)
            .SetEase(Tween.EaseType.In);
        const float minSize = 0.5f;
        modulateTween.Parallel().TweenProperty(this, ScaleProperty, new Vector2(minSize, minSize), duration)
            .SetEase(Tween.EaseType.In);
        
        modulateTween.Finished += QueueFree;
    }
    
}
