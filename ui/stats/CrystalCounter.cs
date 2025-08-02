public partial class CrystalCounter : Control
{

    [Export] private Color _crystalColor;

    private const float CrystalCollectGlowMinStrength = 0.5f;

    private Label _label = null!;
    private TextureRect _crystalTexture = null!;
    private Glow _glow = null!;

    private readonly ShakeTween _textureShakeTween = new ShakeTween()
        .TiltDelta(20f)
        .MaxTilt(80f)
        .SizeDelta(0.2f)
        .MaxSize(1.6f)
        .InTime(0.08f)
        .OutTime(0.2f);
    
    private readonly GlowTween _glowTween = new GlowTween()
        .MinStrength(CrystalCollectGlowMinStrength)
        .StrengthDelta(0.5f)
        .MaxStrength(2f)
        .RadiusDelta(15f)
        .MaxRadius(60f)
        .InTime(0.1f)
        .OutTime(0.4f);

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
        _glowTween.Play(_glow);
    }

    private void AnimateTexture()
    {
        _textureShakeTween.Play(_crystalTexture);
    }

}