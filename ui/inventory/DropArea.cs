public partial class DropArea : TextureButton
{

    public static DropArea Instance { get; private set; } = null!;

    [Signal]
    public delegate void HoveredEventHandler();
    
    [Signal]
    public delegate void UnhoveredEventHandler();

    private Glow _glow = null!;
    private Tween? _animationTween;
    
    public DropArea()
    {
        Instance = this;
    }
    
    public override void _Ready()
    {
        _glow = Glow.AddGlow(this)
            .SetColor(ColorScheme.Red)
            .SetStrength(1)
            .SetRadius(0);

        Modulate = Colors.Transparent;
        OffsetTransformScale = new Vector2(0.75f, 0.75f);
        OffsetTransformRotation = RandomUtils.DeltaRange(0, 0.1f);
        DoHide();

        MouseEntered += OnMouseEnter;
        MouseExited += OnMouseExit;
        Pressed += OnPress;
        
        InventoryManager.Instance.Connect(
            InventoryManager.SignalName.ModuleGrabbed,
            Callable.From((InventoryModule _) => DoShow())
        );
        InventoryManager.Instance.Connect(
            InventoryManager.SignalName.ModuleInserted,
            Callable.From((InventoryModule _) => DoHide())
        );
        InventoryManager.Instance.Connect(
            InventoryManager.SignalName.ModuleDropping,
            Callable.From((InventoryModule _) => DoHide())
        );
    }

    private const float AnimationDuration = 0.2f;

    private void OnPress()
    {
        InventoryManager.Instance.DraggingModule?.Drop();
    }

    private void OnMouseEnter()
    {
        _animationTween?.Kill();

        _animationTween = CreateTween().SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.Out).SetParallel();
        _animationTween.TweenGlowRadius(_glow, 30, AnimationDuration);
        _animationTween.TweenOffsetScale(this, 1.1f, AnimationDuration);
        _animationTween.TweenOffsetRotation(this, RandomUtils.DeltaRange(0, 0.1f), AnimationDuration);
        
        EmitSignalHovered();
    }
    
    private void OnMouseExit()
    {
        _animationTween?.Kill();

        _animationTween = CreateTween().SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.In).SetParallel();
        _animationTween.TweenGlowRadius(_glow, 0, AnimationDuration);
        _animationTween.TweenOffsetScaleReset(this, AnimationDuration);
        _animationTween.TweenOffsetRotationReset(this, AnimationDuration);
        
        EmitSignalUnhovered();
    }

    private void DoShow()
    {
        MouseFilter = MouseFilterEnum.Stop;
        
        _animationTween?.Kill();

        _animationTween = CreateTween().SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.Out).SetParallel();
        _animationTween.FadeIn(this, AnimationDuration);
        _animationTween.TweenGlowRadius(_glow, 0, AnimationDuration);
        _animationTween.TweenOffsetScaleReset(this, AnimationDuration);
        _animationTween.TweenOffsetRotationReset(this, AnimationDuration);
    }
    
    private void DoHide()
    {
        MouseFilter = MouseFilterEnum.Ignore;
        
        _animationTween?.Kill();

        _animationTween = CreateTween().SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.In).SetParallel();
        _animationTween.FadeOut(this, AnimationDuration);
        _animationTween.TweenGlowRadius(_glow, 0, AnimationDuration);
        _animationTween.TweenOffsetScale(this, 0.75f, AnimationDuration);
        _animationTween.TweenOffsetRotation(this, RandomUtils.DeltaRange(0, 0.1f), AnimationDuration);
    }
}
