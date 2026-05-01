[GlobalClass]
public partial class ExtraDamageModule : ModifierModule
{
    
    public override void Modify(SpawnableContext context)
    {
        if (context.IsModifierTypeApplied<ExtraDamageModule>())
        {
            return;
        }
        
        var projectile = context.Spawnable.Node;
        TrailParticles.Create(projectile)
            .WithTexture(ParticleTextures.Triangle)
            .WithScale(0.4f, 0.1f)
            .Color(ColorScheme.Red)
            .Spawn();
    }
    
}