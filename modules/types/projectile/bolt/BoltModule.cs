[GlobalClass]
public partial class BoltModule : ProjectileModule
{

    public override IProjectile CreateProjectile()
    {
        return BoltProjectile.Create();
    }

}