using System.Collections.Generic;
using System.Linq;

public partial class ModuleInventory : Control
{
    [Export] public bool IsLeft { get; private set; }

    private readonly Dictionary<HexCoordinates, InventorySlot> _slots = [];

    public override void _Ready()
    {
        var distanceFromCenter = ShapeGame.WindowCenter.X / 3;
        
        Vector2 centerSlotPosition;
        if (IsLeft)
        {
            centerSlotPosition = new Vector2(ShapeGame.WindowCenter.X - distanceFromCenter, ShapeGame.WindowCenter.Y);
        }
        else
        {
            centerSlotPosition = new Vector2(ShapeGame.WindowCenter.X + distanceFromCenter, ShapeGame.WindowCenter.Y);
        }

        CreateSlots(centerSlotPosition);
    }

    public InventorySlot? TryGetSlot(HexCoordinates coordinates)
    {
        return _slots.GetValueOrDefault(coordinates);
    }
    
    public InventorySlot GetSlot(HexCoordinates coordinates)
    {
        return _slots[coordinates];
    }

    public List<InventorySlot> GetSlots()
    {
        return _slots.Values.ToList();
    }

    private void CreateSlots(Vector2 center)
    {
        const int radius = 3;

        var coordinates = HexCoordinates.Spiral(radius);
        foreach (var hex in coordinates)
        {
            var position = center + hex.ToPixel();
            
            var slot = InventorySlot.Create(this, hex);
            slot.GlobalPosition = position - slot.Size / 2;
            if (hex.Length() == radius - 1)
            {
                slot.SetDisabled(true);
            }

            AddChild(slot);
            _slots.Add(hex, slot);
        }
    }
}