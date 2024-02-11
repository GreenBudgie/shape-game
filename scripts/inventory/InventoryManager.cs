using Godot;
using System;
using System.Collections.Generic;
using Common;
using Inventory;
using Modules;

namespace Inventory;

public partial class InventoryManager : CanvasLayer
{
    [Signal]
    public delegate void InventoryOpenEventHandler();

    [Signal]
    public delegate void InventoryCloseEventHandler();

    [Node] private PlayerInventory _playerInventory;

    private DragAndDropState? _dragAndDropState;
    private List<InventorySlot> _slots = new();

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

        var leftPressed = Input.IsActionJustPressed("inventory_left_click");
        var leftReleased = Input.IsActionJustReleased("inventory_left_click");
        var leftReleaseHandled = false;
        foreach (var inventorySlot in _slots)
        {
            if (!inventorySlot.IsHovered())
            {
                continue;
            }

            if (leftPressed)
            {
                var module = inventorySlot.GetModule();
                if (module == null)
                {
                    break;
                }
                _dragAndDropState = new DragAndDropState(
                    GetViewport().GetMousePosition(),
                    module,
                    inventorySlot
                );
                module.StartFollowingCursor();
                break;
            }

            if (leftReleased && _dragAndDropState.HasValue)
            {
                leftReleaseHandled = true;
                var dragAndDrop = _dragAndDropState.Value;
                _dragAndDropState = null;
                dragAndDrop.ModuleOnCursor.StopFollowingCursor();
                if (dragAndDrop.DragFrom == inventorySlot)
                {
                    break;
                }

                var draggedModule = dragAndDrop.DragFrom.RemoveModule();
                inventorySlot.InsertModule(draggedModule);
            }

            break;
        }

        if (leftReleased && _dragAndDropState.HasValue && !leftReleaseHandled)
        {
            var dragAndDrop = _dragAndDropState.Value;
            dragAndDrop.ModuleOnCursor.StopFollowingCursor();
            _dragAndDropState = null;
        }
    }

    public bool IsOpen()
    {
        return Visible;
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