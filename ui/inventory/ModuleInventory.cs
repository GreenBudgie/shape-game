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

        InventoryManager.Instance.InventoryOpened += ShowSlots;
        InventoryManager.Instance.InventoryClosed += HideSlots;
    }
    
    public List<InventoryModule> GetModules<T>() where T : Module
    {
        return GetModules().Where(x => x.Module is T).ToList();
    }

    public List<InventoryModule> GetModules()
    {
        return GetSlots().Select(x => x.Module).OfType<InventoryModule>().Distinct().ToList();
    }

    public bool TryInsertModule(InventoryModule inventoryModule)
    {
        foreach (var slot in GetSlots())
        {
            var inserted = inventoryModule.TryInsert(slot);
            if (inserted)
            {
                return true;
            }
        }

        return false;
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
            // if (hex.Length() == radius - 1)
            // {
            //     slot.SetDisabled(true);
            // }

            slot.Modulate = slot.Modulate.AsTransparent();
            AddChild(slot);
            _slots.Add(hex, slot);
        }
        
        HideSlots();
    }

    private void ShowSlots()
    {
        foreach (var slot in GetSlots())
        {
            slot.ShowSlot();
        }
    }

    private void HideSlots()
    {
        foreach (var slot in GetSlots())
        {
            slot.HideSlot();
        }
    }
}