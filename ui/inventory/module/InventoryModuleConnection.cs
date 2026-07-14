public partial class InventoryModuleConnection : Node2D
{

    private static readonly PackedScene Scene = GD.Load<PackedScene>("uid://5kibo2t6giff");
    
    private Sprite2D _arrow = null!;
    private Sprite2D _connector = null!;
    private Glow _arrowGlow = null!;

    public InventoryModule Module { get; private set; } = null!;
    public InventorySlot? Slot { get; set; }
    
    public static InventoryModuleConnection Create(InventoryModule module)
    {
        var node = Scene.Instantiate<InventoryModuleConnection>();
        node.Module = module;
        return node;
    }
    
    public override void _Ready()
    {
        _arrow = GetNode<Sprite2D>("Arrow");
        _connector = GetNode<Sprite2D>("Connector");

        _arrow.Modulate = Module.Module.Color;
        
        _arrow.Reparent(InventoryManager.Instance);
        _connector.Reparent(InventoryManager.Instance);
        Hide();
        HideConnector(true);

        _arrowGlow = Glow.AddGlow(_arrow)
            .SetColor(_arrow.Modulate)
            .SetRadius(0)
            .SetStrength(1);

        Module.Connect(InventoryModule.SignalName.Inserted, Callable.From(OnModuleInserted));
        Module.Connect(InventoryModule.SignalName.ShowAnimationFinished, Callable.From(ShowConnector));
        Module.Connect(InventoryModule.SignalName.TakenOut, Callable.From(() => HideConnector(false)));
        InventoryManager.Instance.Connect(
            InventoryManager.SignalName.InventoryClosed,
            Callable.From(() => HideConnector(true))
        );
    }

    private void OnModuleInserted()
    {
        SetIdleState();
        ShowConnector();
    }

    public override void _ExitTree()
    {
        _connector.QueueFree();
    }

    public override void _Process(double delta)
    {
        if (!InventoryManager.Instance.IsOpen)
        {
            return;
        }
        
        _arrow.GlobalPosition = GlobalPosition;
        _arrow.GlobalRotation = GlobalRotation;

        if (Module.IsFollowingCursor)
        {
            return;
        }
        
        _connector.GlobalPosition = GlobalPosition;
        _connector.GlobalRotation = GlobalRotation;
    }

    private enum State
    {
        Idle,
        NewConnection,
        InvalidConnection
    }

    private const float StateChangeDuration = 0.1f;
    private const float GlowRadius = 20f;

    private State _state = State.Idle;
    private Tween? _arrowTween;

    public void SetInvalidConnectionState()
    {
        if (_state == State.InvalidConnection)
        {
            return;
        }

        _state = State.InvalidConnection;
        
        _arrowTween?.Kill();
        _arrowTween = CreateTween().SetParallel().SetEase(Tween.EaseType.InOut).SetTrans(Tween.TransitionType.Quad);

        _arrowTween.TweenModulate(_arrow, ColorScheme.Red, StateChangeDuration);
        _arrowTween.TweenScaleReset(_arrow, StateChangeDuration);
        _arrowTween.TweenGlowColor(_arrowGlow, ColorScheme.Red, StateChangeDuration);
        _arrowTween.TweenGlowRadius(_arrowGlow, GlowRadius, StateChangeDuration);
    }
    
    public void SetIdleState()
    {
        if (_state == State.Idle)
        {
            return;
        }

        _state = State.Idle;
        
        _arrowTween?.Kill();
        _arrowTween = CreateTween().SetParallel().SetEase(Tween.EaseType.InOut).SetTrans(Tween.TransitionType.Quad);

        _arrowTween.TweenModulate(_arrow, Module.Module.Color, StateChangeDuration);
        _arrowTween.TweenScaleReset(_arrow, StateChangeDuration);
        _arrowTween.TweenGlowColor(_arrowGlow, Module.Module.Color, StateChangeDuration);
        _arrowTween.TweenGlowRadius(_arrowGlow, 0, StateChangeDuration);
    }
    
    public void SetNewConnectionState()
    {
        if (_state == State.NewConnection)
        {
            return;
        }

        _state = State.NewConnection;
        
        _arrowTween?.Kill();
        _arrowTween = CreateTween().SetParallel().SetEase(Tween.EaseType.InOut).SetTrans(Tween.TransitionType.Quad);

        _arrowTween.TweenModulate(_arrow, Module.Module.Color, StateChangeDuration);
        _arrowTween.TweenScale(_arrow, 1.25f, StateChangeDuration);
        _arrowTween.TweenGlowColor(_arrowGlow, Module.Module.Color, StateChangeDuration);
        _arrowTween.TweenGlowRadius(_arrowGlow, GlowRadius, StateChangeDuration);
    }

    private const float ConnectorTweenDuration = 0.15f;
    private Tween? _connectorTween;

    private void ShowConnector()
    {
        if (!InventoryManager.Instance.IsOpen)
        {
            return;
        }
        
        _connectorTween?.Kill();
        
        _arrow.Show();

        if (IsConnectedToValidSlot)
        {
            _connector.Show();
        }

        _connectorTween = CreateTween().SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.Out).SetParallel();
        _connectorTween.FadeIn(_arrow, ConnectorTweenDuration / 2);
        
        if (IsConnectedToValidSlot)
        {
            _connectorTween.TweenScaleReset(_connector, ConnectorTweenDuration);
            _connectorTween.FadeIn(_connector, ConnectorTweenDuration / 2);
        }
    }
    
    private void HideConnector(bool hideArrow)
    {
        _connectorTween?.Kill();
        _connectorTween = CreateTween().SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.In).SetParallel();
        _connectorTween.TweenScale(_connector, new Vector2(1, 0.1f), ConnectorTweenDuration);
        _connectorTween.FadeOut(_connector, ConnectorTweenDuration / 2).SetDelay(ConnectorTweenDuration / 2);
        
        if (hideArrow)
        {
            _connectorTween.FadeOut(_arrow, ConnectorTweenDuration / 2).SetDelay(ConnectorTweenDuration / 2);
            _connectorTween.Finished += _arrow.Hide;
        }
        
        _connectorTween.Finished += _connector.Hide;
    }

    private bool IsConnectedToValidSlot => Slot != null && !Slot.IsDisabled();

}
