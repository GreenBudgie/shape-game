using System;

public partial class Explosion : ShapeCast2D, ISpawnable
{
    /// <summary>
    /// Represents max radius for some effects like screen shake, particles e.t.c.
    ///
    /// This does not mean that explosion can't use a larger radius. Just effects won't be more intense for
    /// radii larger than this one.
    /// </summary>
    private const float MaxEffectRadius = 1600;

    private static readonly PackedScene Scene = GD.Load<PackedScene>("uid://b676isra84rkm");

    [Export] private AudioStream _smallExplosionSound = null!;
    [Export] private AudioStream _mediumExplosionSound = null!;
    [Export] private AudioStream _largeExplosionSound = null!;
    
    public event Action<EntityDamagedEvent>? EntityDamaged;
    public event Action? Removed;
    public SpawnableContext Context { get; set; } = null!;
    
    public bool IsRemoving { get; private set; }

    public bool IsInstant { get; private set; }
    
    private float _radius;
    private float _damage;
    private float _fuseTimeSeconds;

    public static Explosion Create()
    {
        return Scene.Instantiate<Explosion>();
    }

    public void Prepare(SpawnableContext context)
    {
        _radius = Context.GetStat<ExplosionRadiusStat>();
        var circleShape = (CircleShape2D)Shape;
        circleShape.Radius = _radius;
        
        _damage = Context.GetStat<ExplosionDamageStat>();
        
        _fuseTimeSeconds = Context.GetStat<LifetimeStat>();
        IsInstant = _fuseTimeSeconds <= 0;
        ExplosionRadiusPreview.Create(this);
    }

    public void Remove()
    {
        if (IsRemoving)
        {
            return;
        }
        
        IsRemoving = true;
        
        Detonate();
        Removed?.Invoke();
    }

    public override void _Ready()
    {
        if (IsInstant)
        {
            Remove();
        }
    }

    public override void _Process(double delta)
    {
        if (IsInstanceValid(Context.Source))
        {
            GlobalPosition = Context.Source.GlobalPosition;
        }
        else
        {
            // Instantly detonate if initiator was freed (invalidated)
            Remove();
            return;
        }
        
        if (_fuseTimeSeconds <= 0)
        {
            return;
        }
        
        _fuseTimeSeconds = Max((float)(_fuseTimeSeconds - delta), 0);
        if (_fuseTimeSeconds <= 0)
        {
            Remove();
        }
    }

    public float GetRadius() => _radius;

    public float GetDamage() => _damage;
    
    public float GetFuseTimeSeconds() => _fuseTimeSeconds;

    public float GetEffectRadiusRatio() => Clamp(_radius / MaxEffectRadius, 0, MaxEffectRadius);

    private void Detonate()
    {
        PlaySound();
        ShakeScreen();
        ExplosionEffects.Instance.PlayEffect(this);
        ExplosionParticles.Create(this);

        ForceShapecastUpdate();
        if (!IsColliding())
        {
            QueueFree();
            return;
        }

        const float maxStrength = 6000f;
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

            if (body is Entity entity)
            {
                entity.HealthController.Damage(DamageSource.Explosion, _damage);
                var entityDamagedEvent = new EntityDamagedEvent(entity, _damage);
                EntityDamaged?.Invoke(entityDamagedEvent);
            }
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