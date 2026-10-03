using System.Collections.Generic;
using System.Linq;

public partial class PiercingModule : ModifierModule
{

    public override ModuleType Type => ModuleTypeRegistry.Piercing;

    public override List<SpawnableStat> GetStats() => [
        new PiercingStat { Value = 1 },
        new ReloadStat { Value = 0.2f },
    ];

    public override void Modify(SpawnableContext context)
    {
        if (context.IsModifierTypeApplied<PiercingModule>())
        {
            return;
        }

        var projectiles = context.GetContextChain()
            .Select(ctx => ctx.Spawnable.Node)
            .OfType<BasicRigidBodyProjectile>();

        foreach (var projectile in projectiles)
        {
            TrailParticles.Create(projectile)
                .WithTexture(ParticleTextures.Triangle)
                .WithScale(0.4f, 0.1f)
                .Color(ColorScheme.LightBlue)
                .Spawn();
        }
    }

}
