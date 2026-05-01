public partial class Explosion : ShapeCast2D, ISpawnable<Explosion>
{

    /// <summary>
    /// Represents max radius for some effects like screen shake, particles e.t.c.
    ///
    /// This does not mean that explosion can't use a larger radius. Just effects won't be more intense for
    /// radii larger than this one.
    /// </summary>
    private const float MaxEffectRadius = 1600;

    private static readonly PackedScene Scene = GD.Load<PackedScene>("uid://b676isra84rkm");

    public Explosion Node => this;
    
    [Signal]
    public delegate void DetonatedEventHandler();

    [Export] private AudioStream _smallExplosionSound = null!;
    [Export] private AudioStream _mediumExplosionSound = null!;
    [Export] private AudioStream _largeExplosionSound = null!;

    private SpawnableContext _context = null!;
    private float _radius;
    private float _damage;
    private float _fuseTimeSeconds;

    public static Explosion Create(Node2D initiator)
    {
        return Scene.Instantiate<Explosion>();
    }

    public void Prepare(SpawnableContext context)
    {
        _context = context;
    }

    public void Remove()
    {
        Detonate();
    }

    public override void _Ready()
    {
        Callable.From(() => ExplosionRadiusPreview.Create(this)).CallDeferred();

        _radius = _context.CalculateStat<ExplosionRadiusStat>();
        _damage = _context.CalculateStat<ExplosionDamageStat>();
        _fuseTimeSeconds = _context.CalculateStat<LifetimeStat>();
    }

    public override void _Process(double delta)
    {
        if (IsInstanceValid(_context.Source))
        {
            GlobalPosition = _context.Source.GlobalPosition;
        }
        else
        {
            // Instantly detonate if initiator was freed (invalidated)
            Detonate();
            return;
        }
        
        if (_fuseTimeSeconds <= 0)
        {
            return;
        }
        
        _fuseTimeSeconds = Max((float)(_fuseTimeSeconds - delta), 0);
        if (_fuseTimeSeconds <= 0)
        {
            Detonate();
        }
    }

    public float GetRadius() => _radius;

    public float GetDamage() => _damage;
    
    public float GetFuseTimeSeconds() => _fuseTimeSeconds;

    public float GetEffectRadiusRatio() => Clamp(_radius / MaxEffectRadius, 0, MaxEffectRadius);

    public void Detonate()
    {
        PlaySound();
        ShakeScreen();
        ExplosionEffects.Instance.PlayEffect(this);
        ExplosionParticles.Create(this);
        
        EmitSignalDetonated();

        ForceShapecastUpdate();
        if (!IsColliding())
        {
            QueueFree();
            return;
        }

        const float maxStrength = 2000f;
        var strength = Clamp(Sqrt(GetEffectRadiusRatio()), 0.1f, 1) * maxStrength;
        for (var i = 0; i < GetCollisionCount(); i++)
        {
            var collider = GetCollider(i);
            if (collider is not RigidBody2D body)
            {
                continue;
            }

            var bodyPosition = body.ToGlobal(body.CenterOfMass);
            var direction = GlobalPosition.DirectionTo(bodyPosition);
            var distance = GlobalPosition.DistanceTo(bodyPosition);
            var relativeStrength = 1f - Clamp(Sqrt(distance / _radius), 0.1f, 1);
            body.ApplyCentralImpulse(direction * (strength * relativeStrength));

            HealthController.GetHealthControllerIfExists(body)?.Damage(_damage);
        }

        QueueFree();
    }

    private void PlaySound()
    {
        var ratio = GetEffectRadiusRatio();
        var sound = ratio switch
        {
            < 0.33f => _smallExplosionSound,
            < 0.66f => _mediumExplosionSound,
            _ => _largeExplosionSound
        };

        SoundManager.Instance.PlayPositionalSound(this, sound)
            .RandomizePitchOffset(0.1f);
    }

    private void ShakeScreen()
    {
        ScreenShake.Instance.Shake(ShakeStrength.FromRatio(GetEffectRadiusRatio()));
    }

}