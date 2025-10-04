[GlobalClass]
public partial class BarrierModule : ProjectileModule
{
    
    public override IProjectile<Node2D> CreateProjectile()
    {
        return Barrier.Create();
    }
    
}
