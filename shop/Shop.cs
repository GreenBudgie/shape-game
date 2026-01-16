public partial class ShopManager : Node2D
{
    public static ShopManager Instance { get; private set; } = null!;

    private HBoxContainer _modules = null!;
    
    public ShopManager()
    {
        Instance = this;
    }

    public override void _Ready()
    {
        _modules = GetNode<HBoxContainer>("%Modules");
        
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
        Visible = false;
        
        foreach (var module in _modules.GetChildren())
        {
            module.QueueFree();
        }
    }
}