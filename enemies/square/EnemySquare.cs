public partial class EnemySquare : Enemy
{

    private static readonly PackedScene ProjectileScene = GD.Load<PackedScene>("uid://bhj8dgeytmpxx");
    private static readonly PackedScene PathScene = GD.Load<PackedScene>("uid://b1ehfaspqd28s");
    
    [Export] private AudioStream _shotSound = null!;

    private const double FireDelay = 1;

    private double _fireTimer = FireDelay;

    private EnemySquarePath _path = null!;

    public override void _Ready()
    {
        base._Ready();
        
        _path = PathScene.Instantiate<EnemySquarePath>();
        ShapeGame.Instance.CallDeferred(Node.MethodName.AddChild, _path);
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        
        if (IsDestroyed)
        {
            return;
        }
        
        // TODO move to physics process
        var direction = GlobalPosition.DirectionTo(_path.PathPoint.GlobalPosition);
        var distance = GlobalPosition.DistanceTo(_path.PathPoint.GlobalPosition);
        ApplyCentralForce(direction * distance * 10f);

        if (_fireTimer <= 0)
        {
            Fire();
            _fireTimer = FireDelay;
        }
        else
        {
            _fireTimer -= delta;
        }
    }

    public override float GetMaxHealth()
    {
        return 10;
    }

    private void Fire()
    {
        var bullet = ProjectileScene.Instantiate<EnemySquareProjectile>();
        ShapeGame.Instance.AddChild(bullet);
        bullet.GlobalPosition = GlobalPosition;
        var randomStrength = (float)GD.RandRange(1f, 2f);
        var velocityLength = LinearVelocity.Length();
        var impulse = Vector2.Down * velocityLength * 0.5f - LinearVelocity * randomStrength;
        bullet.ApplyCentralImpulse(impulse);
        bullet.ApplyTorqueImpulse(velocityLength * 0.005f);

        var randomOffset = new Vector2((float)GD.RandRange(-3f, 3f), (float)GD.RandRange(-3f, 3f));

        ApplyImpulse(-impulse * 0.3f, randomOffset);

        var sound = SoundManager.Instance.PlayPositionalSound(this, _shotSound);
        sound.PitchScale = Clamp(impulse.Length() / 4000f + 0.75f, 0.7f, 1.3f);
    }

}