using System;
using Godot.Collections;
using Modules;

namespace Inventory;

public partial class ModuleInventory : Control
{

    [Export] private Array<InventorySlot> _slots;
    
    
    
}