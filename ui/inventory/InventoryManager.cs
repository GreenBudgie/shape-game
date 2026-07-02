using System.Collections.Generic;
using System.Linq;

public partial class InventoryManager : Control
{

    public const float AnimationDuration = 0.15f;
    public const float SlotAnimationDuration = AnimationDuration;
    public const float SlotHideDelay = AnimationDuration / 2f;
    public const float ModuleShowDelay = AnimationDuration / 2f;
    public const float ModuleAnimationDuration = AnimationDuration;

    [Signal]
    public delegate void InventoryOpenedEventHandler();

    [Signal]
    public delegate void InventoryClosedEventHandler();

    public static InventoryManager Instance { get; private set; } = null!;

    public bool IsOpen { get; private set; } = true;

    [Export] public ModuleInventory LeftBlasterInventory { get; private set; } = null!;
    [Export] public ModuleInventory RightBlasterInventory { get; private set; } = null!;

    private ColorRect _overlay = null!;
    
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
            LeftBlasterInventory,
            RightBlasterInventory
        ];
        _slots = _inventories.SelectMany(inventory => inventory.GetSlots()).ToList();

        _overlay = GetNode<ColorRect>("Overlay");
        
        Callable.From(PostSetup).CallDeferred();
    }

    public List<InventorySlot> GetAllSlots()
    {
        return _slots;
    }
    
    public List<InventorySlot> GetActiveSlots()
    {
        return _slots.Where(slot => !slot.IsDisabled()).ToList();
    }

    private void PostSetup()
    {
        var module = ModuleManager.GetModule<PiercingModule>();
        var inventoryModule = InventoryModule.Create(module);
        AddChild(inventoryModule);
        var slot = LeftBlasterInventory.GetSlot(HexCoordinates.Zero);
        inventoryModule.TryInsert(slot);
        
        Close();
        Visible = false;
    }

    public override void _Process(double delta)
    {
        HandleInventoryOpenAndClose();
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

    private void Open()
    {
        if (IsOpen)
        {
            return;
        }

        Input.MouseMode = Input.MouseModeEnum.Visible;
        Visible = true;
        MouseFilter = MouseFilterEnum.Stop;
        IsOpen = true;

        _alphaTween?.Kill();
        _alphaTween = _overlay.CreateTween();
        _alphaTween.FadeIn(_overlay, AnimationDuration);

        EmitSignalInventoryOpened();
    }

    private void Close()
    {
        if (!IsOpen)
        {
            return;
        }

        IsOpen = false;

        if (Visible)
        {
            _alphaTween?.Kill();
            _alphaTween = _overlay.CreateTween();
            _alphaTween.FadeOut(_overlay, duration: AnimationDuration);
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
        MouseFilter = MouseFilterEnum.Ignore;
    }

}