public partial class ExplosionEffect : ColorRect
{
    private const float MinWidth = 0.04f;
    private const float MaxWidth = 0.15f;
    private const float RadiusIncreaseFactor = 1.4f;
    private const float DurationByRadiusFactor = 0.02f;

    private const string CenterParam = "center";
    private const string RadiusParam = "radius";
    private const string IntensityParam = "intensity";
    private const string WidthParam = "width";

    private static readonly PackedScene Scene = GD.Load<PackedScene>("uid://bafb067vadvj4");

    private Explosion _explosion = null!;

    public static ExplosionEffect Create(Explosion explosion)
    {
        var effect = Scene.Instantiate<ExplosionEffect>();
        effect._explosion = explosion;
        return effect;
    }

    public override void _Ready()
    {
        // Set position
        var viewportSize = GetViewportRect().Size;
        var screenPos = _explosion.GlobalPosition;
        var uv = screenPos / viewportSize;
        var shaderMaterial = (ShaderMaterial)Material;
        shaderMaterial.SetShaderParameter(CenterParam, uv);

        // Set initial parameters
        shaderMaterial.SetShaderParameter(RadiusParam, 0.0f);
        shaderMaterial.SetShaderParameter(IntensityParam, 1.0f);

        var radiusFactor = _explosion.GetEffectRadiusRatio();
        var width = Lerp(MinWidth, MaxWidth, radiusFactor);
        shaderMaterial.SetShaderParameter(WidthParam, width);

        // Tween
        var duration = Sqrt(_explosion.GetRadius()) * DurationByRadiusFactor;
        var tween = CreateTween().SetParallel();

        var targetRadius = _explosion.GetRadius() / viewportSize.Y * RadiusIncreaseFactor;
        tween.TweenProperty(shaderMaterial, ShaderParameter(RadiusParam), targetRadius, duration)
            .SetTrans(Tween.TransitionType.Quart)
            .SetEase(Tween.EaseType.Out);
        tween.TweenProperty(shaderMaterial, ShaderParameter(IntensityParam), 0.0f, duration)
            .SetTrans(Tween.TransitionType.Quad)
            .SetEase(Tween.EaseType.Out);
        tween.TweenProperty(shaderMaterial, ShaderParameter(WidthParam), 0.0f, duration)
            .SetTrans(Tween.TransitionType.Quad)
            .SetEase(Tween.EaseType.Out);

        tween.Finished += QueueFree;
    }
}