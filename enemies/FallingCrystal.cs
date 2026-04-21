public partial class FallingCrystal : RigidBody2D, IPlayerCollisionDetector
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

    private static readonly PackedScene Scene = GD.Load<PackedScene>("uid://bu4bb10k0x66d");

    private bool _isCollected;
    private GlowWrapper _glowWrapper = null!;
    private AnimationPlayer _crystalAnimations = null!;

    public static FallingCrystal Spawn(Vector2 globalPosition)
    {
        var crystal = Scene.Instantiate<FallingCrystal>();
        ShapeGame.Instance.AddChildDeferred(crystal);

        crystal.GlobalPosition = globalPosition;
        var randomStrength = (float)GD.RandRange(750f, 1500f);
        var randomAngle = GD.RandRange(5 * Pi / 4, 7 * Pi / 4);
        var randomDirection = Vector2.FromAngle((float)randomAngle);
        var impulse = randomDirection * randomStrength;
        crystal.ApplyCentralImpulse(impulse);

        return crystal;
    }

    public override void _Ready()
    {
        _glowWrapper = GetNode<GlowWrapper>("Glow");
        ResetGlowToMin();

        _crystalAnimations = GetNode<AnimationPlayer>("CrystalAnimations");

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

    public void CollideWithPlayer(Player player)
    {
        Collect();
    }

    private void ApplyMagnet()
    {
        var player = Player.FindPlayer();
        if (player == null)
        {
            LinearDamp = 0;
            SetGravityScale(1);
            ResetGlowToMin();
            return;
        }

        var distanceToPlayerSq = GlobalPosition.DistanceSquaredTo(player.GlobalPosition);
        if (distanceToPlayerSq > MaxMagnetDistanceSq)
        {
            LinearDamp = 0;
            SetGravityScale(1);
            ResetGlowToMin();
            return;
        }

        var direction = GlobalPosition.DirectionTo(player.GlobalPosition);

        if (_isCollected)
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

        _glowWrapper.SetStrength(Lerp(MinGlowStrength, MaxGlowStrength, distancePercent));
        _glowWrapper.SetRadius(Lerp(MinGlowRadius, MaxGlowRadius, distancePercent));
    }

    private void ResetGlowToMin()
    {
        _glowWrapper.SetStrength(MinGlowStrength);
        _glowWrapper.SetRadius(MinGlowRadius);
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
        }
    }

    private void Collect()
    {
        _isCollected = true;
        CollisionLayer = 0;
        CollisionMask = 0;

        var fadeOutTween = _glowWrapper.CreateTween();
        var setColorAction = _glowWrapper.SetColor;
        var finalGlowColor = _glowWrapper.GetColor();
        finalGlowColor.A = 0;
        fadeOutTween.TweenMethod(Callable.From(setColorAction), _glowWrapper.GetColor(), finalGlowColor, 0.15);

        _crystalAnimations.Play("collect");
        _crystalAnimations.AnimationFinished += _ => QueueFree();

        CrystalManager.Instance.CollectCrystal();
    }
}