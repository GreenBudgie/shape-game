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
        const int edgeSlots = 3;
        const int disableSlotsEdgeDistance = 1;
        
        const int verticalSlots = edgeSlots * 2 - 1;

        var firstSlotPosition = center + HexDirection.TopLeft.ToVector(DistanceBetweenSlots * (edgeSlots - 1));

        var currentVerticalPosition = firstSlotPosition;
        var currentSlotNumber = 0;
        for (var vertical = 0; vertical < verticalSlots; vertical++)
        {
            var reachedCenter = vertical >= edgeSlots - 1;
            int horizontalSlots;
            if (reachedCenter)
            {
                horizontalSlots = edgeSlots * 3 - vertical - 2;
            }
            else
            {
                horizontalSlots = vertical + edgeSlots;
            }

            for (var horizontal = 0; horizontal < horizontalSlots; horizontal++)
            {
                var position = currentVerticalPosition +
                               HexDirection.Right.ToVector(DistanceBetweenSlots * horizontal);
                var correctedPosition = position - new Vector2(DistanceBetweenSlots / 2, DistanceBetweenSlots / 2);

                var disableSlot = vertical < disableSlotsEdgeDistance ||
                                  verticalSlots - vertical <= disableSlotsEdgeDistance ||
                                  horizontal < disableSlotsEdgeDistance ||
                                  horizontalSlots - horizontal <= disableSlotsEdgeDistance;

                var slot = InventorySlot.Create(currentSlotNumber);
                slot.GlobalPosition = correctedPosition;
                if (disableSlot)
                {
                    slot.SetDisabled(true);
                }

                AddChild(slot);
                _slots.Add(slot);
                
                currentSlotNumber++;
            }

            if (reachedCenter)
            {
                currentVerticalPosition += HexDirection.BottomRight.ToVector(DistanceBetweenSlots);
            }
            else
            {
                currentVerticalPosition += HexDirection.BottomLeft.ToVector(DistanceBetweenSlots);
            }
        }
    }
}