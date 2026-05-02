public partial class EnemyRhombusProjectile : BasicRigidBodyProjectile<EnemyRhombusProjectile>
{
    private const float MaxLifetimeSeconds = 4;

    private static readonly PackedScene Scene = GD.Load<PackedScene>("uid://c0kcy42pxfucm");

    [Export] private AudioStream _hitWallSound = null!;

    private bool _isDissolving;
    private GpuParticles2D _particles = null!;
    
    public override EnemyRhombusProjectile Node => this;

    public static EnemyRhombusProjectile Create()
    {
        return Scene.Instantiate<EnemyRhombusProjectile>();
    }

    public override void Prepare(SpawnableContext context)
    {
        context.Stats.Add(new LifetimeStat {Lifetime = MaxLifetimeSeconds});
        context.Stats.Add(new DamageStat {Damage = 3});
    }

    public override void _Ready()
    {
        base._Ready();
        
        _particles = GetNode<GpuParticles2D>("GPUParticles2D");
    }

    public override void Remove()
    {
        Dissolve();
    }

    private bool _isFirstTick = true;

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        
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
        DissolveEffect.DissolveAndRemove(this, GetNode<Sprite2D>("Sprite2D"));
        _isDissolving = true;
    }

}