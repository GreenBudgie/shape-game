using System;
using System.Collections.Generic;
using System.Linq;

public partial class InventoryModule : TextureButton
{
    
    private const float TargetPositionFollowSpeed = 10f;
    private const float CursorFollowSpeed = 30f;
    
    private static readonly PackedScene Scene = GD.Load<PackedScene>("uid://csoad8g8f13qn");

    [Export] private AudioStream _hoverSound = null!;
    [Export] private AudioStream _grabSound = null!;
    [Export] private AudioStream _insertSound = null!;
    [Export] private AudioStream _rotateSound = null!;
    [Export] private AudioStream _slotSnapSound = null!;
    [Export] private AudioStream _invalidConnectionSound = null!;
    
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
    
    /// <summary>
    /// Emitted when module dropping animation has started
    /// </summary>
    [Signal]
    public delegate void DroppingEventHandler();

    private ShaderMaterial _material = null!;
    private ModuleInfo? _moduleInfo;
    private HexCoordinates? _mousePivot;
    private Dictionary<HexCoordinates, HexData> _hexes = [];
    private bool _isFirstInsert = true;
    private bool _isFirstFrame = true;
    
    // Nullable since it uses a deferred call 
    private Glow? _glow;
    
    private Vector2? _targetPosition;
    private float _targetRotation;

    private TextureRect _moduleTexture = null!;
    private TextureRect _outline = null!;

    public Module Module { get; private set; } = null!;
    public Dictionary<HexCoordinates, InventorySlot> Slots { get; private set; } = [];
    public Dictionary<HexCoordinates, InventoryModuleConnection> Connections { get; private set; } = [];
    public bool IsFollowingCursor => _mousePivot.HasValue;
    
    private const float AnimationTweenDuration = 0.125f;

    private Tween? _appearTween;
    private Tween? _animationTween;

    public static InventoryModule Create(Module module)
    {
        var inventoryModule = Scene.Instantiate<InventoryModule>();
        inventoryModule.Module = module;
        return inventoryModule;
    }

    public override void _Ready()
    {
        _moduleTexture = GetNode<TextureRect>("ModuleTexture");
        _outline = GetNode<TextureRect>("Outline");

        _outline.Texture = Module.Shape.OutlineTexture;
        _outline.Modulate = Module.Color;

        SelfModulate = ColorScheme.DarkOrange;
        TextureNormal = Module.Shape.FillTexture;
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

        foreach (var connectionHex in Module.OutgoingConnections)
        {
            AddConnection(connectionHex, ConnectionType.Outgoing);
        }
        
        foreach (var connectionHex in Module.IncomingConnections)
        {
            AddConnection(connectionHex, ConnectionType.Incoming);
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

        DropArea.Instance.Connect(DropArea.SignalName.Hovered, Callable.From(OnDropAreaHovered));
        DropArea.Instance.Connect(DropArea.SignalName.Unhovered, Callable.From(OnDropAreaUnhovered));
    }

    private void AddConnection(HexCoordinates connectionHex, ConnectionType type)
    {
        var source = connectionHex.FindFirstNeighbor(GetModuleHexes());
        if (!source.HasValue)
        {
            throw new ArgumentException(
                $"Module {Module.Name} has an incorrect connection configuration: {connectionHex} does not have a neighbor");
        }

        var connection = InventoryModuleConnection.Create(this, type);

        _hexes.Add(
            connectionHex,
            new HexData(ModuleShape.GetVisualHexPosition(connectionHex), connection)
        );

        var sourcePosition = _hexes[source.Value].RealPosition;
        var targetPosition = _hexes[connectionHex].RealPosition;
        connection.Position = (sourcePosition + targetPosition) / 2;
            
        AddChild(connection);
    }

    private void BeforeRemove()
    {
        HideModuleInfo();
        InventoryManager.Instance.DraggingModule = null;
        ResetHoveredSlots();
        
        // Clear occupying slots
        if (Slots.Count != 0)
        {
            foreach (var slot in Slots)
            {
                slot.Value.Module = null;
            }
        }

        // Clear connections
        if (Connections.Count != 0)
        {
            foreach (var connection in Connections)
            {
                connection.Value.Slot?.Connections.Remove(connection.Value);
                connection.Value.Slot = null;
            }
        }
    }

    private void OnDropAreaHovered()
    {
        if (_isDropping || !IsFollowingCursor)
        {
            return;
        }
        
        _animationTween?.Kill();
        _animationTween = CreateTween()
            .SetTrans(Tween.TransitionType.Quad)
            .SetEase(Tween.EaseType.Out)
            .SetParallel();

        _animationTween.TweenOffsetScale(this, 0.9f, AnimationTweenDuration);
        _animationTween.TweenOffsetRotation(this, RandomUtils.RandomSign(0.1f), AnimationTweenDuration);
    }
    
    private void OnDropAreaUnhovered()
    {
        if (_isDropping || !IsFollowingCursor)
        {
            return;
        }
        
        _animationTween?.Kill();
        _animationTween = CreateTween().SetParallel().SetEase(Tween.EaseType.In).SetTrans(Tween.TransitionType.Quad);

        _animationTween.TweenOffsetScaleReset(this, AnimationTweenDuration);
        _animationTween.TweenOffsetRotationReset(this, AnimationTweenDuration);
    }

    private bool _isDropping;

    public void Drop()
    {
        if (_isDropping)
        {
            return;
        }

        _isDropping = true;
        
        BeforeRemove();
        
        var worldModule = WorldModule.Create(Module);
        WorldModuleManager.Instance.SpawnModule(worldModule);

        const float duration = 0.2f;
        
        _animationTween?.Kill();
        _animationTween = CreateTween().SetParallel().SetEase(Tween.EaseType.In).SetTrans(Tween.TransitionType.Quad);

        _animationTween.TweenOffsetScale(this, 0.5f, duration);
        _animationTween.TweenOffsetPosition(this, new Vector2(0, 80), duration);
        _animationTween.TweenOffsetRotation(this, RandomUtils.RandomSign(0.3f), duration);
        _animationTween.FadeOut(this, duration);
        
        if (_glow != null)
        {
            _animationTween.TweenGlowColor(_glow, _glow.Color.AsTransparent(), duration / 3);
            _animationTween.TweenGlowRadius(_glow, 0, duration / 3);
        }

        _animationTween.Finished += QueueFree;
        
        EmitSignalDropping();
        InventoryManager.Instance.EmitSignal(InventoryManager.SignalName.ModuleDropping, this);
    }

    private void OnInventoryOpened()
    {
        if (_isDropping)
        {
            return;
        }
        
        ShowModule();
    }

    private void OnInventoryClosed()
    {
        if (_isDropping)
        {
            return;
        }
        
        HideModule();
        StopFollowingCursor();
    }

    private IEnumerable<HexCoordinates> GetModuleHexes()
    {
        return _hexes.Where(x => x.Value.Connection == null).Select(x => x.Key);
    }

    private void OnMouseEnter()
    {
        if (_isDropping)
        {
            return;
        }
        
        if (_mousePivot.HasValue || !InventoryManager.Instance.IsOpen || InventoryManager.Instance.DraggingModule != null)
        {
            return;
        }

        SoundManager.Instance.PlaySound(_hoverSound).RandomizePitchOffset();
        ShowModuleInfo();
        
        _appearTween?.Kill();
        FullyShow();
        
        _animationTween?.Kill();
        _animationTween = CreateTween().SetParallel().SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Quad);

        _animationTween.TweenOffsetScale(this, 1.04f, AnimationTweenDuration);

        if (_glow != null)
        {
            _animationTween.TweenGlowRadius(_glow, 30, AnimationTweenDuration);
        }
    }

    private void OnMouseExit()
    {
        if (_isDropping)
        {
            return;
        }
        
        HideModuleInfo();
        
        if (_mousePivot.HasValue || !InventoryManager.Instance.IsOpen)
        {
            return;
        }
        
        _animationTween?.Kill();
        _animationTween = CreateTween().SetParallel().SetEase(Tween.EaseType.In).SetTrans(Tween.TransitionType.Quad);

        _animationTween.TweenOffsetScaleReset(this, AnimationTweenDuration);
        _animationTween.TweenOffsetRotationReset(this, AnimationTweenDuration);

        if (_glow != null)
        {
            _animationTween.TweenGlowRadius(_glow, 0, AnimationTweenDuration);
        }
    }

    public override void _ExitTree()
    {
        HideModuleInfo();
    }

    public override void _Process(double delta)
    {
        if (_isDropping)
        {
            return;
        }
        
        RotateToTarget(delta);
        MoveToTarget(delta);
        
        if (_mousePivot.HasValue)
        {
            FollowCursor();
            return;
        }
        
        if (IsHovered() && Input.IsActionJustPressed("drop_module"))
        {
            Drop();
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

    // Outgoing connections logic
    
    private IEnumerable<InventorySlot> GetOutgoingConnectionsSlots()
    {
        return Connections.Values
            .Where(connection => connection.Type == ConnectionType.Outgoing)
            .Select(connection => connection.Slot)
            .OfType<InventorySlot>();
    }
    
    private HashSet<InventoryModule> GetDirectOutgoingConnectedModules(
        IEnumerable<InventorySlot> slots,
        IEnumerable<InventorySlot> outgoingConnectionSlots,
        InventoryModule ignoredModule
    )
    {
        return GetOwnDirectOutgoingConnectedModules(outgoingConnectionSlots, ignoredModule)
            .Concat(GetExternalDirectOutgoingConnectedModules(slots, ignoredModule))
            .ToHashSet();
    }

    private IEnumerable<InventoryModule> GetOwnDirectOutgoingConnectedModules(
        IEnumerable<InventorySlot> outgoingConnectionSlots,
        InventoryModule ignoredModule
    )
    {
        return outgoingConnectionSlots.Select(slot => slot.Module)
            .OfType<InventoryModule>()
            .Where(module => module != this && module != ignoredModule);
    }

    private IEnumerable<InventoryModule> GetExternalDirectOutgoingConnectedModules(
        IEnumerable<InventorySlot> slots,
        InventoryModule ignoredModule
    )
    {
        return slots
            .SelectMany(slot => slot.Connections)
            .Where(connection => connection.Type == ConnectionType.Incoming)
            .Select(connection => connection.Module)
            .Where(module => module != this && module != ignoredModule);
    }
    
    public HashSet<InventoryModule> GetAllOutgoingConnectedModules()
    {
        return GetAllOutgoingConnectedModules(
            Slots.Values,
            GetOutgoingConnectionsSlots(),
            this,
            [],
            stopAtInterruptingModules: true
        );
    }

    public HashSet<InventoryModule> GetAllOutgoingConnectedModules(
        InventoryModule ignoredModule,
        bool stopAtInterruptingModules
    )
    {
        return GetAllOutgoingConnectedModules(
            Slots.Values,
            GetOutgoingConnectionsSlots(),
            ignoredModule,
            [],
            stopAtInterruptingModules
        );
    }

    private HashSet<InventoryModule> GetAllOutgoingConnectedModules(
        InventoryModule ignoredModule,
        HashSet<InventoryModule> visitedModules,
        bool stopAtInterruptingModules
    )
    {
        return GetAllOutgoingConnectedModules(
            Slots.Values,
            GetOutgoingConnectionsSlots(),
            ignoredModule,
            visitedModules,
            stopAtInterruptingModules
        );
    }

    private HashSet<InventoryModule> GetAllOutgoingConnectedModules(
        IEnumerable<InventorySlot> slots,
        IEnumerable<InventorySlot> outgoingConnectionSlots,
        InventoryModule ignoredModule,
        HashSet<InventoryModule> visitedModules,
        bool stopAtInterruptingModules
    )
    {
        var directModules = GetDirectOutgoingConnectedModules(slots, outgoingConnectionSlots, ignoredModule);
        if (directModules.Count == 0)
        {
            return [];
        }

        var unvisitedModules = directModules.Where(visitedModules.Add).ToList();

        var modulesToRecurse = stopAtInterruptingModules
            ? unvisitedModules.Where(module => !module.Module.InterruptsConnections)
            : unvisitedModules;

        // This logic does not reflect complex chains with more than 3 modules,
        // some of which might be incoming, some outgoing. But it's fine for now
        return modulesToRecurse
            .SelectMany(module => module.GetAllOutgoingConnectedModules(ignoredModule, visitedModules, stopAtInterruptingModules))
            .Concat(directModules)
            .ToHashSet();
    }
    
    // Incoming connections logic
    
    private IEnumerable<InventorySlot> GetIncomingConnectionsSlots()
    {
        return Connections.Values
            .Where(connection => connection.Type == ConnectionType.Incoming)
            .Select(connection => connection.Slot)
            .OfType<InventorySlot>();
    }

    private HashSet<InventoryModule> GetDirectIncomingConnectedModules(
        IEnumerable<InventorySlot> slots,
        IEnumerable<InventorySlot> incomingConnectionSlots,
        InventoryModule ignoredModule
    )
    {
        return GetExternalDirectIncomingConnectedModules(slots, ignoredModule)
            .Concat(GetOwnDirectIncomingConnectedModules(incomingConnectionSlots, ignoredModule))
            .ToHashSet();
    }

    private IEnumerable<InventoryModule> GetExternalDirectIncomingConnectedModules(
        IEnumerable<InventorySlot> slots,
        InventoryModule ignoredModule
    )
    {
        return slots.SelectMany(slot => slot.Connections)
            .Where(connection => connection.Type == ConnectionType.Outgoing)
            .Select(connection => connection.Module)
            .Where(module => module != this && module != ignoredModule);
    }

    private IEnumerable<InventoryModule> GetOwnDirectIncomingConnectedModules(
        IEnumerable<InventorySlot> incomingConnectionSlots,
        InventoryModule ignoredModule)
    {
        return incomingConnectionSlots.Select(slot => slot.Module)
            .OfType<InventoryModule>()
            .Where(module => module != this && module != ignoredModule);
    }

    public HashSet<InventoryModule> GetAllIncomingConnectedModules()
    {
        return GetAllIncomingConnectedModules(
            Slots.Values,
            GetIncomingConnectionsSlots(),
            this,
            [],
            stopAtInterruptingModules: true
        );
    }

    private HashSet<InventoryModule> GetAllIncomingConnectedModules(
        InventoryModule ignoredModule,
        HashSet<InventoryModule> visitedModules,
        bool stopAtInterruptingModules
    )
    {
        return GetAllIncomingConnectedModules(
            Slots.Values,
            GetIncomingConnectionsSlots(),
            ignoredModule,
            visitedModules,
            stopAtInterruptingModules
        );
    }

    private HashSet<InventoryModule> GetAllIncomingConnectedModules(
        IEnumerable<InventorySlot> slots,
        IEnumerable<InventorySlot> incomingConnectionSlots,
        InventoryModule ignoredModule,
        HashSet<InventoryModule> visitedModules,
        bool stopAtInterruptingModules
    )
    {
        var directModules = GetDirectIncomingConnectedModules(slots, incomingConnectionSlots, ignoredModule);
        if (directModules.Count == 0)
        {
            return [];
        }

        var unvisitedModules = directModules.Where(visitedModules.Add).ToList();

        var modulesToRecurse = stopAtInterruptingModules
            ? unvisitedModules.Where(module => !module.Module.InterruptsConnections)
            : unvisitedModules;

        // This logic does not reflect complex chains with more than 3 modules,
        // some of which might be incoming, some outgoing. But it's fine for now
        return modulesToRecurse
            .SelectMany(module => module.GetAllIncomingConnectedModules(ignoredModule, visitedModules, stopAtInterruptingModules))
            .Concat(directModules)
            .ToHashSet();
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
        if (_isFirstFrame)
        {
            // Move immediately on first frame
            if (_targetPosition.HasValue)
            {
                Position = _targetPosition.Value;
            }
            else
            {
                GlobalPosition = MouseInputManager.Instance.GetCachedGlobalMousePosition();
            }
            
            _isFirstFrame = false;
            return;
        }
        
        if (!_targetPosition.HasValue || _targetPosition.Value.IsEqualApprox(Position))
        {
            return;
        }
        
        var followSpeed = IsFollowingCursor ? CursorFollowSpeed : TargetPositionFollowSpeed;
        var distanceToX = Abs(Position.X - _targetPosition.Value.X);
        var distanceToY = Abs(Position.Y - _targetPosition.Value.Y);
        var x = MoveToward(
            Position.X,
            _targetPosition.Value.X,
            followSpeed * (float)delta * distanceToX
        );
        var y = MoveToward(+
            Position.Y,
            _targetPosition.Value.Y,
            followSpeed * (float)delta * distanceToY
        );

        Position = new Vector2(x, y);
    }
    
    private Dictionary<HexCoordinates, InventorySlot> _hoveredSlots = [];
    private Dictionary<HexCoordinates, ConnectionData> _hoveredConnectorSlots = [];
    private bool _isJustGrabbed;

    private void FollowCursor()
    {
        if (!_mousePivot.HasValue)
        {
            return;
        }
        
        if (Input.IsActionJustPressed("drop_module"))
        {
            Drop();
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
        if (!allSlotsHovered || !IsAllSlotsAvailable(hoveredSlots.Values))
        {
            ResetHoveredSlots();
            _targetPosition = mousePosition - pivotOffset;
            _isJustGrabbed = false;
            return;
        }
        
        if (!hoveredSlots.ContentEqual(_hoveredSlots) || !hoveredConnectorSlots.ContentEqual(_hoveredConnectorSlots))
        {
            SlotsUnhovered(_hoveredSlots, _hoveredConnectorSlots);
            
            _hoveredSlots = hoveredSlots;
            _hoveredConnectorSlots = hoveredConnectorSlots;
            
            SlotsHovered(_hoveredSlots, _hoveredConnectorSlots);
        }
        
        ProcessHoveredSlots(hoveredSlots, hoveredConnectorSlots);
        
        _isJustGrabbed = false;
    }

    private void ResetHoveredSlots()
    {
        if (_hoveredSlots.Count == 0 && _hoveredConnectorSlots.Count == 0)
        {
            return;
        }
        
        SlotsUnhovered(_hoveredSlots, _hoveredConnectorSlots);
            
        _hoveredSlots.Clear();
        _hoveredConnectorSlots.Clear();
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

    private void SlotsHovered(
        Dictionary<HexCoordinates, InventorySlot> slots,
        Dictionary<HexCoordinates, ConnectionData> connectorSlots
    )
    {
        var slotsValues = slots.Values;
        foreach (var slot in slotsValues)
        {
            slot.SetHoveredState();
        }

        var validationResult = ValidateConnection(slots, connectorSlots);
        
        var incomingConnectionSlots = validationResult.IncomingConnectedModules
            .SelectMany(module => module.Slots.Values);
        var outgoingConnectionSlots = validationResult.OutgoingConnectedModules
            .SelectMany(module => module.Slots.Values);
        var allConnectionSlots = incomingConnectionSlots
            .Concat(outgoingConnectionSlots)
            .Concat(validationResult.IncomingConnectionDirectSlots)
            .Concat(validationResult.OutgoingConnectionDirectSlots)
            .Distinct();
        
        foreach (var slot in allConnectionSlots)
        {
            if (!validationResult.IsValid)
            {
                slot.SetShowsCycleState();
            }
            else
            {
                slot.SetShowsConnectionsState();
            }
        }

        if (_isJustGrabbed)
        {
            return;
        }
        
        if (!validationResult.IsValid)
        {
            SoundManager.Instance.PlaySound(_invalidConnectionSound).RandomizePitchOffset();
        }
        else
        {
            SoundManager.Instance.PlaySound(_slotSnapSound).RandomizePitchOffset();
        }
    }

    private ConnectionValidationResult ValidateConnection(
        Dictionary<HexCoordinates, InventorySlot> slots,
        Dictionary<HexCoordinates, ConnectionData> connectorSlots)
    {
        var incomingConnectionDirectSlots = connectorSlots
            .Where(slot => slot.Value.Connection.Type == ConnectionType.Incoming)
            .Select(slot => slot.Value.Slot)
            .OfType<InventorySlot>()
            .ToHashSet();
        var outgoingConnectionDirectSlots = connectorSlots
            .Where(slot => slot.Value.Connection.Type == ConnectionType.Outgoing)
            .Select(slot => slot.Value.Slot)
            .OfType<InventorySlot>()
            .ToHashSet();

        var incomingConnectedModules = GetAllIncomingConnectedModules(
            slots.Values,
            incomingConnectionDirectSlots,
            ignoredModule: this,
            visitedModules: [],
            stopAtInterruptingModules: false
        );
        var outgoingConnectedModules = GetAllOutgoingConnectedModules(
            slots.Values,
            outgoingConnectionDirectSlots,
            ignoredModule: this,
            visitedModules: [],
            stopAtInterruptingModules: false
        );
        
        var hasCycle = outgoingConnectedModules.Any(incomingConnectedModules.Contains);
        return new ConnectionValidationResult(
            incomingConnectionDirectSlots,
            outgoingConnectionDirectSlots,
            incomingConnectedModules,
            outgoingConnectedModules,
            !hasCycle
        );
    }

    private readonly record struct ConnectionValidationResult(
        HashSet<InventorySlot> IncomingConnectionDirectSlots,
        HashSet<InventorySlot> OutgoingConnectionDirectSlots,
        HashSet<InventoryModule> IncomingConnectedModules,
        HashSet<InventoryModule> OutgoingConnectedModules,
        bool IsValid
    );
    
    private void ProcessHoveredSlots(
        Dictionary<HexCoordinates, InventorySlot> slots,
        Dictionary<HexCoordinates, ConnectionData> connectorSlots
    )
    {
        _targetPosition = GetSlotBasedPosition(slots);

        if (!Input.IsActionJustPressed("inventory_left_click"))
        {
            return;
        }
        
        var validationResult = ValidateConnection(slots, connectorSlots);

        if (!validationResult.IsValid)
        {
            SoundManager.Instance.PlaySound(_invalidConnectionSound).RandomizePitchOffset();
                
            _animationTween?.Kill();
            _animationTween = CreateTween().SetTrans(Tween.TransitionType.Quad);
                
            _animationTween.TweenOffsetRotation(this, 0.1f, AnimationTweenDuration / 3)
                .SetEase(Tween.EaseType.Out);

            _animationTween.TweenOffsetRotation(this, -0.1f, AnimationTweenDuration / 3);
                
            _animationTween.TweenOffsetRotationReset(this, AnimationTweenDuration / 3);
                
            return;
        }
            
        SoundManager.Instance.PlaySound(_insertSound).RandomizePitchOffset();
        ForceInsert(slots, connectorSlots);
        StopFollowingCursor();
    }
    
    private void SlotsUnhovered(
        Dictionary<HexCoordinates, InventorySlot> slots,
        Dictionary<HexCoordinates, ConnectionData> connectorSlots
    )
    {
        InventoryManager.Instance.EmitSignal(InventoryManager.SignalName.SlotsStateReset);
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

        SoundManager.Instance.PlaySound(_rotateSound).RandomizePitchOffset();
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

        Slots = moduleSlots.ToDictionary();
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

        if (!_isFirstInsert)
        {
            return;
        }

        _isFirstInsert = false;
        if (InventoryManager.Instance.IsOpen)
        {
            ShowModule();
        }
        else
        {
            Modulate = Modulate.AsTransparent();
            HideModule();
        }

        _targetPosition = GetSlotBasedPosition(Slots);
    }

    public void StartFollowingCursor(bool grabClosestHex = true)
    {
        if (_mousePivot.HasValue)
        {
            return;
        }

        _isJustGrabbed = true;
        HideModuleInfo();
        InventoryManager.Instance.DraggingModule = this;
        MouseFilter = MouseFilterEnum.Ignore;

        var mousePosition = MouseInputManager.Instance.GetCachedGlobalMousePosition();

        KeyValuePair<HexCoordinates, HexData> pivotHex;
        if (grabClosestHex)
        {
            pivotHex = _hexes
                .Where(x => x.Value.Connection == null)
                .MinBy(entry => (entry.Value.RealPosition + Position).DistanceSquaredTo(mousePosition));
        }
        else
        {
            pivotHex = _hexes.First();
        }
        
        _mousePivot = pivotHex.Key;

        ZIndex += 1;

        _animationTween?.Kill();
        _animationTween = CreateTween().SetTrans(Tween.TransitionType.Quad);

        _animationTween.TweenOffsetScale(this, 1.15f, AnimationTweenDuration).SetEase(Tween.EaseType.Out);
        _animationTween.Parallel()
            .TweenOffsetRotation(this, RandomUtils.RandomSignedDeltaRange(0.1f, 0.05f), AnimationTweenDuration)
            .SetEase(Tween.EaseType.Out);

        _animationTween.TweenOffsetScaleReset(this, AnimationTweenDuration).SetEase(Tween.EaseType.In);
        _animationTween.Parallel().TweenOffsetRotationReset(this, AnimationTweenDuration).SetEase(Tween.EaseType.In);
        
        SoundManager.Instance.PlaySound(_grabSound).RandomizePitchOffset();
        
        EmitSignalTakenOut();
        InventoryManager.Instance.EmitSignal(InventoryManager.SignalName.ModuleGrabbed, this);
    }

    private void StopFollowingCursor()
    {
        if (!_mousePivot.HasValue)
        {
            return;
        }
        
        InventoryManager.Instance.DraggingModule = null;
        ResetHoveredSlots();
        MouseFilter = MouseFilterEnum.Stop;
        _mousePivot = null;
        ZIndex -= 1;

        var isInserted = Slots.Count != 0;
        if (!isInserted)
        {
            Drop();
            return;
        }

        if (IsHovered())
        {
            ShowModuleInfo();
        }
 
        _animationTween?.Kill();
        _animationTween = CreateTween().SetTrans(Tween.TransitionType.Quad);

        _animationTween.TweenOffsetScale(this, 0.9f, AnimationTweenDuration).SetEase(Tween.EaseType.Out);
        _animationTween.Parallel()
            .TweenOffsetRotation(this, RandomUtils.RandomSignedDeltaRange(0.1f, 0.05f), AnimationTweenDuration)
            .SetEase(Tween.EaseType.Out);

        _animationTween.TweenOffsetScaleReset(this, AnimationTweenDuration).SetEase(Tween.EaseType.In);
        _animationTween.Parallel().TweenOffsetRotationReset(this, AnimationTweenDuration).SetEase(Tween.EaseType.In);
        
        EmitSignalInserted();
        InventoryManager.Instance.EmitSignal(InventoryManager.SignalName.ModuleInserted, this);
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
        _animationTween?.Kill();
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