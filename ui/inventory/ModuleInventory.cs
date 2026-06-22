using System.Collections.Generic;
using System.Linq;

public partial class ModuleInventory : Control
{
    public const float DistanceBetweenSlots = 200;

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

    public InventorySlot GetSlot(HexCoordinates coordinates)
    {
        return _slots[coordinates];
    }

    public List<InventorySlot> GetSlots()
    {
        return _slots.Values.ToList();
    }

    /// <summary>
    /// Add the module to the first empty slot, or does nothing if there are no empty slots
    /// </summary>
    /// <param name="module">Module to add</param>
    /// <returns>True if module was added, false otherwise</returns>
    public bool AddModule(Module module)
    {
        var firstEmptySlot = _slots.FirstOrDefault(slot => !slot.HasModule());
        if (firstEmptySlot == null)
        {
            return false;
        }

        firstEmptySlot.InsertModule(InventoryModule.Create(module));
        return true;
    }

    private void CreateSlots(Vector2 center)
    {
        const int radius = 3;

        var coordinates = HexCoordinates.Spiral(radius);
        foreach (var hex in coordinates)
        {
            var slot = InventorySlot.Create(hex);
            slot.GlobalPosition = center + hex.ToVector() * DistanceBetweenSlots;
            if (hex.Length() == radius)
            {
                slot.SetDisabled(true);
            }

            AddChild(slot);
            _slots.Add(hex, slot);
        }
    }
}