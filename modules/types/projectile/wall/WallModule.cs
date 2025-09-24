[GlobalClass]
public partial class WallModule : ProjectileModule
{
    
    public override IProjectile<Node2D> CreateProjectile()
    {
        return Wall.Create();
    }
    
}
