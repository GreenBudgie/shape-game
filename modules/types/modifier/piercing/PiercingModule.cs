[GlobalClass]
public partial class PiercingModule : ModifierModule
{
    
    public override void Modify(SpawnableContext context)
    {
        if (context.IsModifierTypeApplied<PiercingModule>())
        {
            return;
        }
        
        var projectile = context.Spawnable.Node;
        TrailParticles.Create(projectile)
            .WithTexture(ParticleTextures.Triangle)
            .WithScale(0.4f, 0.1f)
            .Color(ColorScheme.LightBlue)
            .Spawn();
    }
    
}
