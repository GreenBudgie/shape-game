public partial class CrystalCounter : Control
{

    [Export] private Color _crystalColor;

    private const float CrystalCollectGlowMinStrength = 0.5f;
    private const float CrystalCollectGlowStrengthDelta = 0.5f;
    private const float CrystalCollectMaxGlowStrength = 2f;
    private const float CrystalCollectGlowRadiusDelta = 15f;
    private const float CrystalCollectMaxGlowRadius = 60f;
    private const float CrystalCollectTextureTiltDegDelta = 20f;
    private const float CrystalCollectMaxTextureTiltDeg = 80f;
    private const float CrystalCollectTextureSizeDelta = 0.2f;
    private const float CrystalCollectMaxTextureSize = 1.6f;

    private const float CrystalCollectTextureInTime = 0.08f;
    private const float CrystalCollectTextureOutTime = 0.2f;
    private const float CrystalCollectGlowInTime = 0.1f;
    private const float CrystalCollectGlowOutTime = 0.4f;

    private Label _label = null!;
    private TextureRect _crystalTexture = null!;
    private Glow _glow = null!;

    private Tween? _textureTween;
    private Tween? _glowTween;

    public override void _Ready()
    {
        _label = GetNode<Label>("CrystalCounterLabel");
        _crystalTexture = GetNode<TextureRect>("CrystalTexture");

        _glow = Glow.AddGlow(_crystalTexture)
            .SetColor(_crystalColor)
            .SetStrength(CrystalCollectGlowMinStrength)
            .SetRadius(0);

        CrystalManager.Instance.CrystalCollected += OnCrystalCollected;
    }

    private void OnCrystalCollected()
    {
        _label.Text = CrystalManager.Instance.Crystals.ToString();

        AnimateTexture();
        AnimateGlow();
    }

    private void AnimateGlow()
    {
        _glowTween?.Kill();
        _glowTween = _glow.CreateTween();
        var setStrengthAction = _glow.SetStrength;
        var setRadiusAction = _glow.SetRadius;

        var strength = _glow.GetStrength();
        var radius = _glow.GetRadius();
        
        var cappedStrength = Min(
            strength + CrystalCollectGlowStrengthDelta,
            CrystalCollectMaxGlowStrength
        );
        var cappedRadius = Min(
            strength + CrystalCollectGlowRadiusDelta,
            CrystalCollectMaxGlowRadius
        );
        
        _glowTween.TweenMethod(
            method: Callable.From(setStrengthAction),
            from: strength,
            to: cappedStrength,
            duration: CrystalCollectGlowInTime
        ).SetEase(Tween.EaseType.Out);
        _glowTween.Parallel().TweenMethod(
            method: Callable.From(setRadiusAction),
            from: radius,
            to: cappedRadius,
            duration: CrystalCollectGlowInTime
        ).SetEase(Tween.EaseType.Out);
        
        _glowTween.TweenMethod(
            method: Callable.From(setStrengthAction),
            from: cappedStrength,
            to: CrystalCollectGlowMinStrength,
            duration: CrystalCollectGlowOutTime
        ).SetEase(Tween.EaseType.In);
        _glowTween.Parallel().TweenMethod(
            method: Callable.From(setRadiusAction),
            from: cappedRadius,
            to: 0,
            duration: CrystalCollectGlowOutTime
        ).SetEase(Tween.EaseType.In);
    }

    private void AnimateTexture()
    {
        _textureTween?.Kill();
        _textureTween = _crystalTexture.CreateTween();

        var randomDirTilt = GD.Randf() > 0.5 ? -CrystalCollectTextureTiltDegDelta : CrystalCollectTextureTiltDegDelta;
        var cappedTilt = Min(
            _crystalTexture.RotationDegrees + randomDirTilt,
            CrystalCollectMaxTextureTiltDeg
        );
        var cappedSize = Min(
            _crystalTexture.Scale.X + CrystalCollectTextureSizeDelta,
            CrystalCollectMaxTextureSize
        );

        _textureTween.TweenProperty(
            @object: _crystalTexture,
            property: Control.PropertyName.RotationDegrees.ToString(),
            finalVal: cappedTilt,
            duration: CrystalCollectTextureInTime
        ).SetEase(Tween.EaseType.Out);
        _textureTween.Parallel().TweenProperty(
            @object: _crystalTexture,
            property: Control.PropertyName.Scale.ToString(),
            finalVal: new Vector2(cappedSize, cappedSize),
            duration: CrystalCollectTextureInTime
        ).SetEase(Tween.EaseType.Out);

        _textureTween.TweenProperty(
            @object: _crystalTexture,
            property: Control.PropertyName.RotationDegrees.ToString(),
            finalVal: 0,
            duration: CrystalCollectTextureOutTime
        ).SetEase(Tween.EaseType.In);
        _textureTween.Parallel().TweenProperty(
            @object: _crystalTexture,
            property: Control.PropertyName.Scale.ToString(),
            finalVal: Vector2.One,
            duration: CrystalCollectTextureOutTime
        ).SetEase(Tween.EaseType.In);
    }

}