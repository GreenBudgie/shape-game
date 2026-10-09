public partial class InitialImpulseComponent : Component
{
    
    public float Spread { get; set; }
    public float SpeedDelta { get; set; }

    public override ComponentType Type => ComponentTypeRegistry.InitialImpulse;

    public override void Apply(SpawnableContext context)
    {
        if (Spawnable.Node is not RigidBody2D rigidBodyProjectile)
        {
            return;
        }

        var initialSpeed = context.CalculateStat<SpeedStat>();
        if (IsEqualApprox(initialSpeed, 0))
        {
            return;
        }
        
        var randomSpreadDegree = RandomUtils.DeltaRange(0, Spread / 2);
        var randomSpreadDegreeRad = DegToRad(randomSpreadDegree);
        var speed = RandomUtils.DeltaRange(initialSpeed, SpeedDelta);
        var vector = context.Direction * speed;
        var moveVector = vector.Rotated(randomSpreadDegreeRad);
        rigidBodyProjectile.ApplyCentralImpulse(moveVector);
    }
    
}