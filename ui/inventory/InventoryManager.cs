using System.Collections.Generic;
using System.Linq;

public partial class InventoryManager : Control
{

    private const float FadeDuration = 0.1f;

    [Signal]
    public delegate void InventoryOpenedEventHandler();

    [Signal]
    public delegate void InventoryClosedEventHandler();

    public static InventoryManager Instance { get; private set; } = null!;

    public bool IsOpen { get; private set; } = true;

    [Export] private PlayerInventory _playerInventory = null!;
    [Export] private BlasterInventory _leftBlasterInventory = null!;
    [Export] private BlasterInventory _rightBlasterInventory = null!;

    private InventorySlot? _dragAndDropFrom;
    private List<InventorySlot> _slots = null!;
    private List<ModuleInventory> _inventories = null!;

    private Tween? _alphaTween;

    public override void _EnterTree()
    {
        Instance = this;
    }

    public override void _Ready()
    {
        _inventories =
        [
            _playerInventory,
            _leftBlasterInventory,
            _rightBlasterInventory
        ];
        _slots = _inventories.SelectMany(inventory => inventory.GetSlots()).ToList();

        Visible = false;
        Close();
    }

    public override void _Process(double delta)
    {
        HandleInventoryOpenAndClose();
        HandleDragAndDrop();
    }

    private void HandleInventoryOpenAndClose()
    {
        if (!Input.IsActionJustPressed("inventory"))
        {
            return;
        }

        if (IsOpen)
        {
            Close();
        }
        else
        {
            Open();
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
            _dragAndDropFrom?.GetModule()?.StopFollowingCursor();
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
        _dragAndDropFrom?.GetModule()?.StopFollowingCursor();
        if (_dragAndDropFrom == slot)
        {
            return;
        }

        var draggedModule = _dragAndDropFrom?.RemoveModule();
        slot.InsertModule(draggedModule);
        _dragAndDropFrom = null;
    }

    private void Open()
    {
        if (IsOpen)
        {
            return;
        }

        Input.MouseMode = Input.MouseModeEnum.Visible;
        Visible = true;
        IsOpen = true;
        foreach (var child in GetChildren())
        {
            child.SetProcess(true);
        }

        _alphaTween?.Kill();
        _alphaTween = CreateTween();
        _alphaTween.TweenProperty(
            @object: this,
            property: CanvasItem.PropertyName.Modulate.ToString(),
            finalVal: Colors.White,
            duration: FadeDuration
        );

        EmitSignalInventoryOpened();
    }

    private void Close()
    {
        if (!IsOpen)
        {
            return;
        }

        Input.MouseMode = Input.MouseModeEnum.Hidden;
        IsOpen = false;
        foreach (var child in GetChildren())
        {
            child.SetProcess(false);
        }

        if (Visible)
        {
            _alphaTween?.Kill();
            _alphaTween = CreateTween();
            _alphaTween.TweenProperty(
                @object: this,
                property: CanvasItem.PropertyName.Modulate.ToString(),
                finalVal: Colors.Transparent,
                duration: FadeDuration
            );
            _alphaTween.Finished += FullyHide;
        }
        else
        {
            Modulate = Colors.Transparent;
        }

        EmitSignalInventoryClosed();
    }

    public void _OnInventorySlotInteraction(
        ModuleInventory inventory,
        InventorySlot slot,
        InputEventMouseButton inputEvent
    )
    {
    }

    private void FullyHide()
    {
        Visible = false;
    }

}