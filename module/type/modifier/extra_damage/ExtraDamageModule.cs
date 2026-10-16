using System.Collections.Generic;
using System.Linq;

public partial class ExtraDamageModule : ModifierModule
{

    public override ModuleType Type => ModuleTypeRegistry.ExtraDamage;

    public override ISpawnableModifier CreateModifier()
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
            
            var projectile = context.Spawnable.Node;
            if (projectile is not RigidBody2D rigidBody)
            {
                return;
            }

            TrailParticles.Create(rigidBody)
                .WithTexture(ParticleTextures.Triangle)
                .WithScale(0.4f, 0.1f)
                .Color(ColorScheme.Red)
                .Spawn();
        }

    }

}
