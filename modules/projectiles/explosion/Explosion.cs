public partial class Explosion : ShapeCast2D
{

    private const float MediumExplosionRadius = 600;
    private const float LargeExplosionRadius = 1200;

    private static readonly PackedScene Scene = GD.Load<PackedScene>("uid://b676isra84rkm");

    [Export] private AudioStream _smallExplosionSound = null!;
    [Export] private AudioStream _mediumExplosionSound = null!;
    [Export] private AudioStream _largeExplosionSound = null!;

    private float _radius = 300;
    private float _fuseTimeSeconds;

    public static Explosion Create(Vector2 globalPosition)
    {
        var explosion = Scene.Instantiate<Explosion>();
        explosion.GlobalPosition = globalPosition;
        ShapeGame.Instance.AddChild(explosion);
        return explosion;
    }

    public Explosion Radius(float radius)
    {
        _radius = radius;
        var circleShape = (CircleShape2D)Shape;
        circleShape.Radius = radius;
        return this;
    }

    public float GetRadius() => _radius;

    public Explosion FuseTimeSeconds(float fuseTimeSeconds)
    {
        _fuseTimeSeconds = fuseTimeSeconds;
        return this;
    }

    public void Detonate()
    {
        PlaySound();
        ExplosionEffects.Instance.PlayEffect(this);
        ScreenShake.Instance.Shake(ShakeStrength.High);

        ForceShapecastUpdate();
        if (!IsColliding())
        {
            QueueFree();
            return;
        }

        for (var i = 0; i < GetCollisionCount(); i++)
        {
            var collider = GetCollider(i);
            if (collider is not RigidBody2D body)
            {
                continue;
            }

            var direction = GlobalPosition.DirectionTo(body.ToGlobal(body.CenterOfMass));
            var strength = 200;
            body.ApplyCentralImpulse(direction * strength);
        }

        QueueFree();
    }

    private void PlaySound()
    {
        var sound = _radius switch
        {
            < MediumExplosionRadius => _smallExplosionSound,
            < LargeExplosionRadius => _mediumExplosionSound,
            _ => _largeExplosionSound
        };

        SoundManager.Instance.PlayPositionalSound(this, sound)
            .RandomizePitchOffset(0.1f);
    }

}