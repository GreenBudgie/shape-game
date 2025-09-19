[GlobalClass]
public partial class MiniSphereModule : ProjectileModule
{

    public override IProjectile<Node2D> CreateProjectile()
    {
        return MiniSphereProjectile.Create();
    }

}