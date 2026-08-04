using System.Collections.Generic;
using System.Linq;

public partial class WorldModuleManager : Node
{

    public static WorldModuleManager Instance { get; private set; } = null!;

    private readonly List<WorldModule> _orderedWorldModules = [];
    private WorldModule? _selectedModule;

    public WorldModuleManager()
    {
        Instance = this;
    }

    public void SpawnModule(WorldModule module)
    {
        _orderedWorldModules.Insert(0, module);
        ShapeGame.Instance.AddChild(module);
    }

    public void OnModuleRemoved(WorldModule module)
    {
        _orderedWorldModules.Remove(module);
        
        if (_selectedModule != module)
        {
            return;
        }
        
        _selectedModule = null;
        ReselectModule();
    }

    public void ModuleHovered(WorldModule module)
    {
        ReselectModule();
    }
    
    public void ModuleUnhovered(WorldModule module)
    {
        ReselectModule();
    }

    private void SelectModule(WorldModule module)
    {
        if (_selectedModule == module)
        {
            return;
        }
        
        DeselectModule();
        _selectedModule = module;
        module.OnSelect();
    }

    private void DeselectModule()
    {
        _selectedModule?.OnDeselect();
        _selectedModule = null;
    }

    private void ReselectModule()
    {
        foreach (var worldModule in _orderedWorldModules)
        {
            if (worldModule.IsHovered())
            {
                SelectModule(worldModule);
                return;
            }
        }

        DeselectModule();
    }
    
}