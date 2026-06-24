using System.Collections.Generic;
using System.Linq;

public partial class InventoryModule : TextureButton
{

    private const float StretchAmountPerPixel = 0.02f;
    private const float TargetPositionFollowSpeed = 10f;
    private const float CursorFollowSpeed = 30f;

    private static readonly StringName StretchAmount = "stretch_amount";
    private static readonly StringName StretchDirection = "stretch_direction";

    private static readonly PackedScene Scene = GD.Load<PackedScene>("uid://csoad8g8f13qn");

    private bool _isFollowingCursor;
    private ShaderMaterial _material = null!;
    private Vector2 _targetPosition;
    private ModuleInfo? _moduleInfo;
    private int _rotation;
    private HexCoordinates? _pivot;
    
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
        if (_pivot.HasValue)
        {
            DebugDraw.DrawPoint(_pivot.Value.ToPixel() - Module.Shape.PixelCenter + this.GetCenterGlobalPosition());
        }

        if (!_isFollowingCursor && IsHovered() && Input.IsActionJustPressed("inventory_left_click"))
        {
            StartFollowingCursor();
        } else if (_isFollowingCursor)
        {
            if (Input.IsActionJustPressed("ui_rotate_clockwise"))
            {
                _rotation = (_rotation + 1) % HexCoordinates.HexEdges;
            }
            
            if (Input.IsActionJustPressed("ui_rotate_counter_clockwise"))
            {
                _rotation = (_rotation - 1) % HexCoordinates.HexEdges;
            }

            var mousePosition = MouseInputManager.Instance.GetGlobalMousePosition();
            var pivotOffset = _pivot.Value.ToPixel() - Module.Shape.PixelCenter;
            var mouseHexPositions = Module.Shape.PixelHexPositions.ToDictionary(x => x.Key, x => mousePosition + x.Value - pivotOffset);

            Dictionary<HexCoordinates, InventorySlot> hoveredSlots = []; 
            foreach (var slot in InventoryManager.Instance.GetActiveSlots())
            {
                foreach (var hexPosition in mouseHexPositions)
                {
                    if (slot.GetCenterGlobalPosition().DistanceSquaredTo(hexPosition.Value) < InventorySlot.InradiusSq)
                    {
                        hoveredSlots.Add(hexPosition.Key, slot);
                    }
                }
            }
            
            var allSlotsHovered = hoveredSlots.Count == Module.Shape.Hexes.Count; 
            if (hoveredSlots.Count == 0)
            {
                _targetPosition = mousePosition - pivotOffset;
            }
            else
            {
                var firstHoveredSlot = hoveredSlots.First();
                var slotPosition = firstHoveredSlot.Value.GetCenterGlobalPosition();
                var shapeHexPosition = Module.Shape.PixelHexPositions[firstHoveredSlot.Key];
                _targetPosition = slotPosition - shapeHexPosition;
                
                if (allSlotsHovered && Input.IsActionJustPressed("inventory_left_click"))
                {
                    Slots = hoveredSlots;
                    StopFollowingCursor();
                }
            }
        }
        else if (Slots.Count > 0)
        {
            var firstHoveredSlot = Slots.First();
            var slotPosition = firstHoveredSlot.Value.GetCenterGlobalPosition();
            var shapeHexPosition = Module.Shape.PixelHexPositions[firstHoveredSlot.Key];
            _targetPosition = slotPosition - shapeHexPosition;
        }

        MoveToTargetPosition(delta);
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
        var closestHex = Module.Shape.PixelHexPositions
            .MinBy(entry => (entry.Value + this.GetCenterGlobalPosition()).DistanceSquaredTo(mousePosition));
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

    private void MoveToTargetPosition(double delta)
    {
        var position = this.GetCenterGlobalPosition();
        if (position.IsEqualApprox(_targetPosition))
        {
            SetStretchAmount(0);
            return;
        }

        SetStretchDirection(position.DirectionTo(_targetPosition).Rotated(Pi / 2));

        var followSpeed = _isFollowingCursor ? CursorFollowSpeed : TargetPositionFollowSpeed;
        var distanceToX = Abs(position.X - _targetPosition.X);
        var distanceToY = Abs(position.Y - _targetPosition.Y);
        var x = MoveToward(
            position.X,
            _targetPosition.X,
            followSpeed * (float)delta * distanceToX
        );
        var y = MoveToward(
            position.Y,
            _targetPosition.Y,
            followSpeed * (float)delta * distanceToY
        );
        this.SetCenterGlobalPosition(new Vector2(x, y));

        var positionDelta = this.GetCenterGlobalPosition().DistanceTo(position);
        var stretchAmount = Clamp(StretchAmountPerPixel * positionDelta, 0, 2);
        SetStretchAmount(stretchAmount);
    }

    private void SetStretchAmount(float stretchAmount)
    {
        _material.SetShaderParameter(StretchAmount, stretchAmount);
    }

    private float GetStretchAmount()
    {
        return (float)_material.GetShaderParameter(StretchAmount);
    }

    private void SetStretchDirection(Vector2 stretchDirection)
    {
        _material.SetShaderParameter(StretchDirection, stretchDirection);
    }

    private Vector2 GetStretchDirection()
    {
        return (Vector2)_material.GetShaderParameter(StretchDirection);
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