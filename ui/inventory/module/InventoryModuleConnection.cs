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
        
        _connector.Reparent(InventoryManager.Instance);
        _connector.Hide();
        HideConnector();
        
        _module.Connect(InventoryModule.SignalName.Inserted, Callable.From(OnModuleInserted));
        _module.Connect(InventoryModule.SignalName.TakenOut, Callable.From(OnModuleTakenOut));
    }

    public override void _ExitTree()
    {
        _connector.QueueFree();
    }

    public override void _Process(double delta)
    {
        _connector.GlobalPosition = GlobalPosition;
        _connector.GlobalRotation = GlobalRotation;
    }

    private void OnModuleInserted()
    {
        ShowConnector();
    }

    private void OnModuleTakenOut()
    {
        HideConnector();
    }

    private const float ConnectorTweenDuration = 0.15f;
    private Tween? _connectorTween;
    

    private void ShowConnector()
    {
        _connectorTween?.Kill();
        _connector.Show();
        _connectorTween = CreateTween().SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.Out);
        _connectorTween.TweenScaleReset(_connector, ConnectorTweenDuration);
    }
    
    private void HideConnector()
    {
        _connectorTween?.Kill();
        _connectorTween = CreateTween().SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.In);
        _connectorTween.TweenScale(_connector, new Vector2(1, 0), ConnectorTweenDuration);
        _connectorTween.TweenCallback(Callable.From(_connector.Hide));
    }

}
