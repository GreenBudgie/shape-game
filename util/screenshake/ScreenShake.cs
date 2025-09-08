public partial class ScreenShake : ColorRect
{

    private const string StrengthParam = "strength";
    private const string MagnitudeParam = "magnitude";

    public static ScreenShake Instance { get; private set; } = null!;

    public ScreenShake()
    {
        Instance = this;
    }

    private Tween? _tween;

    public void Shake(ShakeStrength strength)
    {
        var shaderMaterial = (ShaderMaterial)Material;

        var duration = strength.Duration();
        const float initDuration = 0.05f;
        var actualStrength = strength.Strength();
        var magnitude = new Vector2(strength.Magnitude(), strength.Magnitude());
        
        _tween?.Kill();
        _tween = CreateTween().SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.Out);
        
        _tween.TweenProperty(
            shaderMaterial,
            ShaderParameter(StrengthParam),
            actualStrength,
            initDuration
        );
        _tween.Parallel().TweenProperty(
            shaderMaterial,
            ShaderParameter(MagnitudeParam),
            magnitude,
            initDuration
        );

        _tween.TweenProperty(
            shaderMaterial,
            ShaderParameter(StrengthParam),
            0,
            duration
        );
        _tween.Parallel().TweenProperty(
            shaderMaterial,
            ShaderParameter(MagnitudeParam),
            Vector2.Zero,
            duration
        );
    }

}