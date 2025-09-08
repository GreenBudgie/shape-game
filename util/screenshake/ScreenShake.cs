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
        shaderMaterial.SetShaderParameter(StrengthParam, strength.Strength);
        shaderMaterial.SetShaderParameter(MagnitudeParam, new Vector2(strength.Magnitude, strength.Magnitude));
        
        _tween?.Kill();
        _tween = CreateTween().SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.Out);
        
        _tween.TweenProperty(shaderMaterial, ShaderParameter(StrengthParam), 0, strength.Duration);
        _tween.TweenProperty(shaderMaterial, ShaderParameter(MagnitudeParam), Vector2.Zero, strength.Duration);
    }

}