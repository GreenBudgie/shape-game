using System.Collections.Generic;
using System.Linq;

public class ExtraDamageModuleType : ModifierModuleType
{

    public override Texture2D Texture => GD.Load<Texture2D>("uid://b8t5eahyf0qyr");

    public override ModuleShape Shape => ModuleShapeRegistry.Single;
    
    public override string Name => "Extra Damage";

    public override string Description => "Provides additional damage at the cost of reload time";

    public override int Price => 8;

    public override List<SpawnableStat> Stats => [
        new DamageStat { Value = 1 },
        new ReloadStat { Value = 0.1f },
    ];

    public override HashSet<HexCoordinates> OutgoingConnections => [HexCoordinates.Right];

    public override void Modify(SpawnableContext context)
    {
        if (context.IsModifierTypeApplied<ExtraDamageModuleType>())
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