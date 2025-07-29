[GlobalClass]
public partial class BoltModule : ProjectileModule
{

    public override IProjectile<Node2D> CreateProjectile()
    {
        return BoltProjectile.Create();
    }

}