using System.Linq;

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
        MouseInputManager.Instance.DisableAttack();

        var allModulesCopy = ModuleManager.Modules.ToList();
        for (var i = 0; i < 3; i++)
        {
            var module = allModulesCopy.GetRandom();
            var shopModule = ShopModule.Create(module);
            _modules.AddChild(shopModule);

            allModulesCopy.Remove(module);
        }

        Visible = true;
    }

    private void HideShop()
    {
        Callable.From(MouseInputManager.Instance.EnableAttack).CallDeferred();
        Visible = false;

        foreach (var module in _modules.GetChildren())
        {
            module.QueueFree();
        }
    }
}