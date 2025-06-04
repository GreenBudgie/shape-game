public partial class FallingCrystal : RigidBody2D
{

    private const float MaxMagnetDistance = 512;
    private const float MaxMagnetDistanceSq = MaxMagnetDistance * MaxMagnetDistance;
    private const float MaxMagnetForce = 400;

    private static readonly PackedScene Scene = GD.Load<PackedScene>("uid://bu4bb10k0x66d");

    public static FallingCrystal CreateFallingCrystal()
    {
        return Scene.Instantiate<FallingCrystal>();
    }
    
    public override void _Ready()
    {
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
            return;
        }
        
        var distanceToPlayerSq = GlobalPosition.DistanceSquaredTo(player.GlobalPosition);
        if (distanceToPlayerSq > MaxMagnetDistanceSq)
        {
            return;
        }

        var distanceToPlayer = Sqrt(distanceToPlayerSq);
        var distancePercent = 1 - distanceToPlayer / MaxMagnetDistance;
        var forceStrength = Ease(distancePercent, 0.25f) * MaxMagnetForce;
        var direction = GlobalPosition.DirectionTo(player.GlobalPosition);
        var forceVector = direction * forceStrength;
        
        ApplyCentralForce(forceVector);
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
