using System;
using System.Linq;

public partial class MineProjectile : RigidBody2D, ISpawnable
{
    
    private const float FuseTime = 1f;
    
    private static readonly Color OversaturatedWhite = new(4, 4, 4);

    private static readonly PackedScene Scene = GD.Load<PackedScene>("uid://coo3tnuma02fs");

    [Export] private AudioStream _shotSound = null!;

    [Export] private AudioStream _wallHitSound = null!;

    [Export] private AudioStream _beepSound = null!;

    public static MineProjectile Create()
    {
        return Scene.Instantiate<MineProjectile>();
    }
    
    public event Action<EntityDamagedEvent>? EntityDamaged;
    public event Action? Removed;
    public SpawnableContext Context { get; set; } = null!;

    private GlowWrapper _glowWrapper = null!;
    private Sprite2D _sprite = null!;

    public void Setup(SpawnableContext context)
    {
        var component = this.AddComponentIfPossible(ComponentTypeRegistry.RemoveExplosion);
        if (component is not RemoveExplosionComponent explosionComponent)
        {
            return;
        }

        explosionComponent.FuseTime = FuseTime;
    }

    public override void _Ready()
    {
        SoundManager.Instance.PlayPositionalSound(this, _shotSound).RandomizePitchOffset(0.1f);
        BodyEntered += HandleBodyEntered;

        _glowWrapper = GetNode<GlowWrapper>("Glow")
            .SetColor(ColorScheme.Red)
            .SetStrength(2)
            .SetRadius(0);
        _sprite = _glowWrapper.GetNode<Sprite2D>("MineSprite");
    }

    private bool _isFused;

    // Fuse instead of removing instantly, that's a feature of mine
    public void Remove()
    {
        Fuse();
        Removed?.Invoke();
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
        if (_isFused)
        {
            return;
        }

        _isFused = true;
        
        var timer = new Timer();
        timer.OneShot = true;
        timer.Autostart = true;
        timer.WaitTime = FuseTime;
        timer.Timeout += QueueFree;
        AddChild(timer);
        
        CreateTween()
            .TweenProperty(_glowWrapper, IGlow.RadiusProperty, 100, FuseTime)
            .SetTrans(Tween.TransitionType.Quad)
            .SetEase(Tween.EaseType.In);

        const float beepDuration = 0.1f;
        const int beeps = 4;
        const float pitchIncreasePerBeep = 0.1f;
        var beepDelays = new[] { 0.2f, 0.1f, 0.05f, 0.025f };
        var initialDelay = FuseTime - beepDelays.Sum() - beepDuration * beeps * 2;
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