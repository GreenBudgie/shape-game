public partial class ExplosionEffect : ColorRect
{

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
        shaderMaterial.SetShaderParameter("center", uv);

        var targetRadius = _explosion.GetRadius() / viewportSize.Y;

        shaderMaterial.SetShaderParameter("radius", 0.0f);
        shaderMaterial.SetShaderParameter("intensity", 1.0f);
        var duration = Sqrt(_explosion.GetRadius()) * 0.015f;
        var tween = CreateTween();

        tween.TweenProperty(shaderMaterial, "shader_parameter/radius", targetRadius * 1.2, duration)
            .SetTrans(Tween.TransitionType.Cubic)
            .SetEase(Tween.EaseType.Out);
        
        tween.Parallel().TweenProperty(shaderMaterial, "shader_parameter/intensity", 0.0f, duration)
            .SetTrans(Tween.TransitionType.Cubic)
            .SetEase(Tween.EaseType.OutIn);

        tween.Parallel().TweenProperty(shaderMaterial, "shader_parameter/width", 0.0f, duration)
            .SetTrans(Tween.TransitionType.Cubic)
            .SetEase(Tween.EaseType.OutIn);

        tween.Finished += QueueFree;
    }

}