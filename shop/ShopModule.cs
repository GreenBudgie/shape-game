public partial class ShopModule : Control
{

    private static readonly PackedScene Scene = GD.Load<PackedScene>("uid://cr37qmsjo0vr3");
    
    private Control _buttonGroup = null!;
    private Button _button = null!;
    private Control _buttonTextures = null!;
    private TextureRect _buttonUnhovered = null!;
    private TextureRect _buttonHovered = null!;
    private RichTextLabel _title = null!;
    private TextureRect _moduleTexture = null!;
    private RichTextLabel _priceLabel = null!;
    
    private Mask _buttonMask = null!;

    private Glow _glow = null!;
    
    [Export] private AudioStream _hoverSound = null!;
    [Export] private AudioStream _clickSound = null!;
    [Export] private AudioStream _buttonUpSound = null!;

    public Module Module { get; private set; } = null!;

    public static ShopModule Create(Module module)
    {
        var node = Scene.Instantiate<ShopModule>();
        node.Module = module;
        return node;
    }
    
    public override void _Ready()
    {
        _buttonGroup = GetNode<Control>("ButtonGroup");
        _button = _buttonGroup.GetNode<Button>("Button");
        _buttonTextures = _buttonGroup.GetNode<Control>("ButtonTextures");
        _buttonUnhovered = _buttonTextures.GetNode<TextureRect>("ButtonUnhovered");
        _buttonHovered = _buttonTextures.GetNode<TextureRect>("ButtonHovered");
        
        _glow = Glow.AddGlow(_buttonUnhovered)
            .SetColor(ColorScheme.LightBlue)
            .SetStrength(1f)
            .SetRadius(0);
        
        _title = GetNode<RichTextLabel>("%Title");
        _title.Text = Module.Name;
        
        _moduleTexture = GetNode<TextureRect>("%ModuleTexture");
        _moduleTexture.Texture = Module.Texture;
        
        _priceLabel = GetNode<RichTextLabel>("%PriceLabel");
        _priceLabel.Text = Module.Price.ToString();
        
        _buttonMask = Mask.Attach(_buttonHovered).Axis(MaskAxis.Horizontal).Origin(MaskOrigin.Left);
        _buttonMask.Progress = 0;
    }

    private bool _isHovered;

    public override void _Process(double delta)
    {
        var player = Player.FindPlayer();
        if (player == null)
        {
            return;
        }

        var playerCenter = player.ToGlobal(player.CenterOfMass);
        var buttonRect = _button.GetGlobalRect();

        if (buttonRect.HasPoint(playerCenter))
        {
            if (!_isHovered)
            {
                OnHover();
                _isHovered = true;
            }
        }
        else
        {
            if (_isHovered)
            {
                OnUnhover();
                _isHovered = false;
            }
        }

        if (_isHovered && Input.IsActionJustPressed("inventory_left_click"))
        {
            OnClick();
        }
    }

    private Tween? _buttonMaskTween;
    private Tween? _glowTween;
    private Tween? _transformTween;
    private const float TweenDuration = 0.15f;
    private const float HoverSize = 1.05f;

    private void OnHover()
    {
        var sound = SoundManager.Instance.PlaySound(_hoverSound);
        sound.RandomizePitchOffset(0.2f);
        
        _buttonMaskTween?.Kill();
        _buttonMaskTween = CreateTween()
            .SetEase(Tween.EaseType.Out)
            .SetTrans(Tween.TransitionType.Quad);
        _buttonMaskTween.TweenProperty(_buttonMask.Material, Mask.ProgressShaderParam, 1f, TweenDuration);
        
        _glowTween?.Kill();
        _glowTween = CreateTween()
            .SetEase(Tween.EaseType.Out)
            .SetTrans(Tween.TransitionType.Quad);
        
        _glowTween.TweenProperty(
            @object: _glow,
            property: IGlow.RadiusProperty,
            finalVal: 30f,
            duration: TweenDuration
        );
        
        _transformTween?.Kill();
        _transformTween = CreateTween().SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Quad);

        _transformTween.TweenProperty(
            @object: _buttonGroup,
            property: ScaleProperty,
            finalVal: new Vector2(HoverSize, HoverSize),
            duration: TweenDuration
        );
    }

    private void OnUnhover()
    {
        var sound = SoundManager.Instance.PlaySound(_buttonUpSound);
        sound.RandomizePitchOffset(0.2f);
        
        _buttonMaskTween?.Kill();
        _buttonMaskTween = CreateTween()
            .SetEase(Tween.EaseType.Out)
            .SetTrans(Tween.TransitionType.Quad);
        _buttonMaskTween.TweenProperty(_buttonMask.Material, Mask.ProgressShaderParam, 0f, TweenDuration);
        
        _glowTween?.Kill();
        _glowTween = CreateTween()
            .SetEase(Tween.EaseType.Out)
            .SetTrans(Tween.TransitionType.Quad);
        
        _glowTween.TweenProperty(
            @object: _glow,
            property: IGlow.RadiusProperty,
            finalVal: 0f,
            duration: TweenDuration
        );
        
        _transformTween?.Kill();
        _transformTween = CreateTween().SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Quad);

        _transformTween.TweenProperty(
            @object: _buttonGroup,
            property: ScaleProperty,
            finalVal: Vector2.One,
            duration: TweenDuration
        );
    }

    private void OnClick()
    {
        var sound = SoundManager.Instance.PlaySound(_clickSound);
        sound.RandomizePitchOffset(0.2f);
    }
}
