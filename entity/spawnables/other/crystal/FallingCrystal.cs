using System;

public partial class FallingCrystal : RigidBody2D, ISpawnable
{
    private const float InitialTorque = 500;
    private const float InitialTorqueDelta = 200;

    private const float MaxMagnetDistance = 512;
    private const float MaxMagnetDistanceSq = MaxMagnetDistance * MaxMagnetDistance;
    private const float MaxMagnetForce = 800;
    private const float MinGlowStrength = 1f;
    private const float MaxGlowStrength = 2f;
    private const float MinGlowRadius = 20f;
    private const float MaxGlowRadius = 40f;
    private const float MaxLinearDamp = 4;
    private const float CollectedDamp = 10;
    private const float CollectedMagnetForce = 3000;
    private const float CollectDuration = 0.2f;

    private static readonly PackedScene Scene = GD.Load<PackedScene>("uid://bu4bb10k0x66d");

    private static readonly Color GlowColor = ColorScheme.Yellow;
    private static readonly Callable CollectScaleEase = BezierEase(Vector2.Zero, new Vector2(0.5f, 1f));
    private static readonly Callable CollectAlphaEase = BezierEase(new Vector2(1f / 3f, 0.255997f), new Vector2(0.5f, 0.9315711f));
    private static readonly Callable CollectDissolveEase = BezierEase(new Vector2(1f / 6f, 0.4266084f), new Vector2(0.506401f, 0.783497f));
    
    public event Action<EntityDamagedEvent>? EntityDamaged;
    public event Action? Removed;
    public SpawnableContext Context { get; set; } = null!;
    public bool IsRemoving { get; private set; }
    
    private Sprite2D _sprite = null!;
    private VisualEffects _effects = null!;
    private float _glowProgress = -1f;
    
    public static FallingCrystal Spawn(Vector2 globalPosition)
    {
        var crystal = Scene.Instantiate<FallingCrystal>();
        
        var randomSpeed = (float)GD.RandRange(750f, 1500f);
        var randomAngle = GD.RandRange(5 * Pi / 4, 7 * Pi / 4);
        var randomDirection = Vector2.FromAngle((float)randomAngle);

        var context = new SpawnableContext(new SingleSpawnableProducer(crystal))
        {
            Position = globalPosition,
            Direction = randomDirection,
        };

        context.AddStat(new SpeedStat { Value = randomSpeed });
        context.AddStat(new LifetimeStat { Value = 10f });

        context.ForceSpawn();

        return crystal;
    }

    public override void _Ready()
    {
        _sprite = GetNode<Sprite2D>("Sprite2D");
        _effects = VisualEffects.Attach(_sprite);
        _effects.GlowEnabled = true;
        _effects.GlowColor = GlowColor;
        UpdateGlow(0f);
        
        var pointer = new PointerArea(this)
        {
            PointerColor = ColorScheme.Yellow,
            ShowOnlyWhenMovingIn = false,
        };
        AddChild(pointer);

        BodyEntered += HandleCollision;
    }

    private bool _torqueApplied;

    public override void _IntegrateForces(PhysicsDirectBodyState2D state)
    {
        if (_torqueApplied)
        {
            return;
        }

        var torque = RandomUtils.RandomSignedDeltaRange(InitialTorque, InitialTorqueDelta);
        ApplyTorqueImpulse(torque);
        _torqueApplied = true;
    }

    public override void _PhysicsProcess(double delta)
    {
        ApplyMagnet();
    }

    public void Remove()
    {
        if (IsRemoving)
        {
            return;
        }
        
        IsRemoving = true;
        CollisionLayer = 0;
        CollisionMask = 0;

        _effects.DissolveEnabled = true;
        var tween = CreateTween().SetParallel();
        tween.TweenScale(_sprite, 0.25f, CollectDuration).SetCustomInterpolator(CollectScaleEase);
        tween.FadeOut(_sprite, CollectDuration).SetCustomInterpolator(CollectAlphaEase);
        tween.TweenDissolveValue(_effects, 0f, CollectDuration)
            .SetCustomInterpolator(CollectDissolveEase);
        tween.TweenGlowFadeOut(_effects, 0.15f);
        tween.Finished += QueueFree;
        
        Removed?.Invoke();
    }

    private void ApplyMagnet()
    {
        var player = Player.FindPlayer();
        if (player == null)
        {
            LinearDamp = 0;
            SetGravityScale(1);
            UpdateGlow(0f);
            return;
        }

        var distanceToPlayerSq = GlobalPosition.DistanceSquaredTo(player.GlobalPosition);
        if (distanceToPlayerSq > MaxMagnetDistanceSq)
        {
            LinearDamp = 0;
            SetGravityScale(1);
            UpdateGlow(0f);
            return;
        }

        var direction = GlobalPosition.DirectionTo(player.GlobalPosition);

        if (IsRemoving)
        {
            LinearDamp = CollectedDamp;
            SetGravityScale(0);
            ApplyCentralForce(direction * CollectedMagnetForce);
        }

        var distanceToPlayer = Sqrt(distanceToPlayerSq);
        var distancePercent = 1 - distanceToPlayer / MaxMagnetDistance;
        var forceStrength = Ease(distancePercent, 0.25f) * MaxMagnetForce;
        var forceVector = direction * forceStrength;

        LinearDamp = Lerp(0, MaxLinearDamp, distancePercent);
        SetGravityScale(distancePercent);
        ApplyCentralForce(forceVector);

        UpdateGlow(distancePercent);
    }

    private void UpdateGlow(float progress)
    {
        if (IsEqualApprox(_glowProgress, progress))
        {
            return;
        }
        _glowProgress = progress;
        _effects.GlowStrength = Lerp(MinGlowStrength, MaxGlowStrength, progress);
        _effects.GlowRadius = Lerp(MinGlowRadius, MaxGlowRadius, progress);
    }

    private static Callable BezierEase(Vector2 first, Vector2 second)
    {
        var curve = new Animation();
        var track = curve.AddTrack(Animation.TrackType.Bezier);
        curve.BezierTrackInsertKey(track, 0, 0f, outHandle: first);
        curve.BezierTrackInsertKey(track, 1, 1f, inHandle: second - Vector2.One);
        return Callable.From<float, float>(progress => curve.BezierTrackInterpolate(track, progress));
    }

    private void HandleCollision(Node node)
    {
        if (node is not CollisionObject2D collisionObject)
        {
            return;
        }

        if (collisionObject.HasCollisionLayer(CollisionLayers.LevelOutsideBoundary))
        {
            QueueFree();
            return;
        }

        if (collisionObject is Player)
        {
            Collect();
        }
    }

    private void Collect()
    {
        Remove();
        CrystalManager.Instance.CollectCrystal();
    }
}