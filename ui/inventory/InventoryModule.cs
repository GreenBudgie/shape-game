using System.Collections.Generic;
using System.Linq;

public partial class InventoryModule : TextureButton
{
    private static readonly PackedScene Scene = GD.Load<PackedScene>("uid://csoad8g8f13qn");

    private ShaderMaterial _material = null!;
    private ModuleInfo? _moduleInfo;
    private int _rotation;
    private HexCoordinates? _mousePivot;
    private Dictionary<HexCoordinates, Vector2> _remappedCoordinates = [];

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
        _remappedCoordinates = Module.Shape.PixelHexPositions.ToDictionary();

        TextureClickMask = Module.Shape.Bitmap;

        MouseEntered += OnMouseEnter;
        MouseExited += OnMouseExit;
        InventoryManager.Instance.Connect(
            InventoryManager.SignalName.InventoryClosed,
            Callable.From(StopFollowingCursor)
        );
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
        foreach (var keyValuePair in _remappedCoordinates)
        {
            DebugDraw.DrawPoint(Position + keyValuePair.Value, 1);
        }

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

        Rotation = HexCoordinates.GetRotationAngleByStep(_rotation);

        var mousePosition = MouseInputManager.Instance.GetGlobalMousePosition();
        var pivotOffset = _remappedCoordinates[_mousePivot.Value];
        var mouseHexPositions = _remappedCoordinates.ToDictionary(
            x => x.Key,
            x => mousePosition - pivotOffset + x.Value
        );

        Dictionary<HexCoordinates, InventorySlot> hoveredSlots = [];
        foreach (var slot in InventoryManager.Instance.GetActiveSlots())
        {
            foreach (var hexPosition in mouseHexPositions)
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

    private void Rotate(int step)
    {
        if (!_mousePivot.HasValue)
        {
            GD.PrintErr("Cannot rotate inventory module while having no pivot");
            return;
        }

        _rotation = (_rotation + step) % HexCoordinates.HexEdges;
        _remappedCoordinates = _remappedCoordinates.ToDictionary(
            x => x.Key.RotatedClockwise(_mousePivot.Value, step),
            x => x.Value.Rotated(HexCoordinates.RotationStep * step)
        );
    }

    private Vector2 GetSlotBasedPosition(Dictionary<HexCoordinates, InventorySlot> slots)
    {
        var firstSlot = slots.First();
        var slotPosition = firstSlot.Value.GetCenterPosition();
        var shapeHexPosition = _remappedCoordinates[firstSlot.Key];
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
        var closestHex = _remappedCoordinates.MinBy(entry => (entry.Value + Position).DistanceSquaredTo(mousePosition));
        _mousePivot = closestHex.Key;

        ZIndex += 1;
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
}