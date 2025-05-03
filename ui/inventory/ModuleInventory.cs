using Godot.Collections;

public partial class ModuleInventory : Control
{
    private Array<InventorySlot> _slots;

    public override void _Ready()
    {
        _slots = FindSlots(GetChildren());
    }

    public Array<InventorySlot> GetSlots()
    {
        return _slots;
    }

    private Array<InventorySlot> FindSlots(Array<Node> children)
    {
        if (children.Count == 0)
        {
            return new Array<InventorySlot>();
        }

        var foundSlots = new Array<InventorySlot>();
        foreach (var child in children)
        {
            if (child is InventorySlot slot)
            {
                foundSlots.Add(slot);
                continue;
            }

            foundSlots.AddRange(FindSlots(child.GetChildren()));
        }

        return foundSlots;
    }
}