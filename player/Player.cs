public partial class Player : CharacterBody2D
{

    private const int CornerDistance = 28;

    private static Player? _instance;

    /**
     * The player's tilt will never go above this value.
     */
    [ExportGroup("Tilt")] [Export(PropertyHint.Range, "0,40,1,or_greater")]
    private double _maxTiltDegrees = 22;

    /**
     * How rapidly the player will tilt when it is moved horizontally. Higher = faster.
     */
    [Export(PropertyHint.Range, "0,1,0.01,or_greater")]
    private double _tiltIncreaseFactor = 0.1;

    /**
     * How fast the player's tilt will decrease over time (arbitrary number). Higher = faster.
     */
    [Export(PropertyHint.Range, "1,20,0.1,or_greater")]
    private double _tiltDecreaseFactor = 7;

    /**
     * The lowest threshold of player's tilt in degrees. When this value is reached, the tilt is instantly set to 0.
     */
    [Export(PropertyHint.Range, "0.1,2,0.1,or_greater")]
    private double _rotationDegreesEpsilon = 0.3;

    private Vector2 _windowCenter;
    private Blaster _leftBlaster = null!;
    private Blaster _rightBlaster = null!;
    private ShapeCast2D _playerCollisionDetector = null!;
    private Sprite2D _sprite = null!;

    public static Player? FindPlayer()
    {
        return _instance;
    }

    public override void _EnterTree()
    {
        _instance = this;
    }

    public override void _Ready()
    {
        _sprite = GetNode<Sprite2D>("PlayerSprite");

        _leftBlaster = Blaster.Create(InventoryManager.Instance.LeftBlasterInventory);
        AddChild(_leftBlaster);
        _rightBlaster = Blaster.Create(InventoryManager.Instance.RightBlasterInventory);
        AddChild(_rightBlaster);
        
        _windowCenter = new Vector2(
            GetViewportRect().Size.X / 2.0f,
            GetViewportRect().Size.Y / 2.0f
        );

        PauseManager.Instance.GameUnpause += () => MoveMouseToWindowCenter(_windowCenter);
        PauseManager.Instance.GamePause += () => MoveMouseToWindowCenter(ShapeGame.PlayableArea.GetCenter());
        InventoryManager.Instance.InventoryClosed += () => MoveMouseToWindowCenter(_windowCenter);
        InventoryManager.Instance.InventoryOpened += () => MoveMouseToWindowCenter(ShapeGame.PlayableArea.GetCenter());

        _playerCollisionDetector = GetNode<ShapeCast2D>("PlayerCollisionDetector");
        Callable.From(SetupCollisionDetector).CallDeferred();
    }

    private void SetupCollisionDetector()
    {
        _playerCollisionDetector.Reparent(ShapeGame.Instance);
    }

    public override void _Process(double delta)
    {
        var mouseDelta = Vector2.Zero;
        if (!InventoryManager.Instance.IsOpen)
        {
            mouseDelta = MoveMouseToWindowCenter(_windowCenter);
        }

        _playerCollisionDetector.GlobalPosition = GlobalPosition;

        var prevPosition = Position;
        Velocity = mouseDelta * (float)(1 / delta);
        MoveAndSlide();

        var positionDelta = Position - prevPosition;
        var tiltDegrees = positionDelta.X * _tiltIncreaseFactor;
        HandleTilt(delta, tiltDegrees);

        RegisterPlayerCollisions();

        HandleFire();
    }

    private void HandleFire()
    {
        if (InventoryManager.Instance.IsOpen)
        {
            return;
        }
        
        if (Input.IsActionPressed("primary_fire"))
        {
            _leftBlaster.Trigger();
        }
        
        if (Input.IsActionPressed("secondary_fire"))
        {
            _rightBlaster.Trigger();
        }
    }

    private void HandleTilt(double delta, double tiltDegrees)
    {
        _sprite.RotationDegrees *= Clamp(1 - (float)(delta * _tiltDecreaseFactor), 0, 1);
        _sprite.RotationDegrees = (float)Clamp(
            _sprite.RotationDegrees + tiltDegrees,
            -_maxTiltDegrees,
            _maxTiltDegrees
        );
        if (Abs(_sprite.RotationDegrees) <= _rotationDegreesEpsilon)
        {
            _sprite.RotationDegrees = 0;
        }
    }

    private void RegisterPlayerCollisions()
    {
        _playerCollisionDetector.Rotation = GetTilt();
        _playerCollisionDetector.TargetPosition = _playerCollisionDetector.ToLocal(GlobalPosition);
        _playerCollisionDetector.ForceShapecastUpdate();

        if (!_playerCollisionDetector.IsColliding())
        {
            return;
        }

        for (var i = 0; i < _playerCollisionDetector.GetCollisionCount(); i++)
        {
            var collider = _playerCollisionDetector.GetCollider(i);
            if (collider is IPlayerCollisionDetector collisionDetector)
            {
                collisionDetector.CollideWithPlayer(this);
            }
        }
    }

    private Vector2 MoveMouseToWindowCenter(Vector2 windowCenterToUse)
    {
        var mousePosition = GetViewport().GetMousePosition();
        GetViewport().WarpMouse(windowCenterToUse);
        return mousePosition - windowCenterToUse;
    }

    public Vector2 GetGlobalNosePosition()
    {
        return GlobalPosition - new Vector2(0, CornerDistance).Rotated(GetTilt());
    }

    public Vector2 GetGlobalLeftCornerPosition()
    {
        return GlobalPosition + new Vector2(-CornerDistance, CornerDistance).Rotated(GetTilt());
    }

    public Vector2 GetGlobalRightCornerPosition()
    {
        return GlobalPosition + new Vector2(CornerDistance, CornerDistance).Rotated(GetTilt());
    }

    public float GetTilt()
    {
        return _sprite.Rotation;
    }
    
    public float GetTiltDegrees()
    {
        return _sprite.RotationDegrees;
    }

}