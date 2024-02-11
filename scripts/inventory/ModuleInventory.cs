using Godot.Collections;

namespace Inventory;

public partial class ModuleInventory : Control
{
    [Export] private Array<InventorySlot> _slots;

    public Array<InventorySlot> GetSlots()
    {
        return _slots;
    }
}