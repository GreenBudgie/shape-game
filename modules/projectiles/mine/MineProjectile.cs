public partial class MineProjectile : RigidBody2D, IProjectile<MineProjectile>
{
    private static readonly PackedScene Scene = GD.Load<PackedScene>("uid://coo3tnuma02fs");

    [Export] private AudioStream _shotSound = null!;

    [Export] private AudioStream _wallHitSound = null!;

    public MineProjectile Node => this;

    public static MineProjectile Create()
    {
        return Scene.Instantiate<MineProjectile>();
    }

    private ShotContext _context = null!;
    private Explosion? _explosion;

    public void Prepare(ShotContext context)
    {
        _context = context;
    }

    public override void _Ready()
    {
        SoundManager.Instance.PlayPositionalSound(this, _shotSound).RandomizePitchOffset(0.1f);
        GetTree().CreateTimer(0.5f).Timeout += Fuse;
        BodyEntered += HandleBodyEntered;
    }

    private bool _torqueApplied;

    public override void _IntegrateForces(PhysicsDirectBodyState2D state)
    {
        if (_torqueApplied)
        {
            return;
        }

        const float initialTorque = 500;
        const float initialTorqueDelta = 200;
        var torque = RandomUtils.RandomSignedDeltaRange(initialTorque, initialTorqueDelta);
        ApplyTorqueImpulse(torque);
        _torqueApplied = true;
    }

    private void Fuse()
    {
        _explosion = Explosion.Create(this)
            .SetDamage(_context.CalculateStat<ExplosionDamageStat>())
            .SetRadius(_context.CalculateStat<ExplosionRadiusStat>())
            .SetFuseTimeSeconds(1);

        _explosion.Connect(Explosion.SignalName.Detonated, Callable.From(QueueFree));
    }

    private void HandleBodyEntered(Node body)
    {
        if (body is not CollisionObject2D collisionObject2D)
        {
            return;
        }

        if (collisionObject2D.GetCollisionLayerValue(CollisionLayers.LevelWalls))
        {
            SoundManager.Instance.PlayPositionalSound(this, _wallHitSound).RandomizePitchOffset(0.1f);
        }

        if (collisionObject2D.GetCollisionLayerValue(CollisionLayers.LevelOutsideBoundary))
        {
            QueueFree();
        }
    }
}