[GlobalClass]
public partial class WallProjectile : ProjectileModule
{
    
    public override IProjectile<Node2D> CreateProjectile()
    {
        return MiniSphereProjectile.Create();
    }
    
}
