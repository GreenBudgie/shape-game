using Godot;
using System;
using Modules;

public partial class InventorySlot : TextureRect
{

    public Module ReplaceModule(Module module)
    {
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
