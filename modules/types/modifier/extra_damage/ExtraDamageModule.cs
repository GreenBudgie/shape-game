[GlobalClass]
public partial class ExtraDamageModule : ModifierModule
{
    
    public override void Apply(ShotContext context)
    {
        var projectile = context.Projectile.Node;
        TrailParticles.Create(projectile)
            .WithTexture(ParticleTextures.Triangle)
            .WithScale(0.4f, 0.1f)
            .Color(ColorScheme.Red)
            .Spawn();
    }
    
}