using System.Collections.Generic;
using System.Linq;

public class PiercingModule : ModifierModule
{

    public override Texture2D Texture => GD.Load<Texture2D>("uid://cctsssm2r3hvt");

    public override ModuleShape Shape => ModuleShapeRegistry.Double;

    public override string Name => "Piercing";

    public override string Description => "The projectile pierces +1 additional enemy";

    public override int Price => 15;

    public override List<SpawnableStat> Stats => [
        new PiercingStat { Value = 1 },
        new ReloadStat { Value = 0.2f },
    ];
    
    public override HashSet<HexCoordinates> OutgoingConnections => [HexCoordinates.Right * 2];

    public override void Modify(SpawnableContext context)
    {
        if (context.IsModifierTypeApplied<PiercingModule>())
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
                .Color(ColorScheme.LightBlue)
                .Spawn();
        }
    }

}
