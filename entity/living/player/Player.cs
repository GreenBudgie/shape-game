using System.Collections.Generic;

public partial class Player : Entity
{

    public static readonly Vector2 MaxVisibleSize = new(140, 140);

    private const int CornerDistance = 28;

    /**
     * The player's tilt will never go above this value.
     */
    private const double MaxTiltDegrees = 22;

    /**
     * How rapidly the player will tilt when it is moved horizontally. Higher = faster.
     */
    private const double TiltIncreaseFactor = 0.1;

    /**
     * How fast the player's tilt will decrease over time (arbitrary number). Higher = faster.
     */
    private const double TiltDecreaseFactor = 7;

    /**
     * The lowest threshold of player's tilt in degrees. When this value is reached, the tilt is instantly set to 0.
     */
    private const double RotationDegreesEpsilon = 0.3;
    
    private static readonly PackedScene Scene = GD.Load<PackedScene>("uid://bw1n7eocfhsbo");
    
    private static Player? _instance;

    [Signal]
    public delegate void RespawnedEventHandler();
    
    private static int _lastIndex = -1;

    private Vector2 _windowCenter;
    private Blaster _leftBlaster = null!;
    private Blaster _rightBlaster = null!;
    private ShapeCast2D _playerCollisionDetector = null!;
    private Vector2 _prevPosition = Vector2.Zero;

    public static Player? FindPlayer()
    {
        return _instance;
    }

    public static void Respawn()
    {
        if (_instance != null)
        {
            return;
        }

        var newPlayer = Scene.Instantiate<Player>();
        newPlayer.GlobalPosition = ShapeGame.Center;
        ShapeGame.Instance.AddChild(newPlayer);
        ShapeGame.Instance.MoveChild(newPlayer, _lastIndex);

        PlayerManager.Instance.EmitSignal(PlayerManager.SignalName.Respawned);
    }

    public override void _EnterTree()
    {
        _instance = this;
    }

    public override void _ExitTree()
    {
        _lastIndex = GetIndex();
        _playerCollisionDetector.QueueFree();
        _instance = null;
    }

    protected override void Setup()
    {
        Sprite = GetNode<Sprite2D>("GlowWrapper/PlayerSprite");

        _leftBlaster = Blaster.Create(InventoryManager.Instance.LeftBlasterInventory);
        AddChild(_leftBlaster);
        _rightBlaster = Blaster.Create(InventoryManager.Instance.RightBlasterInventory);
        AddChild(_rightBlaster);
        
        _playerCollisionDetector = GetNode<ShapeCast2D>("PlayerCollisionDetector");
        Callable.From(SetupCollisionDetector).CallDeferred();
        
        Glow = GetNode<GlowWrapper>("GlowWrapper")
            .SetColor(ColorScheme.LightBlueGreen)
            .SetStrength(0)
            .SetRadius(0)
            .EnablePulsing();

        _prevPosition = Position;

        HealthController.MaxHealth = 10;
        HealthController.InvulnerabilityPeriod = 0.1f;
        HealthController.DestroyAnimationFinished += QueueFree;
        HealthController.Destroyed += OnDestroy;
        HealthController.HealthChanged += OnHealthChanged;
        
        FireDisplay.Attach(this).SetColor(ColorScheme.LightBlueGreen);
    }
    
    private void OnHealthChanged(float delta)
    {
        PlayerManager.Instance.EmitSignal(PlayerManager.SignalName.HealthChanged, HealthController.Health);
    }

    private void OnDestroy()
    {
        PlayerManager.Instance.EmitSignal(PlayerManager.SignalName.Destroyed);
    }

    private void SetupCollisionDetector()
    {
        _playerCollisionDetector.Reparent(ShapeGame.Instance);
    }

    public override void _Process(double delta)
    {
        Unstuck();
        
        var mouseDelta = MouseInputManager.Instance.GetMouseDelta();

        var force = mouseDelta * 800;
        ApplyCentralForce(force);

        var positionDelta = Position - _prevPosition;
        var tiltDegrees = positionDelta.X * TiltIncreaseFactor;
        HandleTilt(delta, tiltDegrees);

        RegisterPlayerCollisions();
        _prevPosition = Position;

        HandleFire();
    }

    private void Unstuck()
    {
        const float expansion = 1024;

        var expandedArea = new Rect2(
            ShapeGame.PlayableArea.Position.X - expansion,
            ShapeGame.PlayableArea.Position.Y - expansion,
            ShapeGame.PlayableArea.Size.X + expansion * 2,
            ShapeGame.PlayableArea.Size.Y + expansion * 2
        );
        if (expandedArea.HasPoint(GlobalPosition))
        {
            return;
        }

        GlobalPosition = _prevPosition;
        LinearVelocity = Vector2.Zero;
    }

    private void HandleFire()
    {
        if (!MouseInputManager.Instance.IsAttackEnabled || !MouseInputManager.Instance.IsCharacterControlEnabled)
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
        Sprite.RotationDegrees *= Clamp(1 - (float)(delta * TiltDecreaseFactor), 0, 1);
        Sprite.RotationDegrees = (float)Clamp(
            Sprite.RotationDegrees + tiltDegrees,
            -MaxTiltDegrees,
            MaxTiltDegrees
        );
        if (Abs(Sprite.RotationDegrees) <= RotationDegreesEpsilon)
        {
            Sprite.RotationDegrees = 0;
        }
    }

    private readonly List<CollisionObject2D> _playerColliders = [];
    
    /// <summary>
    /// Reminder on how to set up player precise collision detection if physical interaction with player is not needed:
    /// - Enable collision LAYER 10: player_collider
    /// - Enable collision MASK 14: player_collider_mask
    /// - Do NOT enable collision layer 1: player
    ///
    /// Listen to BodyEntered/BodyExited signals - they will be called like player itself has collided
    /// </summary>
    private void RegisterPlayerCollisions()
    {
        _playerCollisionDetector.Rotation = GetTilt();
        _playerCollisionDetector.Position = _prevPosition;
        _playerCollisionDetector.TargetPosition = _playerCollisionDetector.ToLocal(GlobalPosition);
        _playerCollisionDetector.ForceShapecastUpdate();

        List<CollisionObject2D> currentFrameColliders = [];
        
        // Register first player collisions
        for (var i = 0; i < _playerCollisionDetector.GetCollisionCount(); i++)
        {
            var collider = _playerCollisionDetector.GetCollider(i);
            if (collider is not CollisionObject2D collisionObject)
            {
                continue;
            }

            if (!collisionObject.HasCollisionMask(CollisionLayers.PlayerColliderMask))
            {
                continue;
            }

            if (collisionObject is RigidBody2D body && body.GetCollisionExceptions().Contains(this))
            { 
                continue;
            }

            currentFrameColliders.Add(collisionObject);

            if (!_playerColliders.Contains(collisionObject))
            {
                if (collisionObject is Area2D area)
                {
                    area.EmitSignal(Area2D.SignalName.BodyEntered, this);
                }
                
                if (collisionObject is RigidBody2D rigidBody)
                {
                    rigidBody.EmitSignal(RigidBody2D.SignalName.BodyEntered, this);
                }
                
                _playerColliders.Add(collisionObject);
            }
        }
        
        // Unregister exiting player collisions
        foreach (var collider in _playerColliders)
        {
            if (!IsInstanceValid(collider))
            {
                continue;
            }
            
            if (!currentFrameColliders.Contains(collider))
            {
                if (collider is Area2D area)
                {
                    area.EmitSignal(Area2D.SignalName.BodyExited, this);
                }
                
                if (collider is RigidBody2D rigidBody)
                {
                    rigidBody.EmitSignal(RigidBody2D.SignalName.BodyExited, this);
                }
            }
        }
        
        // Remove all colliders that exited the player shape
        _playerColliders.RemoveAll(collider => !currentFrameColliders.Contains(collider));
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
    
    public Vector2 GetGlobalCenter()
    {
        return ToGlobal(CenterOfMass);
    }

    public float GetTilt()
    {
        return Sprite.Rotation;
    }
    
    public float GetTiltDegrees()
    {
        return Sprite.RotationDegrees;
    }

}