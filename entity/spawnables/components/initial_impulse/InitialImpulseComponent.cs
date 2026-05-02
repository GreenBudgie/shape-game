public partial class InitialImpulseComponent : Node, ISpawnableComponent
{
    [Export(PropertyHint.Range, "0,360")] public float Spread { get; private set; }
    [Export] public float SpeedDelta { get; private set; }

    public Node Node => this;

    private SpawnableContext _context = null!;
    
    public void Prepare(SpawnableContext context)
    {
        _context = context;
    }

    public override void _Ready()
    {
        var projectile = _context.Spawnable.Node;
        if (projectile is not RigidBody2D rigidBodyProjectile)
        {
            return;
        }

        var initialSpeed = _context.CalculateStat<SpeedStat>();
        if (IsEqualApprox(initialSpeed, 0))
        {
            return;
        }
        
        var randomSpreadDegree = RandomUtils.DeltaRange(0, Spread / 2);
        var randomSpreadDegreeRad = DegToRad(randomSpreadDegree);
        var speed = RandomUtils.DeltaRange(initialSpeed, SpeedDelta);
        var vector = _context.Direction * speed;
        var moveVector = vector.Rotated(randomSpreadDegreeRad);
        rigidBodyProjectile.ApplyCentralImpulse(moveVector);
    }
}