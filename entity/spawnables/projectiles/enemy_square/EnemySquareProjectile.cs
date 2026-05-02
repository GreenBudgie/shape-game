public partial class EnemySquareProjectile : BasicRigidBodyProjectile<EnemySquareProjectile>
{

    private const float InitialTorque = 1500; 
    private const float InitialTorqueDelta = 500; 
    
    private static readonly PackedScene Scene = GD.Load<PackedScene>("uid://bhj8dgeytmpxx");

    public override EnemySquareProjectile Node => this;
    
    [Export] private AudioStream _hitWallSound = null!;

    public static EnemySquareProjectile Create()
    {
        return Scene.Instantiate<EnemySquareProjectile>();
    }
    
    public override void Prepare(SpawnableContext context)
    {
        context.Stats.Add(new DamageStat {Damage = 2});
    }

    private bool _torqueApplied;
    
    public override void _IntegrateForces(PhysicsDirectBodyState2D state)
    {
        base._IntegrateForces(state);
        
        if (_torqueApplied)
        {
            return;
        }

        var torque = RandomUtils.RandomSignedDeltaRange(InitialTorque, InitialTorqueDelta);
        ApplyTorqueImpulse(torque);
        _torqueApplied = true;
    }

}