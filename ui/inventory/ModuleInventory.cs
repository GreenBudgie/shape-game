using System.Collections.Generic;
using System.Linq;
using Godot.Collections;

public partial class ModuleInventory : Control
{
    private List<InventorySlot> _slots = null!;

    public override void _Ready()
    {
        _slots = FindSlots(GetChildren());
    }
    
    public InventorySlot GetSlot(int slotIndex)
    {
        return _slots[slotIndex];
    }

    public List<InventorySlot> GetSlots()
    {
        return _slots;
    }

    private List<InventorySlot> FindSlots(Array<Node> children)
    {
        if (children.Count == 0)
        {
            return [];
        }

        var foundSlots = new List<InventorySlot>();
        foreach (var child in children)
        {
            if (child is InventorySlot slot)
            {
                foundSlots.Add(slot);
                continue;
            }

            foundSlots.AddRange(FindSlots(child.GetChildren()));
        }

        return foundSlots.OrderBy(slot => slot.Number).ToList();
    }
}