[GlobalClass]
public partial class MineModule : ProjectileModule
{

    public override IProjectile<Node2D> CreateProjectile()
    {
        return MineProjectile.Create();
    }

}