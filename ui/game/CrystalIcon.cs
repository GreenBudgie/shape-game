public partial class CrystalIcon : TextureRect
{

    private Glow _glow = null!;
    private Tween? _tween;
    
    public override void _Ready()
    {
        _glow = Glow.AddGlow(this)
            .SetColor(ColorScheme.Yellow)
            .SetStrength(1)
            .SetRadius(0);
        
        CrystalManager.Instance.CrystalAmountChanged += OnCrystalAmountChanged;
    }

    private void OnCrystalAmountChanged()
    {
        const float maxParallelPickups = 5f;
        
        const float startDuration = 0.1f;
        const float endDuration = 0.3f;
        
        const float scaleStep = 0.2f;
        const float maxScale = 1f + scaleStep * maxParallelPickups;
        const float rotationStep = 0.15f;
        const float maxRotation = rotationStep * maxParallelPickups;
        const float glowRadiusStep = 4f;
        const float maxGlowRadius = glowRadiusStep * maxParallelPickups;
        
        _tween?.Kill();
        _tween = CreateTween().SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Quad);
        
        _tween.TweenOffsetScale(this, Min(OffsetTransformScale.X + scaleStep, maxScale), startDuration);
        _tween.Parallel()
            .TweenOffsetRotation(this, Min(OffsetTransformRotation + rotationStep, maxRotation), startDuration);
        _tween.Parallel()
            .TweenGlowRadius(_glow, Min(_glow.Radius + maxGlowRadius, maxGlowRadius), startDuration);
        
        _tween.TweenOffsetScaleReset(this, endDuration);
        _tween.Parallel().TweenOffsetRotationReset(this, endDuration);
        _tween.Parallel().TweenGlowRadius(_glow, 0, endDuration);
    }
    
}
