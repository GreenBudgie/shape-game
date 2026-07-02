public partial class InventoryModuleConnection : Node2D
{

    private Sprite2D _arrow = null!;
    private Sprite2D _connector = null!;

    private static readonly PackedScene Scene = GD.Load<PackedScene>("uid://5kibo2t6giff");

    private InventoryModule _module = null!;
    
    public static InventoryModuleConnection Create(InventoryModule module)
    {
        var node = Scene.Instantiate<InventoryModuleConnection>();
        node._module = module;
        return node;
    }
    
    public override void _Ready()
    {
        _arrow = GetNode<Sprite2D>("Arrow");
        _connector = GetNode<Sprite2D>("Connector");
        
        _arrow.Reparent(InventoryManager.Instance);
        _connector.Reparent(InventoryManager.Instance);
        Hide();
        HideConnector(true);

        _module.Connect(InventoryModule.SignalName.Inserted, Callable.From(ShowConnector));
        _module.Connect(InventoryModule.SignalName.ShowAnimationFinished, Callable.From(ShowConnector));
        _module.Connect(InventoryModule.SignalName.TakenOut, Callable.From(() => HideConnector(false)));
        InventoryManager.Instance.Connect(
            InventoryManager.SignalName.InventoryClosed,
            Callable.From(() => HideConnector(true))
        );
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

        if (_module.IsFollowingCursor)
        {
            return;
        }
        
        _connector.GlobalPosition = GlobalPosition;
        _connector.GlobalRotation = GlobalRotation;
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
        
        _connector.Show();
        _arrow.Show();
        
        _connectorTween = CreateTween().SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.Out).SetParallel();
        _connectorTween.TweenScaleReset(_connector, ConnectorTweenDuration);
        _connectorTween.FadeIn(_connector, ConnectorTweenDuration / 2);
        _connectorTween.FadeIn(_arrow, ConnectorTweenDuration / 2);
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

}
