using Godot.Collections;

namespace Inventory;

public partial class ModuleInventory : Control
{
    [Export] private Array<InventorySlot> _slots;

    [Signal]
    public delegate void InventorySlotInteractionEventHandler(
        ModuleInventory inventory,
        InventorySlot slot,
        InputEventMouseButton inputEvent
    );

    public override void _Ready()
    {
        foreach (var inventorySlot in _slots)
        {
            inventorySlot.SlotInteraction += (slot, inputEvent) =>
                EmitSignal(SignalName.InventorySlotInteraction, this, slot, inputEvent);
        }
    }

    public Array<InventorySlot> GetSlots()
    {
        return _slots;
    }

}