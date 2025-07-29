[GlobalClass]
public partial class ExtraFireRateModule : ModifierModule
{

    public override void Apply(IProjectile<Node2D> projectile)
    {
        var fireRateComponent = projectile.GetComponentOrNull<FireRateComponent>();
        if (fireRateComponent != null)
        {
            fireRateComponent.FireRate *= 0.5f;
        }
    }

}