public partial class EnemyDestroyParticles : GpuParticles2D
{
    private static readonly PackedScene Scene = GD.Load<PackedScene>("uid://ch1pv0ey6wg5u");

    private Enemy _enemy = null!;

    public static EnemyDestroyParticles Create(Enemy enemy)
    {
        var node = Scene.Instantiate<EnemyDestroyParticles>();
        node._enemy = enemy;
        node.GlobalPosition = enemy.GlobalPosition;
        ShapeGame.Instance.AddChild(node);
        return node;
    }

    public override void _Ready()
    {
        // Delay for one frame for enemy velocity to update
        GetTree().Connect(
            SceneTree.SignalName.PhysicsFrame,
            Callable.From(SpawnParticles),
            (uint)ConnectFlags.OneShot
        );
    }

    private void SpawnParticles()
    {
        const float particlesPerPixel = 0.15f;
        Amount = RoundToInt(Sqrt(_enemy.AreaRect.Area) * particlesPerPixel);

        var material = (ParticleProcessMaterial)ProcessMaterial;

        const float velocitySpreadFactor = 0.08f;
        const float minVelocity = 300f;
        const float velocityDelta = 150f;
        const float maxVelocity = 2000f;
        var normalizedEnemyVelocityDirection = _enemy.LinearVelocity.Normalized();
        var enemyVelocityLength = _enemy.LinearVelocity.Length();
        material.Spread = 180 - Clamp(enemyVelocityLength * velocitySpreadFactor, 0, 180);
        material.Direction = new Vector3(normalizedEnemyVelocityDirection.X, normalizedEnemyVelocityDirection.Y, 0);
        var velocity = Clamp(enemyVelocityLength, minVelocity, maxVelocity);
        material.InitialVelocityMin = minVelocity;
        material.InitialVelocityMax = Clamp(velocity, minVelocity + velocityDelta, maxVelocity);

        var halfRectSize = _enemy.AreaRect.Size / 2f;
        material.EmissionBoxExtents = new Vector3(halfRectSize.X, halfRectSize.Y, 1);
        Modulate = _enemy.Color;

        Finished += QueueFree;
        Restart();
    }
}