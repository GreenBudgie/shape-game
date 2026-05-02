public partial class EnemyRectangleProjectile : BasicRigidBodyProjectile<EnemyRectangleProjectile>
{

    private static readonly PackedScene Scene = GD.Load<PackedScene>("uid://b1rpikysssmoh");
    
    private EnemyRectangle _owner = null!;
    
    public override EnemyRectangleProjectile Node => this;
    
    public static EnemyRectangleProjectile Create()
    {
        return Scene.Instantiate<EnemyRectangleProjectile>();
    }

    public override void _Ready()
    {
        base._Ready();

        EnemyRectangleProjectileParticles.Create(this);
    }
    
    public override void Prepare(SpawnableContext context)
    {
        base.Prepare(context);
        context.Stats.Add(new DamageStat {Damage = 5});
    }

    private bool _torqueApplied;

    public override void _IntegrateForces(PhysicsDirectBodyState2D state)
    {
        base._IntegrateForces(state);
        
        if (_torqueApplied)
        {
            return;
        }

        const float initialTorque = 4000;
        const float initialTorqueDelta = 1000;
        var torque = RandomUtils.RandomSignedDeltaRange(initialTorque, initialTorqueDelta);
        ApplyTorqueImpulse(torque);
        _torqueApplied = true;
    }
    
}
