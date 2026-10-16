using System.Collections.Generic;

public partial class BoltModule : SpawnableModule
{

    public override ModuleType Type => ModuleTypeRegistry.Bolt;

    public override ISpawnableProducer CreateProducer()
    {
        return BoltModuleProducer.Instance;
    }

    private class BoltModuleProducer : ISpawnableProducer
    {

        public static readonly BoltModuleProducer Instance = new();

        public List<SpawnableStat> GetStats() => [
            new DamageStat { Value = 5 },
            new SpeedStat { Value = 3000 },
            new ReloadStat { Value = 0.6f },
            new LifetimeStat { Value = 4 },
        ];

        public ISpawnable CreateSpawnable()
        {
            return BoltProjectile.Create();
        }

    }

}
