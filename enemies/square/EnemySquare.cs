public partial class EnemySquare : Enemy
{

    private static readonly PackedScene BulletScene = GD.Load<PackedScene>("uid://bhj8dgeytmpxx");
    private static readonly PackedScene PathScene = GD.Load<PackedScene>("uid://b1ehfaspqd28s");

    private const double FireDelay = 1;

    private double _fireTimer = FireDelay;

    private EnemySquarePath _path = null!;

    private float _health = 10;

    private Glow _glow = null!;

    public override void _Ready()
    {
        _path = PathScene.Instantiate<EnemySquarePath>();
        ShapeGame.Instance.CallDeferred(Node.MethodName.AddChild, _path);

        var sprite = GetNode<Sprite2D>("Sprite2D");
        _glow = Glow.AddGlow(sprite)
            .SetColor(new Color("39A0ED"))
            .SetStrength(0)
            .SetRadius(0)
            .EnablePulsing();
    }

    public override void _Process(double delta)
    {
        var direction = GlobalPosition.DirectionTo(_path.PathPoint.GlobalPosition);
        var distance = GlobalPosition.DistanceTo(_path.PathPoint.GlobalPosition);
        ApplyCentralForce(direction * distance * 20);

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

    public override void Damage()
    {
        _health -= 1;

        if (_health <= 0)
        {
            QueueFree();
        }

        var hpPercent = Clamp(_health / 10f, 0f, 1f);

        // Invert percent so lower HP = stronger effect
        var dangerLevel = 1f - hpPercent;

        // Example tunable parameters

        _glow
            .SetRadius(20f * dangerLevel)
            .SetStrength(2f * dangerLevel)
            .SetPulseRadiusDelta(10f * dangerLevel)
            .SetPulseStrengthDelta(0.5f * dangerLevel)
            .SetPulsesPerSecond(1f + dangerLevel * (3f - 1f));
    }

    private void Fire()
    {
        var bullet = BulletScene.Instantiate<EnemySquareProjectile>();
        ShapeGame.Instance.AddChild(bullet);
        bullet.GlobalPosition = GlobalPosition;
        var randomStrength = (float)GD.RandRange(2f, 3f);
        var velocityLength = LinearVelocity.Length();
        var impulse = Vector2.Down * velocityLength * 0.5f - LinearVelocity * randomStrength;
        bullet.ApplyCentralImpulse(impulse);
        bullet.ApplyTorqueImpulse(velocityLength * 0.005f);

        var randomOffset = new Vector2((float)GD.RandRange(-3f, 3f), (float)GD.RandRange(-3f, 3f));

        ApplyImpulse(-impulse * 0.3f, randomOffset);
    }
}