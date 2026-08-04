using System.Collections.Generic;
using System.Linq;

public class MassiveShotModule : ModifierModule
{
    
    private const float GravityScaleFactor = 1f;
    private const float MassFactor = 0.5f;

    public override Texture2D Texture => GD.Load<Texture2D>("uid://b18uxs8lf5pwj");

    public override ModuleShape Shape => ModuleShapeRegistry.Single;

    public override string Name => "Massive Shot";

    public override string Description => "Makes projectile heavier while increasing its damage";

    public override int Price => 10;

    public override List<SpawnableStat> Stats => [
        new DamageStat { ValuePercent = 40f },
    ];
    
    public override HashSet<HexCoordinates> OutgoingConnections => [HexCoordinates.Right];

    public override void Modify(SpawnableContext context)
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

        if (context.IsModifierTypeApplied<MassiveShotModule>())
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
