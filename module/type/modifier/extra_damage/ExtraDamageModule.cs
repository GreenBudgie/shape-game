using System.Collections.Generic;
using System.Linq;

public partial class ExtraDamageModule : ModifierModule
{

    public override ModuleType Type => ModuleTypeRegistry.ExtraDamage;

    public override ISpawnableModifier CreateModifier(SpawnableContext context)
    {
        return ExtraDamageModifier.Instance;
    }

    private class ExtraDamageModifier : ISpawnableModifier
    {

        public static readonly ExtraDamageModifier Instance = new();

        public List<SpawnableStat> GetStats() => [
            new DamageStat { Value = 1 },
            new ReloadStat { Value = 0.1f },
        ];

        public void Modify(SpawnableContext context)
        {
            if (context.IsModifierTypeApplied<ExtraDamageModifier>())
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
                    .Color(ColorScheme.Red)
                    .Spawn();
            }
        }

    }

}
