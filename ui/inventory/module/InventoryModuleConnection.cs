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
        _connector.Show();
    }

    private void OnModuleTakenOut()
    {
        _connector.Hide();
    }

}
