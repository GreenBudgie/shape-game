public partial class Explosion : ShapeCast2D
{

    private const float MediumExplosionRadius = 600;
    private const float LargeExplosionRadius = 1200;

    private static readonly PackedScene Scene = GD.Load<PackedScene>("uid://b676isra84rkm");

    [Signal]
    public delegate void DetonatedEventHandler();

    [Export] private AudioStream _smallExplosionSound = null!;
    [Export] private AudioStream _mediumExplosionSound = null!;
    [Export] private AudioStream _largeExplosionSound = null!;

    private Node2D _initiator = null!;
    private float _radius = 300;
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
        _radius = radius;
        var circleShape = (CircleShape2D)Shape;
        circleShape.Radius = radius;
        return this;
    }

    public float GetRadius() => _radius;

    public Explosion SetFuseTimeSeconds(float fuseTimeSeconds)
    {
        _fuseTimeSeconds = fuseTimeSeconds;
        return this;
    }

    public float GetFuseTimeSeconds() => _fuseTimeSeconds;

    public void Detonate()
    {
        PlaySound();
        ExplosionEffects.Instance.PlayEffect(this);
        //ScreenShake.Instance.Shake(ShakeStrength.High);
        
        EmitSignalDetonated();

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