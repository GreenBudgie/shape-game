public partial class ModuleEffect : Node2D
{

    private const float DisplayScale = 0.75f;

    public ModuleEffectPlace Place { get; private set; }
    public Module Module { get; private set; } = null!;
    
    private string _text = null!;
    private Color _textColor;

    private ModuleDisplay _display = null!; 
    private Vector2 _startPosition;
    private Vector2 _endPosition; 
    
    /// <summary>
    /// Do not call directly! Use ModuleEffectManager.Instance.ShowEffect instead
    /// </summary>
    public static ModuleEffect Create(ModuleEffectPlace place, Module module, string text, Color textColor)
    {
        var node = new ModuleEffect();
        node.Place = place;
        node.Module = module;
        node._text = text;
        node._textColor = textColor;
        
        node.CalculateRandomDisplayPositionsStartEnd();
        node.GlobalPosition = node._startPosition;
        
        return node;
    }

    public override void _Ready()
    {
        _display = ModuleDisplay.Create(Module.Type);
        AddChild(_display);

        Modulate = Colors.Transparent;
        Scale = new Vector2(DisplayScale, DisplayScale);
        Rotation = RandomUtils.DeltaRange(0, Pi / 6);
        PlayAnimationAndShowText();
    }

    /// <summary>
    /// Stops the disappearing animation and shows the text again at the same effect
    /// </summary>
    public void ProlongWithText(string text, Color textColor)
    {
        PlayAnimationAndShowText();
    }

    private Tween? _appearPositionTween;
    private Tween? _appearHoldTween;
    private Tween? _appearModulateTween;
    private Tween? _disappearPositionTween;
    private Tween? _disappearModulateTween;

    private void PlayAnimationAndShowText()
    {
        const float startDuration = 0.25f;
        const float holdDuration = 1f;

        _disappearPositionTween?.Kill();
        if (_appearPositionTween == null)
        {
            _appearPositionTween = CreateTween().SetTrans(Tween.TransitionType.Back).SetEase(Tween.EaseType.Out);
            _appearPositionTween.TweenPosition(this, _endPosition, startDuration);
        }
        
        _disappearModulateTween?.Kill();        
        if (_appearModulateTween == null)
        {
            _appearModulateTween = CreateTween().SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.Out);
            _appearModulateTween.FadeIn(this, startDuration / 2);
        }

        _appearHoldTween?.Kill();
        _appearHoldTween = CreateTween().SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.InOut);
        _appearHoldTween.TweenScale(_display, 0.9f, startDuration / 3);
        _appearHoldTween.TweenScale(_display, 1.1f, startDuration / 3);
        _appearHoldTween.Parallel().TweenCallback(Callable.From(ShowText));
        _appearHoldTween.TweenScaleReset(_display, startDuration / 3);
        _appearHoldTween.TweenRotation(_display, RandomUtils.DeltaRange(0, Pi / 6), holdDuration);

        _appearHoldTween.Finished += PlayDisappearAnimation;
    }

    private void PlayDisappearAnimation()
    {
        _appearModulateTween?.Kill();
        _appearPositionTween = null;
        
        _appearModulateTween?.Kill();
        _appearModulateTween = null;
        
        const float endDuration = 0.25f;
        
        _disappearPositionTween?.Kill();
        _disappearPositionTween = CreateTween().SetTrans(Tween.TransitionType.Back).SetEase(Tween.EaseType.In);
        _disappearPositionTween.TweenPosition(this, _startPosition, endDuration);
        
        _disappearModulateTween?.Kill();
        _disappearModulateTween = CreateTween().SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.In);
        _disappearModulateTween.FadeIn(this, endDuration);

        _disappearModulateTween.Finished += QueueFree;
    }

    private void ShowText()
    {
        const float offset = 50f;
        
        var scaledShapeSize = Module.Type.Shape.PixelSize.Y * DisplayScale / 2f;
        var labelPosition = GlobalPosition - new Vector2(0, scaledShapeSize + offset);
        
        var label = PopupLabel.Create(labelPosition, _text);
        label.SetColor(_textColor);
    }

    private void CalculateRandomDisplayPositionsStartEnd()
    {
        var shapeSize = Module.Type.Shape.PixelSize;
        var startY = ShapeGame.PlayableArea.End.Y + shapeSize.Y * DisplayScale / 2f;
        var endY = ShapeGame.PlayableArea.End.Y - shapeSize.Y * DisplayScale / 2f - 50f;

        _startPosition = new Vector2(Place.X, startY);
        _endPosition = new Vector2(Place.X, endY);
    }
    
}