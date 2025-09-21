using System.Linq;

public partial class MineProjectile : RigidBody2D, IProjectile<MineProjectile>
{
    private static readonly Color OversaturatedWhite = new(4, 4, 4);

    private static readonly PackedScene Scene = GD.Load<PackedScene>("uid://coo3tnuma02fs");

    [Export] private AudioStream _shotSound = null!;

    [Export] private AudioStream _wallHitSound = null!;

    [Export] private AudioStream _beepSound = null!;

    public MineProjectile Node => this;

    public static MineProjectile Create()
    {
        return Scene.Instantiate<MineProjectile>();
    }

    private ShotContext _context = null!;
    private Explosion? _explosion;
    private Glow _glow = null!;
    private Sprite2D _sprite = null!;

    public void Prepare(ShotContext context)
    {
        _context = context;
    }

    public override void _Ready()
    {
        SoundManager.Instance.PlayPositionalSound(this, _shotSound).RandomizePitchOffset(0.1f);
        GetTree().CreateTimer(0.5f).Timeout += Fuse;
        BodyEntered += HandleBodyEntered;

        _sprite = GetNode<Sprite2D>("MineSprite");
        _glow = Glow.AddGlow(_sprite)
            .SetColor(ColorScheme.Red)
            .SetStrength(2)
            .SetRadius(0);
    }

    private bool _torqueApplied;

    public override void _IntegrateForces(PhysicsDirectBodyState2D state)
    {
        if (_torqueApplied)
        {
            return;
        }

        const float initialTorque = 500;
        const float initialTorqueDelta = 200;
        var torque = RandomUtils.RandomSignedDeltaRange(initialTorque, initialTorqueDelta);
        ApplyTorqueImpulse(torque);
        _torqueApplied = true;
    }

    private void Fuse()
    {
        const float fuseTime = 1f;

        _explosion = Explosion.Create(this)
            .SetDamage(_context.CalculateStat<ExplosionDamageStat>())
            .SetRadius(_context.CalculateStat<ExplosionRadiusStat>())
            .SetFuseTimeSeconds(fuseTime);

        _explosion.Connect(Explosion.SignalName.Detonated, Callable.From(QueueFree));

        CreateTween()
            .TweenProperty(_glow, Glow.RadiusProperty, 100, fuseTime)
            .SetTrans(Tween.TransitionType.Quad)
            .SetEase(Tween.EaseType.In);

        const float beepDuration = 0.1f;
        const int beeps = 4;
        const float pitchIncreasePerBeep = 0.1f;
        var beepDelays = new[] { 0.2f, 0.1f, 0.05f, 0.025f };
        var initialDelay = fuseTime - beepDelays.Sum() - beepDuration * beeps * 2;
        var beepScale = new Vector2(1.2f, 1.2f);

        var tween = CreateTween();
        for (var i = 0; i < beeps; i++)
        {
            var delay = i == 0 ? initialDelay : beepDelays[i];
            var pitch = 1 + pitchIncreasePerBeep * i;
            tween.TweenProperty(_sprite, ScaleProperty, beepScale, beepDuration)
                .SetDelay(delay);
            tween.Parallel().TweenProperty(_sprite, ModulateProperty, OversaturatedWhite, beepDuration);
            tween.TweenCallback(Callable.From(() => PlayBeepSound(pitch)));
            tween.TweenProperty(_sprite, ScaleProperty, Vector2.One, beepDuration);
            tween.Parallel().TweenProperty(_sprite, ModulateProperty, Colors.White, beepDuration);
        }
    }

    private void PlayBeepSound(float pitch)
    {
        var sound = SoundManager.Instance.PlayPositionalSound(this, _beepSound);
        sound.PitchScale = pitch;
        sound.RandomizePitchOffset(0.02f);
    }

    private void HandleBodyEntered(Node body)
    {
        if (body is not CollisionObject2D collisionObject2D)
        {
            return;
        }

        if (collisionObject2D.HasCollisionLayer(CollisionLayers.LevelWalls))
        {
            SoundManager.Instance.PlayPositionalSound(this, _wallHitSound).RandomizePitchOffset(0.1f);
        }

        if (collisionObject2D.HasCollisionLayer(CollisionLayers.LevelOutsideBoundary))
        {
            QueueFree();
        }
    }
}