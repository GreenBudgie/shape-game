public partial class Explosion : ShapeCast2D
{

    /// <summary>
    /// Represents max radius for some effects like screen shake, particles e.t.c.
    ///
    /// This does not mean that explosion can't use a larger radius. Just effects won't be more intense for
    /// radii larger than this one.
    /// </summary>
    private const float MaxEffectRadius = 1600;

    private static readonly PackedScene Scene = GD.Load<PackedScene>("uid://b676isra84rkm");

    [Signal]
    public delegate void DetonatedEventHandler();

    [Export] private AudioStream _smallExplosionSound = null!;
    [Export] private AudioStream _mediumExplosionSound = null!;
    [Export] private AudioStream _largeExplosionSound = null!;

    private Node2D _initiator = null!;
    private float _radius = 300;
    private float _damage = 1;
    private float _fuseTimeSeconds;

    public static Explosion Create(Node2D initiator)
    {
        var explosion = Scene.Instantiate<Explosion>();
        explosion._initiator = initiator;
        explosion.GlobalPosition = initiator.GlobalPosition;
        ShapeGame.Instance.AddChild(explosion);
        return explosion;
    }

    public override void _Ready()
    {
        Callable.From(() => ExplosionRadiusPreview.Create(this)).CallDeferred();
    }

    public override void _Process(double delta)
    {
        if (IsInstanceValid(_initiator))
        {
            GlobalPosition = _initiator.GlobalPosition;
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

    public Explosion SetRadius(float radius)
    {
        _radius = Max(radius, 0);
        var circleShape = (CircleShape2D)Shape;
        circleShape.Radius = radius;
        return this;
    }

    public float GetRadius() => _radius;
    
    public Explosion SetDamage(float damage)
    {
        _damage = Max(damage, 0);
        return this;
    }

    public float GetDamage() => _damage;

    public float GetEffectRadiusRatio() => Clamp(_radius / MaxEffectRadius, 0, MaxEffectRadius);

    public Explosion SetFuseTimeSeconds(float fuseTimeSeconds)
    {
        _fuseTimeSeconds = fuseTimeSeconds;
        return this;
    }

    public float GetFuseTimeSeconds() => _fuseTimeSeconds;

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
        var strength = GetEffectRadiusRatio() * maxStrength;
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
            var relativeStrength = 1f - Clamp(distance / _radius, 0, 1);
            body.ApplyCentralImpulse(direction * strength * relativeStrength);

            if (body is Enemy enemy)
            {
                enemy.Damage(_damage);
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