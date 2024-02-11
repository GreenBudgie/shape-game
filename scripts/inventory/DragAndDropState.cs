using Modules;

namespace Inventory;

public struct DragAndDropState
{

    public readonly Vector2 InitialClickPosition;
    public readonly Module ModuleOnCursor;
    public readonly InventorySlot DragFrom;

    public bool IsDraggedOut = false;

    public DragAndDropState(Vector2 initialClickPosition, Module moduleOnCursor, InventorySlot dragFrom)
    {
        InitialClickPosition = initialClickPosition;
        ModuleOnCursor = moduleOnCursor;
        DragFrom = dragFrom;
    }
    
}