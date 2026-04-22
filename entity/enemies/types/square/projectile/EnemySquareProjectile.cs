public partial class EnemySquareProjectile : RigidBody2D, IPlayerCollisionDetector
{

    private const float InitialTorque = 1500; 
    private const float InitialTorqueDelta = 500; 
    
    private static readonly PackedScene Scene = GD.Load<PackedScene>("uid://bhj8dgeytmpxx");
    
    [Export] private AudioStream _hitWallSound = null!;

    public static EnemySquareProjectile Create(EnemySquare shooter)
    {
        var bullet = Scene.Instantiate<EnemySquareProjectile>();
        return bullet;
    }
    
    public override void _Ready()
    {
        BodyEntered += HandleBodyEntered;
    }

    private bool _torqueApplied;
    
    public override void _IntegrateForces(PhysicsDirectBodyState2D state)
    {
        if (_torqueApplied)
        {
            return;
        }

        var torque = RandomUtils.RandomSignedDeltaRange(InitialTorque, InitialTorqueDelta);
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
        
        if (!body.IsInGroup("level_walls"))
        {
            return;
        }

        var speed = LinearVelocity.Length();
        if (speed <= 100)
        {
            return;
        }

        var pitch = speed / 1000f + 0.75f;
        var sound = SoundManager.Instance.PlayPositionalSound(this, _hitWallSound);
        sound.PitchScale = Clamp(pitch, 0.75f, 1.25f);
    }
}