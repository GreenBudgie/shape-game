using System.Globalization;

public partial class HealthLabel : Label
{

    private static readonly PackedScene Scene = GD.Load<PackedScene>("uid://vtgnvnodulxr");

    private float _health;
    
    /**
     * Spawns a health display label. Negative health refers to damage, positive to healing (0 included).
     */
    public static HealthLabel Create(float health, Vector2 spawnPosition)
    {
        var node = Scene.Instantiate<HealthLabel>();
        node.GlobalPosition = spawnPosition;
        node._health = health;
        ShapeGame.Instance.AddChild(node);
        return node;
    }

    public override void _Ready()
    {
        var sign = _health > 0 ? "+" : "";
        var color = Sign(_health) switch
        {
            -1 => ColorScheme.Red,
            0 => ColorScheme.Yellow,
            _ => ColorScheme.LightGreen
        };
        AddThemeColorOverride("font_color", color);

        string healthToDisplay; 
        if (Abs(_health) < 1)
        {
            if (IsEqualApprox(_health, 0f))
            {
                healthToDisplay = "0";
            }
            else
            {
                healthToDisplay = _health.ToString("0.#", CultureInfo.InvariantCulture);
            }
        }
        else
        {
            healthToDisplay = RoundToInt(_health).ToString();
        }
        
        Text = sign + healthToDisplay;
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
