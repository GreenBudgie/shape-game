using System.Collections.Generic;
using System.Linq;

public partial class InventoryManager : Control
{

    private const float FadeDuration = 0.1f;

    [Signal]
    public delegate void InventoryOpenedEventHandler();

    [Signal]
    public delegate void InventoryClosedEventHandler();

    [Signal]
    public delegate void DragAndDropStartedEventHandler();
    
    [Signal]
    public delegate void DragAndDropEndedEventHandler();

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

        Callable.From(PostSetup).CallDeferred();
    }

    private void PostSetup()
    {
        PlayerInventory.GetSlot(0).InsertModule(
            UiModule.Create(ModuleManager.GetModule<BoltModule>())
        );
        
        RightBlasterInventory.GetSlot(5).InsertModule(
            UiModule.Create(ModuleManager.GetModule<MiniSphereModule>())
        );
        RightBlasterInventory.GetSlot(4).InsertModule(
            UiModule.Create(ModuleManager.GetModule<MineModule>())
        );
        RightBlasterInventory.GetSlot(3).InsertModule(
            UiModule.Create(ModuleManager.GetModule<ExtraDamageModule>())
        );
        
        LeftBlasterInventory.GetSlot(5).InsertModule(
            UiModule.Create(ModuleManager.GetModule<WallModule>())
        );
        LeftBlasterInventory.GetSlot(4).InsertModule(
            UiModule.Create(ModuleManager.GetModule<ExtraFireRateModule>())
        );
        LeftBlasterInventory.GetSlot(3).InsertModule(
            UiModule.Create(ModuleManager.GetModule<ExtraFireRateModule>())
        );
        LeftBlasterInventory.GetSlot(2).InsertModule(
            UiModule.Create(ModuleManager.GetModule<ExtraFireRateModule>())
        );
        LeftBlasterInventory.GetSlot(1).InsertModule(
            UiModule.Create(ModuleManager.GetModule<PiercingModule>())
        );
        LeftBlasterInventory.GetSlot(0).InsertModule(
            UiModule.Create(ModuleManager.GetModule<PiercingModule>())
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

    public bool IsDragAndDropActive()
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
        EmitSignalDragAndDropStarted();
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
        
        EmitSignalDragAndDropEnded();
    }

    private void CancelDragAndDrop()
    {
        if (_dragAndDropFrom == null)
        {
            return;
        }
        
        _dragAndDropFrom.GetModule()?.StopFollowingCursor();
        _dragAndDropFrom = null;
        EmitSignalDragAndDropEnded();
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
            child.ProcessMode = ProcessModeEnum.Inherit;
        }

        _alphaTween?.Kill();
        _alphaTween = CreateTween();
        _alphaTween.TweenProperty(
            @object: this,
            property: ModulateProperty,
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
            child.ProcessMode = ProcessModeEnum.Disabled;
        }

        if (Visible)
        {
            _alphaTween?.Kill();
            _alphaTween = CreateTween();
            _alphaTween.TweenProperty(
                @object: this,
                property: ModulateProperty,
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