using System.Collections.Generic;

public partial class InventorySlot : TextureButton
{

    public const float Inradius = 87;
    public const float InradiusSq = Inradius * Inradius;
    public const float Circumradius = 97;

    public ModuleInventory Inventory { get; private set; } = null!;
    public HexCoordinates Coordinates { get; private set; }
    public InventoryModule? Module { get; set; }
    public List<InventoryModuleConnection> Connections { get; private set; } = [];
    
    private Vector2 _centerOffsetPosition;
    private Glow _glow = null!;

    private static readonly PackedScene Scene = GD.Load<PackedScene>("uid://dilsv34jaqcrd");

    public static InventorySlot Create(ModuleInventory inventory, HexCoordinates coordinates)
    {
        var node = Scene.Instantiate<InventorySlot>();
        node.Inventory = inventory;
        node.Coordinates = coordinates;
        return node;
    }

    public override void _Ready()
    {
        _glow = Glow.AddGlow(this)
            .SetColor(ColorScheme.LightBlue)
            .SetStrength(1)
            .SetRadius(0);

        InventoryManager.Instance.Connect(InventoryManager.SignalName.InventoryClosed, Callable.From(SetIdleState));
        InventoryManager.Instance.Connect(InventoryManager.SignalName.SlotsStateReset, Callable.From(SetIdleState));
    }

    private enum State
    {
        Idle,
        Hovered,
        ShowsConnections,
        ShowsCycle
    }

    private const float StateChangeDuration = 0.15f;
    private const float GlowRadius = 50f;
    private const float StateScale = 1.025f;
    
    private State _state = State.Idle;
    private Tween? _stateTween;

    public void SetIdleState()
    {
        if (_state == State.Idle)
        {
            return;
        }

        _state = State.Idle;
        
        _stateTween?.Kill();
        _stateTween = CreateTween().SetParallel().SetEase(Tween.EaseType.In).SetTrans(Tween.TransitionType.Quad);

        _stateTween.TweenOffsetScaleReset(this, StateChangeDuration);
        _stateTween.TweenGlowColor(_glow, ColorScheme.LightBlue, StateChangeDuration);
        _stateTween.TweenGlowRadius(_glow, 0, StateChangeDuration);
    }
    
    public void SetHoveredState()
    {
        if (_state == State.Hovered)
        {
            return;
        }

        _state = State.Hovered;
        
        _stateTween?.Kill();
        _stateTween = CreateTween().SetParallel().SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Quad);

        _stateTween.TweenOffsetScale(this, StateScale, StateChangeDuration);
        _stateTween.TweenGlowColor(_glow, ColorScheme.LightBlue, StateChangeDuration);
        _stateTween.TweenGlowRadius(_glow, GlowRadius, StateChangeDuration);
    }
    
    public void SetShowsCycleState()
    {
        if (_state == State.ShowsCycle)
        {
            return;
        }

        _state = State.ShowsCycle;
        
        _stateTween?.Kill();
        _stateTween = CreateTween().SetParallel().SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Quad);

        _stateTween.TweenOffsetScale(this, StateScale, StateChangeDuration);
        _stateTween.TweenGlowColor(_glow, ColorScheme.Red, StateChangeDuration);
        _stateTween.TweenGlowRadius(_glow, GlowRadius, StateChangeDuration);
    }
    
    public void SetShowsConnectionsState()
    {
        if (_state == State.ShowsConnections)
        {
            return;
        }

        _state = State.ShowsConnections;
        
        _stateTween?.Kill();
        _stateTween = CreateTween().SetParallel().SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Quad);

        _stateTween.TweenOffsetScale(this, StateScale, StateChangeDuration);
        _stateTween.TweenGlowColor(_glow, ColorScheme.Yellow, StateChangeDuration);
        _stateTween.TweenGlowRadius(_glow, GlowRadius, StateChangeDuration);
    }

    /// <summary>
    /// Returns global center position of the slot
    /// </summary>
    public Vector2 GetCenterPosition()
    {
        return GetGlobalRect().GetCenter();
    }

    private Tween? _tween;
    
    public void ShowSlot()
    {
        _tween?.Kill();
        _tween = CreateTween().SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.Out).SetParallel();

        _tween.TweenOffsetScaleReset(this, InventoryManager.SlotAnimationDuration);
        _tween.TweenOffsetRotationReset(this, InventoryManager.SlotAnimationDuration);
        _tween.FadeIn(this, InventoryManager.SlotAnimationDuration);
    }
    
    public void HideSlot()
    {
        _tween?.Kill();
        _tween = CreateTween().SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.In).SetParallel();

        _tween.TweenOffsetScale(this, RandomUtils.DeltaRange(0.5f, 0.2f), InventoryManager.SlotAnimationDuration)
            .SetDelay(InventoryManager.SlotHideDelay);
        _tween.TweenOffsetRotation(this, RandomUtils.DeltaRange(0, Pi / 4), InventoryManager.SlotAnimationDuration)
            .SetDelay(InventoryManager.SlotHideDelay);
        _tween.FadeOut(this, InventoryManager.SlotAnimationDuration)
            .SetDelay(InventoryManager.SlotHideDelay);
    }

}