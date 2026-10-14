public partial class RemoveExplosionComponent : Component
{
    public override ComponentType Type => ComponentTypeRegistry.RemoveExplosion;
    
    public float FuseTime { get; set; }

    public override void Prepare(SpawnableContext context)
    {
        context.Spawnable.Removed += Explode;
    }

    public void Explode()
    {
        var explosion = Explosion.Create();
        var explosionContext = new SpawnableContext(explosion)
        {
            Position = GlobalPosition,
            Source = Spawnable.Node,
            OriginalSource = Spawnable.Context.OriginalSource,
        };

        explosionContext.AddStats(Spawnable.Context.GetStats<ExplosionDamageStat>());
        explosionContext.AddStats(Spawnable.Context.GetStats<ExplosionRadiusStat>());

        if (FuseTime > 0)
        {
            explosionContext.AddStat(new LifetimeStat { Value = FuseTime });
        }

        explosionContext.Spawn();
    }
    
}