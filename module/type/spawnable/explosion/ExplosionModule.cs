using System.Collections.Generic;

public partial class ExplosionModule : SpawnableModule
{

    public override ModuleType Type => ModuleTypeRegistry.Explosion;

    public override ISpawnableProducer CreateProducer()
    {
        return ExplosionModuleProducer.Instance;
    }

    private class ExplosionModuleProducer : ISpawnableProducer
    {

        public static readonly ExplosionModuleProducer Instance = new();

        public List<SpawnableStat> GetStats() => [
            new ReloadStat { Value = 3 },
            new ExplosionDamageStat { Value = 10 },
            new ExplosionRadiusStat { Value = 400 },
        ];

        public ISpawnable CreateSpawnable()
        {
            return Explosion.Create();
        }

    }

}
