using System.Collections.Generic;
using System.Linq;

public partial class ExtraDamageModule : ModifierModule
{

    public override ModuleType Type => ModuleTypeRegistry.ExtraDamage;

    public override List<SpawnableStat> GetStats() => [
        new DamageStat { Value = 1 },
        new ReloadStat { Value = 0.1f },
    ];

    public override void Modify(SpawnableContext context)
    {
        if (context.IsModifierTypeApplied<ExtraDamageModule>())
        {
            return;
        }

        var projectiles = context.GetContextChain()
            .Select(ctx => ctx.Spawnable.Node)
            .OfType<BasicRigidBodyProjectile<Node2D>>();
        foreach (var projectile in projectiles)
        {
            TrailParticles.Create(projectile)
                .WithTexture(ParticleTextures.Triangle)
                .WithScale(0.4f, 0.1f)
                .Color(ColorScheme.Red)
                .Spawn();
        }
    }

}
