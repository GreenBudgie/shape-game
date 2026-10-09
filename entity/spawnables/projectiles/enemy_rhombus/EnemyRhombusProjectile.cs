public partial class EnemyRhombusProjectile : BasicRigidBodyProjectile
{
    private static readonly PackedScene Scene = GD.Load<PackedScene>("uid://c0kcy42pxfucm");

    [Export] private AudioStream _hitWallSound = null!;

    private bool _isDissolving;
    private GpuParticles2D _particles = null!;

    public static EnemyRhombusProjectile Create()
    {
        return Scene.Instantiate<EnemyRhombusProjectile>();
    }

    protected override void OnReady()
    {
        _particles = GetNode<GpuParticles2D>("GPUParticles2D");
    }

    public override void Remove()
    {
        LaunchTriggers();
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