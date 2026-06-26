using System.Collections.Generic;
using System.Linq;

public partial class InventoryModule : TextureButton
{
    private static readonly PackedScene Scene = GD.Load<PackedScene>("uid://csoad8g8f13qn");

    private bool _isFollowingCursor;
    private ShaderMaterial _material = null!;
    private ModuleInfo? _moduleInfo;
    private int _rotation;
    private HexCoordinates? _pivot;
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
        if (_isFollowingCursor)
        {
            return;
        }

        ShowModuleInfo();
    }

    private void OnMouseExit()
    {
        if (_isFollowingCursor)
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
        if (!_isFollowingCursor && IsHovered() && Input.IsActionJustPressed("inventory_left_click"))
        {
            StartFollowingCursor();
        }
        else if (_isFollowingCursor && _pivot.HasValue)
        {
            if (Input.IsActionJustPressed("ui_rotate_clockwise"))
            {
                _rotation = (_rotation + 1) % HexCoordinates.HexEdges;
                _remappedCoordinates = _remappedCoordinates.ToDictionary(
                    x => x.Key.RotatedClockwise(_pivot.Value),
                    x => x.Value.Rotated(HexCoordinates.RotationStep)
                );
            }

            if (Input.IsActionJustPressed("ui_rotate_counter_clockwise"))
            {
                _rotation = (_rotation - 1) % HexCoordinates.HexEdges;
                _remappedCoordinates = _remappedCoordinates.ToDictionary(
                    x => x.Key.RotatedCounterClockwise(_pivot.Value),
                    x => x.Value.Rotated(-HexCoordinates.RotationStep)
                );
            }

            Rotation = HexCoordinates.GetRotationAngleByStep(_rotation);

            var mousePosition = MouseInputManager.Instance.GetGlobalMousePosition();
            var pivotOffset = _remappedCoordinates[_pivot.Value];
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
            }
            else
            {
                Position = GetSlotBasedPosition(hoveredSlots);

                if (allSlotsHovered && Input.IsActionJustPressed("inventory_left_click"))
                {
                    Slots = hoveredSlots;
                    StopFollowingCursor();
                }
            }
        }
        else if (Slots.Count > 0)
        {
            Position = GetSlotBasedPosition(Slots);
        }
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

    public void StartFollowingCursor()
    {
        if (_isFollowingCursor)
        {
            return;
        }

        HideModuleInfo();
        _isFollowingCursor = true;

        var mousePosition = MouseInputManager.Instance.GetCachedGlobalMousePosition();
        var closestHex = _remappedCoordinates.MinBy(entry => (entry.Value + Position).DistanceSquaredTo(mousePosition));
        _pivot = closestHex.Key;

        ZIndex += 1;
    }

    public void StopFollowingCursor()
    {
        if (!_isFollowingCursor)
        {
            return;
        }

        if (IsHovered())
        {
            ShowModuleInfo();
        }

        _isFollowingCursor = false;
        _pivot = null;
        ZIndex -= 1;
    }

    private void ShowModuleInfo()
    {
        if (_isFollowingCursor)
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