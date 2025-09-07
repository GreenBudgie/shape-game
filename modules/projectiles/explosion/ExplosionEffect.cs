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

        // Convert explosion radius (pixels) → normalized
        var targetRadius = _explosion.GetRadius() / Min(viewportSize.X, viewportSize.Y);

        var tween = CreateTween();

        tween.TweenProperty(shaderMaterial, "shader_parameter/radius", targetRadius * 1.2, 0.4f)
            .SetTrans(Tween.TransitionType.Quint)
            .SetEase(Tween.EaseType.Out);

        tween.Parallel().TweenProperty(shaderMaterial, "shader_parameter/strength", 0.0f, 0.4f)
            .SetTrans(Tween.TransitionType.Cubic)
            .SetEase(Tween.EaseType.InOut);
        tween.Parallel().TweenProperty(shaderMaterial, "shader_parameter/width", 0.0f, 0.4f)
            .SetTrans(Tween.TransitionType.Cubic)
            .SetEase(Tween.EaseType.InOut);
        tween.Parallel().TweenProperty(shaderMaterial, "shader_parameter/feather", 0.0f, 0.4f)
            .SetTrans(Tween.TransitionType.Cubic)
            .SetEase(Tween.EaseType.InOut);
        
        tween.Finished += QueueFree;
    }

}
