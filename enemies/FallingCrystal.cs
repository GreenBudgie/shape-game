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

    private static readonly Color GlowColor = ColorScheme.Yellow;

    private bool _isCollected;
    private Glow _glow = null!;
    private AnimationPlayer _crystalAnimations = null!;

    public static FallingCrystal Create()
    {
        return Scene.Instantiate<FallingCrystal>();
    }

    public override void _Ready()
    {
        var sprite = GetNode<Sprite2D>("Sprite2D");
        _glow = Glow.AddGlow(sprite).SetColor(GlowColor);
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

        _glow.SetStrength(Lerp(MinGlowStrength, MaxGlowStrength, distancePercent));
        _glow.SetRadius(Lerp(MinGlowRadius, MaxGlowRadius, distancePercent));
    }

    private void ResetGlowToMin()
    {
        _glow.SetStrength(MinGlowStrength);
        _glow.SetRadius(MinGlowRadius);
    }

    private void HandleCollision(Node node)
    {
        if (node is not CollisionObject2D collisionObject)
        {
            return;
        }

        if (collisionObject.GetCollisionLayerValue(CollisionLayers.LevelOutsideBoundary))
        {
            QueueFree();
        }
    }

    private void Collect()
    {
        _isCollected = true;
        CollisionLayer = 0;
        CollisionMask = 0;

        _glow.SetCullOccluded(false);
        var fadeOutTween = _glow.CreateTween();
        var setColorAction = _glow.SetColor;
        var finalGlowColor = _glow.GetColor();
        finalGlowColor.A = 0;
        fadeOutTween.TweenMethod(Callable.From(setColorAction), _glow.GetColor(), finalGlowColor, 0.15);

        _crystalAnimations.Play("collect");
        _crystalAnimations.AnimationFinished += _ => QueueFree();

        CrystalManager.Instance.CollectCrystal();
    }
}