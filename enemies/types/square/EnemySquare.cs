public partial class EnemySquare : Enemy
{
    private static readonly PackedScene PathScene = GD.Load<PackedScene>("uid://b1ehfaspqd28s");

    [Export] private AudioStream _shotSound = null!;

    private const double FireDelay = 0.5f;
    private const double FireDelayDelta = 0.1f;

    private double _fireTimer = RandomUtils.DeltaRange(FireDelay, FireDelayDelta);
    private EnemyPathFollowController _pathFollowController = null!;

    protected override void OnActivate()
    {
        var path = PathScene.Instantiate<EnemySquarePath>();
        ShapeGame.Instance.CallDeferred(Node.MethodName.AddChild, path);
        _pathFollowController = EnemyPathFollowController.AttachEnemyToPath(this, path);
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

        if (IsDestroyed)
        {
            return;
        }

        if (!_pathFollowController.IsPathReached)
        {
            return;
        }

        if (_fireTimer <= 0)
        {
            Fire();
            _fireTimer = RandomUtils.DeltaRange(FireDelay, FireDelayDelta);
        }
        else
        {
            _fireTimer -= delta;
        }
    }

    public override float GetMaxHealth()
    {
        return 5;
    }

    public override float GetCrystalsToDrop()
    {
        return 1;
    }

    private void Fire()
    {
        var bullet = EnemySquareProjectile.Create(this);
        bullet.GlobalPosition = GlobalPosition;
        var randomStrength = (float)GD.RandRange(1f, 2f);
        var velocityLength = LinearVelocity.Length();
        var impulse = Vector2.Down * velocityLength * 0.5f - LinearVelocity * randomStrength;
        bullet.ApplyCentralImpulse(impulse);
        ShapeGame.Instance.AddChild(bullet);

        const float impulseOffset = 10f;
        var randomOffset = new Vector2(
            (float)GD.RandRange(-impulseOffset, impulseOffset),
            (float)GD.RandRange(-impulseOffset, impulseOffset)
        );

        ApplyImpulse(-impulse * 0.3f, randomOffset);

        var sound = SoundManager.Instance.PlayPositionalSound(this, _shotSound);
        var unclampedPitch = impulse.Length() / 4000f + 0.75f;
        sound.PitchScale = Clamp(unclampedPitch, 0.7f, 1.3f);
    }
}