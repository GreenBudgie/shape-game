public partial class ModuleSelector : Control
{

    private VFlowContainer _container = null!;
    
    public override void _Ready()
    {
        _container = GetNode<VFlowContainer>("ModuleSelectorContainer");
        
        foreach (var module in ModuleTypeRegistry.Types)
        {
            AddModuleButton(module);
        }
    }

    private void AddModuleButton(ModuleType moduleType)
    {
        var button = new Button
        {
            Text = moduleType.Name
        };
        button.Pressed += () => SpawnModule(moduleType);
        _container.AddChild(button);
    }

    private void SpawnModule(ModuleType moduleType)
    {
        InventoryManager.Instance.AddModule(moduleType);
    }
}
