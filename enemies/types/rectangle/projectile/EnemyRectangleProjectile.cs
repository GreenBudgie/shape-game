public partial class EnemyRectangleProjectile : RigidBody2D, IPlayerCollisionDetector
{

    private static readonly PackedScene Scene = GD.Load<PackedScene>("uid://b1rpikysssmoh");

    [Export] private AudioStream _hitWallSound = null!;
    
    private EnemyRectangle _owner = null!;
    
    public static EnemyRectangleProjectile Create(EnemyRectangle owner)
    {
        var node = Scene.Instantiate<EnemyRectangleProjectile>();
        node._owner = owner;
        return node;
    }

    public override void _Ready()
    {
        const float initialSpeed = 1500f;
        const float initialSpeedDelta = 250f;
        var speed = RandomUtils.DeltaRange(initialSpeed, initialSpeedDelta);
        var direction = Vector2.FromAngle(_owner.Rotation + Pi / 2);
        ApplyCentralImpulse(direction * speed);

        EnemyRectangleProjectileParticles.Create(this);
        
        BodyEntered += HandleBodyEntered;
    }

    public override void _Process(double delta)
    {
        if (this.IsBelowPlayableArea())
        {
            QueueFree();
        }
    }

    private bool _torqueApplied;

    public override void _IntegrateForces(PhysicsDirectBodyState2D state)
    {
        if (_torqueApplied)
        {
            return;
        }

        const float initialTorque = 4000;
        const float initialTorqueDelta = 1000;
        var torque = RandomUtils.RandomSignedDeltaRange(initialTorque, initialTorqueDelta);
        ApplyTorqueImpulse(torque);
        _torqueApplied = true;
    }

    public void CollideWithPlayer(Player player)
    {
        QueueFree();
    }
    
    private void HandleBodyEntered(Node body)
    {
        if (body is not CollisionObject2D collisionObject)
        {
            return;
        }

        if (collisionObject.HasCollisionLayer(CollisionLayers.LevelOutsideBoundary))
        {
            QueueFree();
            return;
        }

        if (!collisionObject.HasCollisionLayer(CollisionLayers.LevelWalls))
        {
            return;
        }

        var speed = LinearVelocity.Length();
        if (speed <= 100)
        {
            return;
        }

        SoundManager.Instance.PlayPositionalSound(this, _hitWallSound)
            .RandomizePitch(0.8f, 1.2f);
    }
    
}
