using System;

public partial class ExplosionRadiusPreview : Node2D
{

    private const float Thickness = 8.0f;
    private const float BaseRadius = 300.0f;
    private const float BaseDashDeg = 10.0f;
    private const float BaseGapDeg = 20.0f;

    private const float StartScale = 0.7f;
    private const float EndScale = 1.2f;
    private const float EndDuration = 0.5f;
    private const float StartRotationDuration = 0.8f;
    private const float StartAppearDuration = 0.3f;
    private const float StartRotationDegrees = 120;
    private const float EndRotationDegrees = 60;

    private static readonly PackedScene Scene = GD.Load<PackedScene>("uid://cw5lackk3gus7");
    private static readonly Color Color = ColorScheme.Red;

    private Explosion _explosion = null!;

    public static ExplosionRadiusPreview Create(Explosion explosion)
    {
        var node = Scene.Instantiate<ExplosionRadiusPreview>();
        node._explosion = explosion;
        node.GlobalPosition = explosion.GlobalPosition;
        ShapeGame.Instance.AddChild(node);
        return node;
    }

    private Tween? _startTween;

    public override void _Ready()
    {
        _explosion.Detonated += OnDetonate;
        
        var fuseTime = _explosion.GetFuseTimeSeconds();
        if (fuseTime <= 0)
        {
            return;
        }

        Scale = new Vector2(StartScale, StartScale);
        Modulate = Colors.Transparent;
        
        _startTween = CreateTween()
            .SetParallel()
            .SetEase(Tween.EaseType.InOut)
            .SetTrans(Tween.TransitionType.Cubic);
        
        _startTween.TweenProperty(
            this,
            Node2D.PropertyName.RotationDegrees.ToString(),
            StartRotationDegrees,
            fuseTime * StartRotationDuration
        );
        _startTween.TweenProperty(
            this,
            Node2D.PropertyName.Scale.ToString(),
            Vector2.One,
            fuseTime * StartAppearDuration
        );
        _startTween.TweenProperty(
            this,
            CanvasItem.PropertyName.Modulate.ToString(),
            Colors.White,
            fuseTime * StartAppearDuration
        );
    }

    public override void _Process(double delta)
    {
        if (IsInstanceValid(_explosion))
        {
            GlobalPosition = _explosion.GlobalPosition;
        }
    }

    public override void _Draw()
    {
        var radius = _explosion.GetRadius();

        // Calculate fixed linear dash and base gap
        var baseDashLinear = BaseRadius * DegToRad(BaseDashDeg);
        var baseGapLinear = BaseRadius * DegToRad(BaseGapDeg);

        // Circumference in linear units
        var circum = Tau * radius;

        // Ideal number of segments
        var idealNum = circum / (baseDashLinear + baseGapLinear);
        var num = (int)Math.Round(idealNum);
        if (num < 1) num = 1;

        // Total linear for dashes
        var totalDashLinear = num * baseDashLinear;
        var totalGapLinear = circum - totalDashLinear;
        if (totalGapLinear < 0)
        {
            num--;
            totalDashLinear = num * baseDashLinear;
            totalGapLinear = circum - totalDashLinear;
        }

        // Adjusted uniform gap linear
        var gapLinear = totalGapLinear / num;

        // Convert to radians (angles)
        var dashRad = baseDashLinear / radius;
        var gapRad = gapLinear / radius;

        // Draw the dashes
        var currentAngle = 0.0f;
        for (var i = 0; i < num; i++)
        {
            var startAngle = currentAngle;
            var endAngle = currentAngle + dashRad;

            // Draw the arc (body of the dash)
            DrawArc(Vector2.Zero, radius, startAngle, endAngle, 4, Color, Thickness, true);

            // Calculate positions for the caps
            var (startAngleSin, startAngleCos) = SinCos(startAngle);
            var (endAngleSin, endAngleCos) = SinCos(endAngle);
            var startPos = new Vector2(startAngleCos, startAngleSin) * radius;
            var endPos = new Vector2(endAngleCos, endAngleSin) * radius;

            // Draw round caps (filled circles) at each end
            DrawCircle(startPos, Thickness / 2, Color);
            DrawCircle(endPos, Thickness / 2, Color);

            currentAngle += dashRad + gapRad;
        }
    }

    private void OnDetonate()
    {
        _startTween?.Kill();

        var endTween = CreateTween()
            .SetParallel()
            .SetEase(Tween.EaseType.Out)
            .SetTrans(Tween.TransitionType.Cubic);
        
        endTween.TweenProperty(
            this,
            Node2D.PropertyName.RotationDegrees.ToString(),
            EndRotationDegrees,
            EndDuration
        );
        var endScale = new Vector2(EndScale, EndScale);
        endTween.TweenProperty(
            this,
            Node2D.PropertyName.Scale.ToString(),
            endScale,
            EndDuration
        );
        endTween.TweenProperty(
            this,
            CanvasItem.PropertyName.Modulate.ToString(),
            Colors.Transparent,
            EndDuration
        );

        endTween.Finished += QueueFree;
    }
}