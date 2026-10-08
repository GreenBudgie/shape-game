public partial class EnemySquare : Enemy
{
    private static readonly PackedScene PathScene = GD.Load<PackedScene>("uid://b1ehfaspqd28s");

    [Export] private AudioStream _shotSound = null!;

    private const double MaxFireDelay = 0.5f;
    private const double MinFireDelay = 0.25f;
    private const double FireDelayDelta = 0.1f;
    private const double TimeToMinFireDelaySeconds = 30f;

    private double _fireTimer = RandomUtils.DeltaRange(MaxFireDelay, FireDelayDelta);
    private double _currentFireDelay = MaxFireDelay;
    private EnemyPathFollowController _pathFollowController = null!;

    protected override void Setup()
    {
        base.Setup();
        
        HealthController.MaxHealth = 12;
    }
    
    protected override void OnActivate()
    {
        var path = PathScene.Instantiate<EnemySquarePath>();
        ShapeGame.Instance.CallDeferred(Node.MethodName.AddChild, path);
        _pathFollowController = EnemyPathFollowController.AttachEnemyToPath(this, path);
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

        if (HealthController.IsDestroyed())
        {
            return;
        }

        if (!_pathFollowController.IsPathReached)
        {
            return;
        }

        if (_currentFireDelay > MinFireDelay)
        {
            _currentFireDelay -= delta / TimeToMinFireDelaySeconds;
        }

        if (_fireTimer <= 0)
        {
            Fire();
            _fireTimer = RandomUtils.DeltaRange(_currentFireDelay, FireDelayDelta);
        }
        else
        {
            _fireTimer -= delta;
        }
    }

    public override float GetCrystalsToDrop()
    {
        return 2;
    }

    private void Fire()
    {
        var randomStrength = (float)GD.RandRange(1f, 2f);
        var velocityLength = LinearVelocity.Length();
        var impulse = Vector2.Down * velocityLength * 0.5f - LinearVelocity * randomStrength;
        var impulseLength = impulse.Length();
        
        var context = new SpawnableContext(EnemySquareProjectile.Create())
        {
            Position = GlobalPosition,
            Source = this,
            Direction = impulse.Normalized(),
        };
 
        context.Stats.Add(new SpeedStat { Value = impulseLength });
        
        context.Spawn();

        const float impulseOffset = 10f;
        var randomOffset = new Vector2(
            (float)GD.RandRange(-impulseOffset, impulseOffset),
            (float)GD.RandRange(-impulseOffset, impulseOffset)
        );

        ApplyImpulse(-impulse * 0.3f, randomOffset);

        var sound = SoundManager.Instance.PlayPositionalSound(this, _shotSound);
        var unclampedPitch = impulseLength / 4000f + 0.75f;
        sound.PitchScale = Clamp(unclampedPitch, 0.7f, 1.3f);
    }
}