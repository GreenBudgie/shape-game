public partial class ExplosionEffect : ColorRect
{

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
        var viewportSize = GetViewportRect().Size;
        var screenPos = _explosion.GlobalPosition;
        var uv = screenPos / viewportSize;
        var shaderMaterial = (ShaderMaterial)Material;
        shaderMaterial.SetShaderParameter(CenterParam, uv);

        var targetRadius = _explosion.GetRadius() / viewportSize.Y;

        shaderMaterial.SetShaderParameter(RadiusParam, 0.0f);
        shaderMaterial.SetShaderParameter(IntensityParam, 1.0f);
        var duration = Sqrt(_explosion.GetRadius()) * 0.015f;
        var tween = CreateTween();

        tween.TweenProperty(shaderMaterial, ShaderParameter(RadiusParam), targetRadius * 1.2, duration)
            .SetTrans(Tween.TransitionType.Quint)
            .SetEase(Tween.EaseType.Out);
        
        tween.Parallel().TweenProperty(shaderMaterial, ShaderParameter(IntensityParam), 0.0f, duration)
            .SetTrans(Tween.TransitionType.Quad)
            .SetEase(Tween.EaseType.In);
        
        tween.Parallel().TweenProperty(shaderMaterial, ShaderParameter(WidthParam), 0.0f, duration)
            .SetTrans(Tween.TransitionType.Quad)
            .SetEase(Tween.EaseType.In);

        tween.Finished += QueueFree;
    }

}