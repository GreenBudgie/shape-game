using System;
using System.Collections.Generic;
using System.Linq;

public partial class InventoryManager : Control, IScreen
{

    public const float AnimationDuration = 0.15f;
    public const float SlotAnimationDuration = AnimationDuration;
    public const float SlotHideDelay = AnimationDuration / 2f;
    public const float ModuleShowDelay = AnimationDuration / 2f;
    public const float ModuleAnimationDuration = AnimationDuration;

    [Export] private AudioStream _openSound = null!;
    [Export] private AudioStream _closeSound = null!;
    
    [Signal]
    public delegate void InventoryOpenedEventHandler();

    [Signal]
    public delegate void InventoryClosedEventHandler();
    
    [Signal]
    public delegate void SlotsStateResetEventHandler();
    
    [Signal]
    public delegate void ModuleGrabbedEventHandler(InventoryModule module);
    
    [Signal]
    public delegate void ModuleInsertedEventHandler(InventoryModule module);
    
    [Signal]
    public delegate void ModuleDroppingEventHandler(InventoryModule module);

    public static InventoryManager Instance { get; private set; } = null!;

    public bool IsOpen { get; private set; } = true;
    public InventoryModule? DraggingModule { get; set; }

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
        
        ScreenManager.Instance.RegisterScreen(this);
    }

    public List<InventorySlot> GetAllSlots()
    {
        return _slots;
    }

    /// <summary>
    /// Returns enabled slots that have no inserted module
    /// </summary>
    public List<InventorySlot> GetFreeSlots()
    {
        return _slots.Where(slot => !slot.IsDisabled() && slot.Module == null).ToList();
    }

    private void PostSetup()
    {
        AddModule(ModuleRegistry.BoltModule, LeftBlasterInventory);
        AddModule(ModuleRegistry.ExtraFireRateModule, LeftBlasterInventory);
        AddModule(ModuleRegistry.ExtraFireRateModule, RightBlasterInventory);
        AddModule(ModuleRegistry.ExtraFireRateModule, RightBlasterInventory);
        AddModule(ModuleRegistry.ExtraFireRateModule, RightBlasterInventory);
        AddModule(ModuleRegistry.MineModule, RightBlasterInventory);

        Close(playSound: false);
        Visible = false;
    }
    
    /// <summary>
    /// Adds module either directly to the inventory, if it has space, or opens the inventory while holding the module
    /// at cursor
    /// </summary>
    public void AddModule(Module module)
    {
        var result = TryAddModule(module);
        if (result.Success)
        {
            return;
        }
        
        Open();
        result.InventoryModule.GlobalPosition = MouseInputManager.Instance.GetCachedGlobalMousePosition();
        result.InventoryModule.StartFollowingCursor();
    }

    public InsertResult TryAddModule(Module module)
    {
        var inventoryModule = InventoryModule.Create(module);
        AddChild(inventoryModule);
        
        foreach (var inventory in _inventories)
        {
            var inserted = inventory.TryInsertModule(inventoryModule);
            if (inserted)
            {
                return new InsertResult(inventoryModule, true);
            }
        }

        return new InsertResult(inventoryModule, false);
    }

    public readonly record struct InsertResult(InventoryModule InventoryModule, bool Success);

    private void AddModule(Module module, ModuleInventory inventory)
    {
        var inventoryModule = InventoryModule.Create(module);
        AddChild(inventoryModule);
        var inserted = inventory.TryInsertModule(inventoryModule);
        if (!inserted)
        {
            throw new ArgumentException($"No space for module {module.Name}");
        }
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (!@event.IsActionPressed("inventory"))
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

    public void Open()
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

        SoundManager.Instance.PlaySound(_openSound).RandomizePitchOffset(0.05f);
        EmitSignalInventoryOpened();
    }

    private void Close(bool playSound = true)
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

        if (playSound)
        {
            SoundManager.Instance.PlaySound(_closeSound).RandomizePitchOffset(0.05f);
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