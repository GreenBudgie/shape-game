public partial class InventorySlot : TextureButton
{

    private const float GlowStartDistance = 600f;
    private const float GlowStopDistance = 100f;
    private const float GlowStartDistanceSq = GlowStartDistance * GlowStartDistance;
    private const float GlowMinStrength = 0.6f;
    private const float GlowMaxStrength = 1f;
    private const float GlowMaxRadius = 50f;

    private const float GlowHoverStrength = 1.5f;
    private const float GlowHoverRadius = 80f;
    private const float GlowHoverTweenDuration = 0.1f;
    private const float GlowUnhoverTweenDuration = 0.4f;
    private const float ButtonDownTweenDuration = 0.1f;
    private const float ButtonUpTweenDuration = 0.1f;
    private const float ButtonDownGlowStrength = 1.5f;
    private const float ButtonDownSize = 0.9f;
    private const float HoverSize = 1.15f;

    private static readonly AudioStream HoverSound = GD.Load<AudioStream>("uid://djdfrb1kcmfle");
    private static readonly AudioStream ClickSound = GD.Load<AudioStream>("uid://cry6gufmuccda");
    private static readonly AudioStream ButtonUpSound = GD.Load<AudioStream>("uid://cpqn2k806hyjd");
    
    public int Number { get; private set; }

    private Glow _glow = null!;
    private Tween? _glowTween;
    private Tween? _transformTween;

    private UiModule? _module;
    private ModuleInfo? _moduleInfo;

    private static readonly PackedScene Scene = GD.Load<PackedScene>("uid://dilsv34jaqcrd");

    public static InventorySlot Create(int number)
    {
        var node = Scene.Instantiate<InventorySlot>();
        node.Number = number;
        return node;
    }

    public override void _Ready()
    {
        _glow = Glow.AddGlow(this)
            .SetColor(ColorScheme.LightBlue)
            .TurnOff();

        MouseEntered += OnMouseEnter;
        MouseExited += OnMouseExit;
        ButtonDown += OnButtonDown;
        ButtonUp += OnButtonUp;

        InventoryManager.Instance.DragAndDropStarted += OnDragAndDropStarted;
        InventoryManager.Instance.DragAndDropEnded += OnDragAndDropEnded;
    }
    
    public override void _Process(double delta)
    {
        if (!InventoryManager.Instance.IsOpen || IsDisabled())
        {
            _glow.TurnOff();
            return;
        }
        
        if (IsHovered() || IsPressed())
        {
            return;
        }

        var isTweenRunning = _glowTween?.IsValid() == true && _glowTween?.IsRunning() == true;

        var centeredPosition = GlobalPosition + PivotOffset;
        var distanceToMouseSq = MouseInputManager.Instance.GetCachedGlobalMousePosition()
            .DistanceSquaredTo(centeredPosition);
        if (distanceToMouseSq > GlowStartDistanceSq)
        {
            if (!isTweenRunning)
            {
                _glow.TurnOff();
            }

            return;
        }

        var distanceToMouse = Sqrt(distanceToMouseSq);
        var distanceRatio = 1 - (distanceToMouse - GlowStopDistance) / (GlowStartDistance - GlowStopDistance);
        var glowStrength = Min(Lerp(distanceRatio, GlowMinStrength, GlowMaxStrength), GlowMaxStrength);
        var glowRadius = Min(distanceRatio * GlowMaxRadius, GlowMaxRadius);

        if (!isTweenRunning)
        {
            _glow.Strength = glowStrength;
            _glow.Radius = glowRadius;
            return;
        }

        if (glowStrength >= _glow.Strength)
        {
            _glow.Strength = glowStrength;
            _glowTween?.Kill();
        }

        if (glowRadius >= _glow.Radius)
        {
            _glow.Radius = glowRadius;
            _glowTween?.Kill();
        }
    }

    public void SwapModules(InventorySlot otherSlot)
    {
        (otherSlot._module, _module) = (_module, otherSlot._module);
        if (_module != null)
        {
            _module.Slot = this;
        }
        
        if (otherSlot._module != null)
        {
            otherSlot._module.Slot = otherSlot;
        }
    }
    
    public void InsertModule(UiModule module)
    {
        RemoveModule();
        InventoryManager.Instance.AddChild(module);
        _module = module;
        module.Slot = this;
        _module.MoveToSlotInstantly();
    }

    public UiModule? GetModule()
    {
        return _module;
    }

    public UiModule? RemoveModule()
    {
        var module = GetModule();
        if (module == null)
        {
            return null;
        }

        _module = null;
        module.QueueFree();
        return module;
    }

    public bool HasModule()
    {
        return GetModule() != null;
    }

    private void OnButtonDown()
    {
        HideModuleInfo();
        
        _transformTween?.Kill();
        _transformTween = CreateTween().SetEase(Tween.EaseType.In).SetTrans(Tween.TransitionType.Quad);

        _transformTween.TweenProperty(
            @object: this,
            property: ScaleProperty,
            finalVal: new Vector2(ButtonDownSize, ButtonDownSize),
            duration: ButtonDownTweenDuration
        );
        
        _glowTween?.Kill();
        _glowTween = CreateTween().SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Quad);

        _glowTween.TweenProperty(
            @object: _glow,
            property: IGlow.StrengthProperty,
            finalVal: ButtonDownGlowStrength,
            duration: ButtonDownTweenDuration
        );
        
        var sound = SoundManager.Instance.PlaySound(ClickSound);
        sound.RandomizePitchOffset(0.2f);
    }
    
    private void OnButtonUp()
    {
        _transformTween?.Kill();
        _transformTween = CreateTween().SetEase(Tween.EaseType.In).SetTrans(Tween.TransitionType.Quad);

        _transformTween.TweenProperty(
            @object: this,
            property: ScaleProperty,
            finalVal: new Vector2(HoverSize, HoverSize),
            duration: ButtonUpTweenDuration
        );
        
        _glowTween?.Kill();
        _glowTween = CreateTween().SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Quad);

        _glowTween.TweenProperty(
            @object: _glow,
            property: IGlow.StrengthProperty,
            finalVal: GlowHoverStrength,
            duration: ButtonUpTweenDuration
        );
        
        var sound = SoundManager.Instance.PlaySound(ButtonUpSound);
        sound.RandomizePitchOffset(0.2f);

        if (IsHovered())
        {
            ShowModuleInfo();
        }
        else
        {
            OnMouseExit();
        }
    }

    private void OnMouseEnter()
    {
        if (IsPressed() || IsDisabled())
        {
            return;
        }
        
        ShowModuleInfo();
        _module?.SlotHovered();

        var sound = SoundManager.Instance.PlaySound(HoverSound);
        sound.RandomizePitchOffset(0.2f);

        _glowTween?.Kill();
        _glowTween = CreateTween().SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Quad);

        _glowTween.TweenProperty(
            @object: _glow,
            property: IGlow.StrengthProperty,
            finalVal: GlowHoverStrength,
            duration: GlowHoverTweenDuration
        );
        _glowTween.Parallel().TweenProperty(
            @object: _glow,
            property: IGlow.RadiusProperty,
            finalVal: GlowHoverRadius,
            duration: GlowHoverTweenDuration
        );

        _transformTween?.Kill();
        _transformTween = CreateTween().SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Quad);

        _transformTween.TweenProperty(
            @object: this,
            property: ScaleProperty,
            finalVal: new Vector2(HoverSize, HoverSize),
            duration: GlowHoverTweenDuration
        );
    }

    private void OnMouseExit()
    {
        HideModuleInfo();
        
        if (IsPressed() || IsDisabled())
        {
            return;
        }
        
        _module?.SlotUnhovered();

        _glowTween?.Kill();
        _glowTween = CreateTween().SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Quad);

        _glowTween.TweenProperty(
            @object: _glow,
            property: IGlow.StrengthProperty,
            finalVal: 0,
            duration: GlowUnhoverTweenDuration
        );
        _glowTween.Parallel().TweenProperty(
            @object: _glow,
            property: IGlow.RadiusProperty,
            finalVal: 0,
            duration: GlowUnhoverTweenDuration
        );

        _transformTween?.Kill();
        _transformTween = CreateTween().SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Quad);

        _transformTween.TweenProperty(
            @object: this,
            property: ScaleProperty,
            finalVal: Vector2.One,
            duration: GlowHoverTweenDuration
        );
    }

    private void ShowModuleInfo()
    {
        if (_module == null || InventoryManager.Instance.IsDragAndDropActive())
        {
            return;
        }
        
        _moduleInfo = ModuleInfo.Create(_module.Module);
        InventoryManager.Instance.AddChild(_moduleInfo);
    }

    private void HideModuleInfo()
    {
        _moduleInfo?.Remove();
        _moduleInfo = null;
    }

    private void OnDragAndDropStarted()
    {
        HideModuleInfo();
    }
    
    private void OnDragAndDropEnded()
    {
        if (IsHovered())
        {
            ShowModuleInfo();
        }
    }

}