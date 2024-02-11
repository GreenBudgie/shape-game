using System.Collections.Generic;
using System.Linq;
using Common;

namespace Inventory;

public partial class InventoryManager : CanvasLayer
{
    [Signal]
    public delegate void InventoryOpenEventHandler();

    [Signal]
    public delegate void InventoryCloseEventHandler();

    [Node] private PlayerInventory _playerInventory;

    private InventorySlot _dragAndDropFrom;
    private readonly List<InventorySlot> _slots = new();

    public override void _Ready()
    {
        this.InitAttributes();
        foreach (var inventorySlot in _playerInventory.GetSlots())
        {
            _slots.Add(inventorySlot);
        }

        Close();
    }

    public override void _Process(double delta)
    {
        HandleInventoryOpenAndClose();
        HandleDragAndDrop();
    }

    public bool IsOpen()
    {
        return Visible;
    }

    private void HandleInventoryOpenAndClose()
    {
        if (Input.IsActionJustPressed("inventory"))
        {
            if (IsOpen())
            {
                Close();
            }
            else
            {
                Open();
            }
        }
    }

    private bool IsDragAndDropActive()
    {
        return _dragAndDropFrom != null;
    }

    private void HandleDragAndDrop()
    {
        var leftPressed = Input.IsActionJustPressed("inventory_left_click");
        var leftReleased = Input.IsActionJustReleased("inventory_left_click");
        var leftReleaseHandled = false;
        var hoveredSlots = _slots.Where(slot => slot.IsHovered());
        foreach (var slot in hoveredSlots)
        {
            if (leftPressed)
            {
                StartDragAndDropFromSlot(slot);
                break;
            }

            if (leftReleased && IsDragAndDropActive())
            {
                leftReleaseHandled = true;
                StopDragAndDropSwappingModulesWithSlot(slot);
            }
        }

        if (leftReleased && IsDragAndDropActive() && !leftReleaseHandled)
        {
            _dragAndDropFrom.GetModule().StopFollowingCursor();
            _dragAndDropFrom = null;
        }
    }

    private void StartDragAndDropFromSlot(InventorySlot slot)
    {
        var module = slot.GetModule();
        if (module == null)
        {
            return;
        }

        _dragAndDropFrom = slot;
        module.StartFollowingCursor();
    }

    private void StopDragAndDropSwappingModulesWithSlot(InventorySlot slot)
    {
        _dragAndDropFrom.GetModule().StopFollowingCursor();
        if (_dragAndDropFrom == slot)
        {
            return;
        }

        var draggedModule = _dragAndDropFrom.RemoveModule();
        slot.InsertModule(draggedModule);
        _dragAndDropFrom = null;
    }

    private void Open()
    {
        if (IsOpen())
        {
            return;
        }

        Visible = true;
        foreach (var child in GetChildren())
        {
            child.SetProcess(true);
        }

        EmitSignal(SignalName.InventoryOpen);
    }

    private void Close()
    {
        if (!IsOpen())
        {
            return;
        }

        Visible = false;
        foreach (var child in GetChildren())
        {
            child.SetProcess(false);
        }

        EmitSignal(SignalName.InventoryClose);
    }

    public void _OnInventorySlotInteraction(
        ModuleInventory inventory,
        InventorySlot slot,
        InputEventMouseButton inputEvent
    )
    {
    }
}