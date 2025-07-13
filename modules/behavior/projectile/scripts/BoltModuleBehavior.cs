[GlobalClass]
public partial class BoltModuleBehavior : ProjectileModuleBehavior
{

    public override ProjectileData CreateProjectile()
    {
        return new ProjectileData(BoltProjectile.Create());
    }

}