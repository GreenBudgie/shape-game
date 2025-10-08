public partial class EnemyRhombusProjectile : RigidBody2D, IPlayerCollisionDetector
{
    private const double MaxLifetimeSeconds = 4;

    private static readonly PackedScene Scene = GD.Load<PackedScene>("uid://c0kcy42pxfucm");

    [Export] private AudioStream _hitWallSound = null!;

    private double _lifetimeSeconds = MaxLifetimeSeconds;
    private bool _isDissolving;
    private GpuParticles2D _particles = null!;

    public static EnemyRhombusProjectile Create()
    {
        var node = Scene.Instantiate<EnemyRhombusProjectile>();
        return node;
    }

    public override void _Ready()
    {
        _particles = GetNode<GpuParticles2D>("GPUParticles2D");
        BodyEntered += HandleBodyEntered;
    }

    public override void _Process(double delta)
    {
        if (_isDissolving)
        {
            return;
        }

        if (_lifetimeSeconds <= 0)
        {
            Dissolve();
            return;
        }

        _lifetimeSeconds -= delta;
    }

    private bool _isFirstTick = true;

    public override void _PhysicsProcess(double delta)
    {
        var direction = LinearVelocity.Normalized();
        var angle = direction.Angle() + Pi;
        Rotation = angle;

        if (!_isFirstTick)
        {
            HandleDissolve();
        }

        _isFirstTick = false;
    }

    private void HandleDissolve()
    {
        if (_isDissolving)
        {
            return;
        }

        if (LinearVelocity.IsZeroApprox())
        {
            Dissolve();
        }
    }

    public void CollideWithPlayer(Player player)
    {
        QueueFree();
    }

    private void Dissolve()
    {
        if (_isDissolving)
        {
            return;
        }

        _particles.QueueFree();
        LinearDamp = 5;
        CollisionLayer = 0;
        CollisionMask = 0;
        DissolveEffect.Dissolve(this, GetNode<Sprite2D>("Sprite2D"));
        _isDissolving = true;
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