public partial class PopupLabel : Label
{
    
    private static readonly PackedScene Scene = GD.Load<PackedScene>("uid://dbaolxtt1kh54");
    
    public static PopupLabel Create(Vector2 spawnPosition, string text)
    {
        var node = Scene.Instantiate<PopupLabel>();
        node.GlobalPosition = spawnPosition;
        node.Text = text;
        ShapeGame.Instance.AddChild(node);
        return node;
    }

    public PopupLabel SetColor(Color color)
    {
        AddThemeColorOverride("font_color", color);
        return this;
    }

    public override void _Ready()
    {
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
        positionTween.TweenPosition(this, finalPosition, duration + initDuration)
            .AsRelative();
        positionTween.Parallel().TweenRotationDegrees(this, finalRotation, duration + initDuration)
            .SetEase(Tween.EaseType.Out);
        
        Modulate = Colors.Transparent;
        Scale = Vector2.Zero;
        
        var modulateTween = CreateTween().SetTrans(Tween.TransitionType.Quad);

        modulateTween.FadeIn(this, initDuration).SetEase(Tween.EaseType.Out);
        modulateTween.Parallel().TweenScaleReset(this, initDuration).SetEase(Tween.EaseType.Out);

        modulateTween.FadeOut(this, duration).SetEase(Tween.EaseType.In);
        const float minSize = 0.5f;
        modulateTween.Parallel().TweenScale(this, new Vector2(minSize, minSize), duration)
            .SetEase(Tween.EaseType.In);
        
        modulateTween.Finished += QueueFree;
    }
    
}
