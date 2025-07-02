public partial class FallingCrystal : RigidBody2D
{

    private const float MaxMagnetDistance = 512;
    private const float MaxMagnetDistanceSq = MaxMagnetDistance * MaxMagnetDistance;
    private const float MaxMagnetForce = 400;
    private const float MinGlowStrength = 1f;
    private const float MaxGlowStrength = 2f;
    private const float MinGlowRadius = 20f;
    private const float MaxGlowRadius = 40f;

    private static readonly PackedScene Scene = GD.Load<PackedScene>("uid://bu4bb10k0x66d");

    [Export] private Color _glowColor;

    private Glow _glow = null!;

    public static FallingCrystal CreateFallingCrystal()
    {
        return Scene.Instantiate<FallingCrystal>();
    }

    public override void _Ready()
    {
        var sprite = GetNode<Sprite2D>("Sprite2D");
        _glow = Glow.AddGlow(sprite).SetColor(_glowColor);
        ResetGlowToMin();

        BodyEntered += HandleCollision;
    }

    public override void _PhysicsProcess(double delta)
    {
        ApplyMagnet();
    }

    private void ApplyMagnet()
    {
        var player = Player.FindPlayer();
        if (player == null)
        {
            SetGravityScale(1);
            ResetGlowToMin();
            return;
        }

        var distanceToPlayerSq = GlobalPosition.DistanceSquaredTo(player.GlobalPosition);
        if (distanceToPlayerSq > MaxMagnetDistanceSq)
        {
            SetGravityScale(1);
            ResetGlowToMin();
            return;
        }

        var distanceToPlayer = Sqrt(distanceToPlayerSq);
        var distancePercent = 1 - distanceToPlayer / MaxMagnetDistance;
        var forceStrength = Ease(distancePercent, 0.25f) * MaxMagnetForce;
        var direction = GlobalPosition.DirectionTo(player.GlobalPosition);
        var forceVector = direction * forceStrength;

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

        if (node is Player)
        {
            Collect();
        }
    }

    private void Collect()
    {
        QueueFree();
    }

}