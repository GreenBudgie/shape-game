public partial class Shop : Node2D
{
    public static Shop Instance { get; private set; } = null!;

    private VBoxContainer _modules = null!;
    
    public Shop()
    {
        Instance = this;
    }

    public override void _Ready()
    {
        _modules = GetNode<VBoxContainer>("%Modules");
        
        HideShop();
        GamePhaseManager.Instance.PhaseChanged += OnPhaseChanged;
    }

    private void OnPhaseChanged(GamePhase phase)
    {
        if (phase == GamePhase.Shop)
        {
            ShowShop();
        }
        else
        {
            HideShop();
        }
    }

    private void ShowShop()
    {
        MouseInputManager.Instance.IsAttackEnabled = false;
        
        for (var i = 0; i < 3; i++)
        {
            var module = ModuleManager.Modules.GetRandom();
            var shopModule = ShopModule.Create(module);
            _modules.AddChild(shopModule);
        }
        
        Visible = true;
    }

    private void HideShop()
    {
        MouseInputManager.Instance.IsAttackEnabled = true;
        Visible = false;
        
        foreach (var module in _modules.GetChildren())
        {
            module.QueueFree();
        }
    }
}