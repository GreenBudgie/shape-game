using System;
using System.Collections.Generic;
using System.Linq;

public partial class InventoryModule : TextureButton
{
    
    private const float TargetPositionFollowSpeed = 10f;
    private const float CursorFollowSpeed = 30f;
    
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
    
    /// <summary>
    /// Emitted when the module is fully shown after inventory is opened
    /// </summary>
    [Signal]
    public delegate void ShowAnimationFinishedEventHandler();

    private ShaderMaterial _material = null!;
    private ModuleInfo? _moduleInfo;
    private HexCoordinates? _mousePivot;
    private Dictionary<HexCoordinates, HexData> _hexes = [];
    
    // Nullable since it uses a deferred call 
    private Glow? _glow;
    
    private Vector2 _targetPosition = Vector2.Zero;
    private float _targetRotation;

    private TextureRect _moduleTexture = null!;

    public Module Module { get; private set; } = null!;
    public Dictionary<HexCoordinates, InventorySlot> Slots { get; private set; } = [];
    public Dictionary<HexCoordinates, InventoryModuleConnection> Connections { get; private set; } = [];
    public bool IsFollowingCursor => _mousePivot.HasValue;

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

        Callable.From(() =>
            _glow = Glow.AddGlow(this)
                .SetColor(Module.Color)
                .SetRadius(0)
                .SetStrength(1)
        ).CallDeferred();

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

    private const float HoverTweenDuration = 0.125f;

    private Tween? _appearTween;
    private Tween? _hoverTween;

    private void OnMouseEnter()
    {
        if (_mousePivot.HasValue || !InventoryManager.Instance.IsOpen)
        {
            return;
        }

        ShowModuleInfo();
        
        _appearTween?.Kill();
        FullyShow();
        
        _hoverTween?.Kill();
        _hoverTween = CreateTween().SetParallel().SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Quad);

        _hoverTween.TweenOffsetScale(this, 1.04f, HoverTweenDuration);

        if (_glow != null)
        {
            _hoverTween.TweenGlowRadius(_glow, 30, HoverTweenDuration);
        }
    }

    private void OnMouseExit()
    {
        HideModuleInfo();
        
        if (_mousePivot.HasValue || !InventoryManager.Instance.IsOpen)
        {
            return;
        }
        
        _hoverTween?.Kill();
        _hoverTween = CreateTween().SetParallel().SetEase(Tween.EaseType.In).SetTrans(Tween.TransitionType.Quad);

        _hoverTween.TweenOffsetScaleReset(this, HoverTweenDuration);

        if (_glow != null)
        {
            _hoverTween.TweenGlowRadius(_glow, 0, HoverTweenDuration);
        }
    }

    public override void _ExitTree()
    {
        HideModuleInfo();
    }

    public override void _Process(double delta)
    {
        RotateToTarget(delta);
        MoveToTarget(delta);
        
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
            _targetPosition = GetSlotBasedPosition(Slots);
        }
    }

    public List<InventoryModuleConnection> GetDirectIncomingConnections()
    {
        List<InventoryModuleConnection> incomingConnections = [];
        foreach (var slot in Slots.Values)
        {
            incomingConnections.AddRange(slot.Connections);
        }

        return incomingConnections;
    }
    
    /// <summary>
    /// Returns all connections to this module, tracing them back to the last connection.
    /// Specific order of connections is not guaranteed
    /// </summary>
    public List<InventoryModuleConnection> GetIncomingConnectionsChain()
    {
        var directConnections = GetDirectIncomingConnections();
        if (directConnections.Count == 0)
        {
            return [];
        }

        return directConnections
            .SelectMany(directConnection => directConnection.Module.GetIncomingConnectionsChain())
            .Concat(directConnections)
            .Distinct()
            .ToList();
    }
    
    /// <summary>
    /// Returns all modules connected to the current one, tracing them back to the last module.
    /// Specific order of connections is not guaranteed
    /// </summary>
    public List<InventoryModule> GetAllIncomingConnectedModules()
    {
        return GetIncomingConnectionsChain().Select(x => x.Module).Distinct().ToList();
    }
    
    private void RotateToTarget(double delta)
    {
        if (IsEqualApprox(_targetRotation, Rotation))
        {
            return;
        }

        var followSpeed = 30f;
        var remainingAngle = Abs(_targetRotation - Rotation);
        var angle = MoveToward(
            Rotation,
            _targetRotation,
            followSpeed * (float)delta * remainingAngle
        );

        Rotation = angle;
    }

    private void MoveToTarget(double delta)
    {
        if (_targetPosition.IsEqualApprox(Position))
        {
            return;
        }
        
        var followSpeed = IsFollowingCursor ? CursorFollowSpeed : TargetPositionFollowSpeed;
        var distanceToX = Abs(Position.X - _targetPosition.X);
        var distanceToY = Abs(Position.Y - _targetPosition.Y);
        var x = MoveToward(
            Position.X,
            _targetPosition.X,
            followSpeed * (float)delta * distanceToX
        );
        var y = MoveToward(+
            Position.Y,
            _targetPosition.Y,
            followSpeed * (float)delta * distanceToY
        );

        Position = new Vector2(x, y);
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
 
        Dictionary<HexCoordinates, InventorySlot> hoveredSlots = [];
        Dictionary<HexCoordinates, ConnectionData> hoveredConnectorSlots = [];
        foreach (var slot in InventoryManager.Instance.GetAllSlots())
        {
            var isSlotAvailable = !slot.IsDisabled() && (slot.Module == null || slot.Module == this);
            foreach (var hex in _hexes)
            {
                var mouseHexPosition = mousePosition - pivotOffset + hex.Value.RealPosition;
                if (slot.GetCenterPosition().DistanceSquaredTo(mouseHexPosition) < InventorySlot.InradiusSq)
                {
                    if (hex.Value.Connection == null)
                    {
                        if (isSlotAvailable)
                        {
                            hoveredSlots.Add(hex.Key, slot);
                        }
                    }
                    else
                    {
                        hoveredConnectorSlots.Add(hex.Key, new ConnectionData(slot, hex.Value.Connection));
                    }
                }
            }
        }

        var allSlotsHovered = hoveredSlots.Count == Module.Shape.Hexes.Count;
        if (!IsAllSlotsAvailable(hoveredSlots.Values) || !allSlotsHovered)
        {
            _targetPosition = mousePosition - pivotOffset;
            return;
        }

        _targetPosition = GetSlotBasedPosition(hoveredSlots);

        if (Input.IsActionJustPressed("inventory_left_click"))
        {
            ForceInsert(hoveredSlots, hoveredConnectorSlots);
            StopFollowingCursor();
        }
    }

    private bool IsAllSlotsAvailable(IEnumerable<InventorySlot> slots)
    {
        var inventorySlots = slots.ToList();
        if (inventorySlots.Count == 0)
        {
            return false;
        }
        
        foreach (var slot in inventorySlots)
        {
            if (slot.IsDisabled())
            {
                return false;
            }

            if (slot.Module != null && slot.Module != this)
            {
                return false;
            }
        }

        return true;
    }

    private void Rotate(int direction)
    {
        if (!_mousePivot.HasValue)
        {
            GD.PrintErr("Cannot rotate inventory module while having no pivot");
            return;
        }

        _targetRotation += HexCoordinates.RotationStep * direction;
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

        Dictionary<HexCoordinates, InventorySlot> moduleSlots = [];
        Dictionary<HexCoordinates, ConnectionData> connectorSlots = [];
        foreach (var hex in _hexes)
        {
            var slot = inventory.TryGetSlot(centerSlot.Coordinates + hex.Key);
            if (slot == null || (slot.Module != null && slot.Module != this) || slot.IsDisabled())
            {
                if (hex.Value.Connection != null)
                {
                    connectorSlots.Add(hex.Key, new ConnectionData(slot, hex.Value.Connection));
                    // It is fine for connection to not have an attached slot, or to attach to a disabled slot,
                    // so just continue
                    continue;   
                }
                
                return false;
            }

            if (hex.Value.Connection == null)
            {
                moduleSlots.Add(hex.Key, slot);
            }
            else
            {
                connectorSlots.Add(hex.Key, new ConnectionData(slot, hex.Value.Connection));
            }
        }

        ForceInsert(moduleSlots, connectorSlots);
        return true;
    }

    private void ForceInsert(
        Dictionary<HexCoordinates, InventorySlot> moduleSlots,
        Dictionary<HexCoordinates, ConnectionData> connections
    )
    {
        // Clear previously occupying slots
        if (Slots.Count != 0)
        {
            foreach (var slot in Slots)
            {
                slot.Value.Module = null;
            }
        }

        // Clear previous connections
        if (Connections.Count != 0)
        {
            foreach (var connection in Connections)
            {
                connection.Value.Slot?.Connections.Remove(connection.Value);
                connection.Value.Slot = null;
            }
        }

        Slots = moduleSlots;
        Connections = connections.ToDictionary(x => x.Key, x => x.Value.Connection);
        
        foreach (var slot in Slots)
        {
            slot.Value.Module = this;
        }
        
        foreach (var connection in connections)
        {
            connection.Value.Slot?.Connections.Add(connection.Value.Connection);
            connection.Value.Connection.Slot = connection.Value.Slot;
        }
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
    
    public void ShowModule()
    {
        _appearTween?.Kill();
        _appearTween = CreateTween().SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.Out).SetParallel();
        
        _appearTween.TweenOffsetScaleReset(this, InventoryManager.ModuleAnimationDuration)
            .SetDelay(InventoryManager.ModuleShowDelay);
        _appearTween.TweenOffsetRotationReset(this, InventoryManager.ModuleAnimationDuration)
            .SetDelay(InventoryManager.ModuleShowDelay);
        _appearTween.FadeIn(this, InventoryManager.ModuleAnimationDuration)
            .SetDelay(InventoryManager.ModuleShowDelay);

        if (_glow != null)
        {
            _appearTween.FadeIn(_glow, InventoryManager.ModuleAnimationDuration)
                .SetDelay(InventoryManager.ModuleShowDelay);
        }

        _appearTween.Finished += FullyShow;
    }
    
    public void HideModule()
    {
        _hoverTween?.Kill();
        _appearTween?.Kill();
        _appearTween = CreateTween().SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.In).SetParallel();

        _appearTween.TweenOffsetScale(this, RandomUtils.DeltaRange(0.7f, 0.1f), InventoryManager.ModuleAnimationDuration);
        _appearTween.TweenOffsetRotation(this, RandomUtils.DeltaRange(0, Pi / 8), InventoryManager.ModuleAnimationDuration);
        _appearTween.FadeOut(this, InventoryManager.ModuleAnimationDuration);

        if (_glow != null)
        {
            _appearTween.TweenGlowRadius(_glow, 0, InventoryManager.ModuleAnimationDuration);
            _appearTween.FadeOut(_glow, InventoryManager.ModuleAnimationDuration / 2f);
        }
    }

    private void FullyShow()
    {
        Modulate = Modulate.AsOpaque();
        OffsetTransformRotation = 0;

        if (_glow != null)
        {
            _glow.Modulate = _glow.Modulate.AsOpaque();
        }

        EmitSignalShowAnimationFinished();
    }

    private readonly record struct HexData(Vector2 RealPosition, InventoryModuleConnection? Connection);
    private readonly record struct ConnectionData(InventorySlot? Slot, InventoryModuleConnection Connection);
}