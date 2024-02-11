using Modules;

namespace Inventory;

public partial class InventorySlot : TextureButton
{

    public Module InsertModule(Module module)
    {
        if (module == null)
        {
            RemoveModule();
        } 
        var previousModule = RemoveModule();
        AddChild(module);
        return previousModule;
    }

    public Module GetModule()
    {
        return GetChildOrNull<Module>(0);
    }

    public Module RemoveModule()
    {
        var module = GetModule();
        if (module == null)
        {
            return null;
        }
        RemoveChild(module);
        return module;
    }

    public bool HasModule()
    {
        return GetModule() != null;
    }

}
