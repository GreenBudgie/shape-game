public partial class ModuleSelector : Control
{

    private VFlowContainer _container = null!;
    
    public override void _Ready()
    {
        _container = GetNode<VFlowContainer>("ModuleSelectorContainer");
        
        foreach (var module in ModuleRegistry.Modules)
        {
            AddModuleButton(module);
        }
    }

    private void AddModuleButton(Module module)
    {
        var button = new Button
        {
            Text = module.Name
        };
        button.Pressed += () => SpawnModule(module);
        _container.AddChild(button);
    }

    private void SpawnModule(Module module)
    {
        InventoryManager.Instance.AddModule(module);
    }
}
