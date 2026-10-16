using System.Collections.Generic;
using System.Linq;

public partial class MassiveShotModule : ModifierModule
{

    public override ModuleType Type => ModuleTypeRegistry.MassiveShot;

    public override ISpawnableModifier CreateModifier(SpawnableContext context)
    {
        return MassiveShotModifier.Instance;
    }

    private class MassiveShotModifier : ISpawnableModifier
    {
        
        private const float GravityScaleFactor = 1f;
        private const float MassFactor = 0.5f;
        
        public static readonly MassiveShotModifier Instance = new();

        public List<SpawnableStat> GetStats() => [
            new DamageStat { ValuePercent = 40f },
        ];

        public void Modify(SpawnableContext context)
        {
            var projectiles = context.GetContextChain()
                .Select(ctx => ctx.Spawnable.Node)
                .OfType<RigidBody2D>()
                .ToList();

            foreach (var projectile in projectiles)
            {
                projectile.GravityScale += GravityScaleFactor;
                projectile.Mass += MassFactor;
            }

            if (context.IsModifierTypeApplied<MassiveShotModifier>())
            {
                return;
            }

            foreach (var projectile in projectiles)
            {
                TrailParticles.Create(projectile)
                    .WithTexture(ParticleTextures.Triangle)
                    .WithScale(0.6f, 0.1f)
                    .Color(ColorScheme.Red)
                    .Spawn();
            }
        }

    }

}
