using System.Collections.Generic;
using System.Linq;

public partial class Shop : Node2D
{
    private const int NumberOfModules = 3;
    
    public static Shop Instance { get; private set; } = null!;

    private List<WorldModule> _shopModules = [];

    public Shop()
    {
        Instance = this;
    }

    public override void _Ready()
    {
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
        const float moduleGap = 500f;
        const float halfNumberOfModules = (NumberOfModules - 1f) / 2f;
        var firstModulePositionX = ShapeGame.Center.X - moduleGap * halfNumberOfModules;
        
        var allModulesCopy = ModuleRegistry.Modules.ToList();
        for (var i = 0; i < NumberOfModules; i++)
        {
            var module = allModulesCopy.GetRandom();
            var shopModule = WorldModule.Create(module);
            
            shopModule.GlobalPosition = new Vector2(firstModulePositionX + i * moduleGap, ShapeGame.Center.Y);
            shopModule.Rotation = 0;
            
            WorldModuleManager.Instance.SpawnModule(shopModule);
            shopModule.SetInShop();
            _shopModules.Add(shopModule);

            allModulesCopy.Remove(module);
        }
    }

    private void HideShop()
    {
        foreach (var module in _shopModules)
        {
            module.Remove();
        }
    }
}