using System;
using System.Collections.Generic;
using System.Linq;

public partial class InventoryModule : TextureButton
{
    private static readonly PackedScene Scene = GD.Load<PackedScene>("uid://csoad8g8f13qn");

    /// <summary>
    /// Emitted whenever this module is rotated
    /// </summary>
    [Signal]
    public delegate void RotatedEventHandler(int direction);
    
    /// <summary>
    /// Emitted whenever this module is inserted into inventory
    /// </summary>
    [Signal]
    public delegate void InsertedEventHandler();
    
    /// <summary>
    /// Emitted whenever this module is taken out from the inventory
    /// </summary>
    [Signal]
    public delegate void TakenOutEventHandler();

    private ShaderMaterial _material = null!;
    private ModuleInfo? _moduleInfo;
    private HexCoordinates? _mousePivot;
    private Dictionary<HexCoordinates, HexData> _hexes = [];

    private TextureRect _moduleTexture = null!;

    public Module Module { get; private set; } = null!;
    public Dictionary<HexCoordinates, InventorySlot> Slots { get; private set; } = [];

    public static InventoryModule Create(Module module)
    {
        var inventoryModule = Scene.Instantiate<InventoryModule>();
        inventoryModule.Module = module;
        return inventoryModule;
    }

    public override void _Ready()
    {
        _moduleTexture = GetNode<TextureRect>("ModuleTexture");

        SelfModulate = Module.Color;
        TextureNormal = Module.Shape.Texture;
        _moduleTexture.Texture = Module.Texture;
        _material = (ShaderMaterial)Material;
        TextureClickMask = Module.Shape.Bitmap;

        foreach (var moduleHex in Module.Shape.PixelHexPositions)
        {
            _hexes.Add(moduleHex.Key, new HexData(moduleHex.Value, null));
        }

        foreach (var connectionHex in Module.Connections)
        {
            var source = connectionHex.FindFirstNeighbor(GetModuleHexes());
            if (!source.HasValue)
            {
                throw new ArgumentException(
                    $"Module has an incorrect connection configuration: {connectionHex} does not have a neighbor");
            }

            var connection = InventoryModuleConnection.Create(this);

            _hexes.Add(
                connectionHex,
                new HexData(ModuleShape.GetVisualHexPosition(connectionHex), connection)
            );

            var sourcePosition = _hexes[source.Value].RealPosition;
            var targetPosition = _hexes[connectionHex].RealPosition;
            connection.Position = (sourcePosition + targetPosition) / 2;
            
            AddChild(connection);
        }

        MouseEntered += OnMouseEnter;
        MouseExited += OnMouseExit;
        
        InventoryManager.Instance.Connect(
            InventoryManager.SignalName.InventoryOpened,
            Callable.From(OnInventoryOpened)
        );
        InventoryManager.Instance.Connect(
            InventoryManager.SignalName.InventoryClosed,
            Callable.From(OnInventoryClosed)
        );
        
    }
    
    private void OnInventoryOpened()
    {
        ShowModule();
    }

    private void OnInventoryClosed()
    {
        HideModule();
        StopFollowingCursor();
    }

    private IEnumerable<HexCoordinates> GetModuleHexes()
    {
        return _hexes.Where(x => x.Value.Connection == null).Select(x => x.Key);
    }

    private void OnMouseEnter()
    {
        if (_mousePivot.HasValue)
        {
            return;
        }

        ShowModuleInfo();
    }

    private void OnMouseExit()
    {
        if (_mousePivot.HasValue)
        {
            return;
        }

        HideModuleInfo();
    }

    public override void _ExitTree()
    {
        HideModuleInfo();
    }

    public override void _Process(double delta)
    {
        if (_mousePivot.HasValue)
        {
            FollowCursor();
            return;
        }

        if (IsHovered() && Input.IsActionJustPressed("inventory_left_click"))
        {
            StartFollowingCursor();
            return;
        }

        if (Slots.Count > 0)
        {
            Position = GetSlotBasedPosition(Slots);
        }
    }

    private void FollowCursor()
    {
        if (!_mousePivot.HasValue)
        {
            return;
        }

        if (Input.IsActionJustPressed("ui_rotate_clockwise"))
        {
            Rotate(1);
        }

        if (Input.IsActionJustPressed("ui_rotate_counter_clockwise"))
        {
            Rotate(-1);
        }

        var mousePosition = MouseInputManager.Instance.GetGlobalMousePosition();
        var pivotOffset = _hexes[_mousePivot.Value].RealPosition;
        var mouseModuleHexPositions = _hexes
            .Where(x => x.Value.Connection == null)
            .ToDictionary(x => x.Key, x => mousePosition - pivotOffset + x.Value.RealPosition);

        Dictionary<HexCoordinates, InventorySlot> hoveredSlots = [];
        foreach (var slot in InventoryManager.Instance.GetActiveSlots())
        {
            foreach (var hexPosition in mouseModuleHexPositions)
            {
                if (slot.GetCenterPosition().DistanceSquaredTo(hexPosition.Value) < InventorySlot.InradiusSq)
                {
                    hoveredSlots.Add(hexPosition.Key, slot);
                }
            }
        }

        var allSlotsHovered = hoveredSlots.Count == Module.Shape.Hexes.Count;
        if (hoveredSlots.Count == 0)
        {
            Position = mousePosition - pivotOffset;
            return;
        }

        Position = GetSlotBasedPosition(hoveredSlots);

        if (allSlotsHovered && Input.IsActionJustPressed("inventory_left_click"))
        {
            Slots = hoveredSlots;
            StopFollowingCursor();
        }
    }

    private void Rotate(int direction)
    {
        if (!_mousePivot.HasValue)
        {
            GD.PrintErr("Cannot rotate inventory module while having no pivot");
            return;
        }

        Rotation += HexCoordinates.RotationStep * direction;
        _hexes = _hexes.ToDictionary(
            x => x.Key.RotatedClockwise(_mousePivot.Value, direction),
            x => x.Value with { RealPosition = x.Value.RealPosition.Rotated(HexCoordinates.RotationStep * direction) }
        );
        
        EmitSignalRotated(direction);
    }

    private Vector2 GetSlotBasedPosition(Dictionary<HexCoordinates, InventorySlot> slots)
    {
        var firstSlot = slots.First();
        var slotPosition = firstSlot.Value.GetCenterPosition();
        var shapeHexPosition = _hexes[firstSlot.Key].RealPosition;
        return slotPosition - shapeHexPosition;
    }

    public bool TryInsert(InventorySlot centerSlot)
    {
        var inventory = centerSlot.Inventory;

        Dictionary<HexCoordinates, InventorySlot> slots = [];
        foreach (var hex in Module.Shape.Hexes)
        {
            var slot = inventory.TryGetSlot(centerSlot.Coordinates + hex);
            if (slot == null || slot.HasModule() || slot.IsDisabled())
            {
                return false;
            }

            slots.Add(hex, slot);
        }

        Slots = slots;
        return true;
    }

    private void StartFollowingCursor()
    {
        if (_mousePivot.HasValue)
        {
            return;
        }

        HideModuleInfo();

        var mousePosition = MouseInputManager.Instance.GetCachedGlobalMousePosition();
        var closestHex = _hexes
            .Where(x => x.Value.Connection == null)
            .MinBy(entry => (entry.Value.RealPosition + Position).DistanceSquaredTo(mousePosition));
        _mousePivot = closestHex.Key;

        ZIndex += 1;
        
        EmitSignalTakenOut();
    }

    private void StopFollowingCursor()
    {
        if (!_mousePivot.HasValue)
        {
            return;
        }

        if (IsHovered())
        {
            ShowModuleInfo();
        }

        _mousePivot = null;
        ZIndex -= 1;
        
        EmitSignalInserted();
    }

    private void ShowModuleInfo()
    {
        if (_mousePivot.HasValue)
        {
            return;
        }

        _moduleInfo = ModuleInfo.Create(Module);
        InventoryManager.Instance.AddChild(_moduleInfo);
    }

    private void HideModuleInfo()
    {
        _moduleInfo?.Remove();
        _moduleInfo = null;
    }
    
    private Tween? _tween;
    
    public void ShowModule()
    {
        _tween?.Kill();
        _tween = CreateTween().SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.Out).SetParallel();
        
        _tween.TweenOffsetScaleReset(this, InventoryManager.ModuleAnimationDuration)
            .SetDelay(InventoryManager.ModuleShowDelay);
        _tween.TweenOffsetRotationReset(this, InventoryManager.ModuleAnimationDuration)
            .SetDelay(InventoryManager.ModuleShowDelay);
        _tween.FadeIn(this, InventoryManager.ModuleAnimationDuration)
            .SetDelay(InventoryManager.ModuleShowDelay);
    }
    
    public void HideModule()
    {
        _tween?.Kill();
        _tween = CreateTween().SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.In).SetParallel();

        _tween.TweenOffsetScale(this, RandomUtils.DeltaRange(0.7f, 0.1f), InventoryManager.ModuleAnimationDuration);
        _tween.TweenOffsetRotation(this, RandomUtils.DeltaRange(0, Pi / 8), InventoryManager.ModuleAnimationDuration);
        _tween.FadeOut(this, InventoryManager.ModuleAnimationDuration);
    }

    private readonly record struct HexData(Vector2 RealPosition, InventoryModuleConnection? Connection);
}