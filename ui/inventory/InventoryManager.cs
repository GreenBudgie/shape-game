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

    [Export] public PlayerInventory PlayerInventory { get; private set; } = null!;
    [Export] public BlasterInventory LeftBlasterInventory { get; private set; } = null!;
    [Export] public BlasterInventory RightBlasterInventory { get; private set; } = null!;

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
            PlayerInventory,
            LeftBlasterInventory,
            RightBlasterInventory
        ];
        _slots = _inventories.SelectMany(inventory => inventory.GetSlots()).ToList();

        CallDeferred(MethodName.PostSetup);
    }

    private void PostSetup()
    {
        LeftBlasterInventory.GetSlot(5).InsertModule(
            UiModule.Create(GD.Load<BoltModule>("uid://cqjg5lcuad1hd"))
        );
        LeftBlasterInventory.GetSlot(4).InsertModule(
            UiModule.Create(GD.Load<ExtraFireRateModule>("uid://dixg0bdyqb2ay"))
        );
        LeftBlasterInventory.GetSlot(3).InsertModule(
            UiModule.Create(GD.Load<ExtraFireRateModule>("uid://dixg0bdyqb2ay"))
        );
        LeftBlasterInventory.GetSlot(2).InsertModule(
            UiModule.Create(GD.Load<ExtraFireRateModule>("uid://dixg0bdyqb2ay"))
        );
        LeftBlasterInventory.GetSlot(1).InsertModule(
            UiModule.Create(GD.Load<BoltModule>("uid://cqjg5lcuad1hd"))
        );
        
        Close();
        Visible = false;
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
        var hoveredSlot = _slots.Find(slot => slot.IsHovered());

        if (hoveredSlot != null)
        {
            if (leftPressed)
            {
                StartDragAndDropFromSlot(hoveredSlot);
            }

            if (leftReleased && IsDragAndDropActive())
            {
                leftReleaseHandled = true;
                StopDragAndDropSwappingModulesWithSlot(hoveredSlot);
            }
        }

        if (leftReleased && IsDragAndDropActive() && !leftReleaseHandled)
        {
            CancelDragAndDrop();
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
        if (_dragAndDropFrom == null)
        {
            return;
        }
        
        if (_dragAndDropFrom == slot)
        {
            CancelDragAndDrop();
            return;
        }
        
        _dragAndDropFrom.GetModule()?.StopFollowingCursor();
        
        _dragAndDropFrom.SwapModules(slot);
        _dragAndDropFrom = null;
    }

    private void CancelDragAndDrop()
    {
        if (_dragAndDropFrom == null)
        {
            return;
        }
        
        var module = _dragAndDropFrom.GetModule();
        if (module != null)
        {
            module.StopFollowingCursor();
        }
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