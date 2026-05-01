public partial class InitialImpulseComponent : Node, ISpawnableComponent
{
    [Export(PropertyHint.Range, "0,360")] public float Spread { get; private set; }
    [Export] public float SpeedDelta { get; private set; }

    public Node Node => this;

    public void Apply(SpawnableContext context)
    {
        var projectile = context.Spawnable.Node;
        if (projectile is not RigidBody2D rigidBodyProjectile)
        {
            return;
        }

        var initialSpeed = context.CalculateStat<SpeedStat>();
        var randomSpreadDegree = RandomUtils.DeltaRange(0, Spread / 2);
        var randomSpreadDegreeRad = DegToRad(randomSpreadDegree);
        var speed = RandomUtils.DeltaRange(initialSpeed, SpeedDelta);
        var vector = context.Direction * speed;
        var moveVector = vector.Rotated(randomSpreadDegreeRad);
        rigidBodyProjectile.ApplyCentralImpulse(moveVector);
    }
}