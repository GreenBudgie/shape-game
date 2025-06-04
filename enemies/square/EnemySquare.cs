public partial class EnemySquare : Enemy
{

    private static readonly PackedScene BulletScene = GD.Load<PackedScene>("uid://bhj8dgeytmpxx");
    private static readonly PackedScene PathScene = GD.Load<PackedScene>("uid://b1ehfaspqd28s");

    [Export] private AudioStream _shotSound = null!;
    [Export] private AudioStream _damageSound = null!;
    [Export] private AudioStream _destroySound = null!;

    private const double FireDelay = 1;
    private const float MaxHealth = 10;

    private double _fireTimer = FireDelay;

    private EnemySquarePath _path = null!;

    private float _health = MaxHealth;

    private Glow _glow = null!;

    private AnimationPlayer _animationPlayer = null!;
    
    private bool _isDestroyed = false;

    public override void _Ready()
    {
        _animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        
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
        if (_isDestroyed)
        {
            return;
        }
        
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

    public override void Damage()
    {
        if (_isDestroyed)
        {
            return;
        }
        
        _health -= 1;
        var hpPercent = Clamp(_health / MaxHealth, 0f, 1f);
        var dangerLevel = 1f - hpPercent;

        if (_health <= 0)
        {
            Destroy();
            return;
        }
        
        var sound = SoundManager.Instance.PlayPositionalSound(this, _damageSound);
        sound.PitchScale = Lerp(0.75f, 1.25f, dangerLevel);

        _glow
            .SetRadius(40f * dangerLevel)
            .SetStrength(2f * dangerLevel)
            .SetPulseRadiusDelta(20f * dangerLevel)
            .SetPulseStrengthDelta(dangerLevel)
            .SetPulsesPerSecond(1f + dangerLevel * (3f - 1f));
        
        _animationPlayer.Play("damage");
    }

    private void Fire()
    {
        var bullet = BulletScene.Instantiate<EnemySquareProjectile>();
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

    private void Destroy()
    {
        _isDestroyed = true;
        CollisionLayer = 0;
        CollisionMask = 0;

        var sound = SoundManager.Instance.PlayPositionalSound(this, _destroySound);

        for (int i = 0; i < 2; i++)
        {
            var crystal = FallingCrystal.CreateFallingCrystal();
            ShapeGame.Instance.CallDeferred(Node.MethodName.AddChild, crystal);
            
            var randomXOffset = (float)GD.RandRange(-80, 80);
            var randomYOffset = (float)GD.RandRange(-80, 80);
            var randomOffset = new Vector2(randomXOffset, randomYOffset);
            crystal.GlobalPosition = GlobalPosition + randomOffset;
            var randomStrength = (float)GD.RandRange(750f, 1500f);
            var randomAngle = GD.RandRange(5 * Pi / 4, 7 * Pi / 4);
            var randomDirection = Vector2.FromAngle((float)randomAngle);
            var impulse = randomDirection * randomStrength;
            crystal.ApplyCentralImpulse(impulse);
            
            var randomTorque = (float)GD.RandRange(-1, 1);
            crystal.ApplyTorqueImpulse(randomTorque);
        }
        
        _glow.DisablePulsing();
        _glow.SetCullOccluded(false);
        var fadeOutTween = _glow.CreateTween();
        var setColorAction = _glow.SetColor;
        var finalGlowColor = _glow.GetColor();
        finalGlowColor.A = 0;
        fadeOutTween.TweenMethod(Callable.From(setColorAction), _glow.GetColor(), finalGlowColor, 0.25);
        
        _animationPlayer.Play("destroy");
        _animationPlayer.AnimationFinished += _ => QueueFree();
    }
}