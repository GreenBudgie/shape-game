using System.Collections.Generic;
using System.Linq;

public partial class MassiveShotModule : ModifierModule
{

    public override ModuleType Type => ModuleTypeRegistry.MassiveShot;

    public override ISpawnableModifier CreateModifier()
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
            var projectile = context.Spawnable.Node;
            if (projectile is not RigidBody2D rigidBody)
            {
                return;
            }

            rigidBody.GravityScale += GravityScaleFactor;
            rigidBody.Mass += MassFactor;

            if (context.IsModifierTypeApplied<MassiveShotModifier>())
            {
                return;
            }

            TrailParticles.Create(rigidBody)
                .WithTexture(ParticleTextures.Triangle)
                .WithScale(0.6f, 0.1f)
                .Color(ColorScheme.Red)
                .Spawn();
        }

    }

}
