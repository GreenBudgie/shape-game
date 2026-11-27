using System.Collections.Generic;

public partial class BoomerangModule : SpawnableModule
{

    public override ModuleType Type => ModuleTypeRegistry.Boomerang;

    public override ISpawnableProducer CreateProducer()
    {
        return BoomerangModuleProducer.Instance;
    }

    private class BoomerangModuleProducer : ISpawnableProducer
    {

        public static readonly BoomerangModuleProducer Instance = new();

        public List<SpawnableStat> GetStats() => [
            new DamageStat { Value = 6 },
            new SpeedStat { Value = 3000 },
            new ReloadStat { Value = 1f },
            new LifetimeStat { Value = 3 },
            new PiercingStat { Value = 4 },
        ];

        public ISpawnable CreateSpawnable()
        {
            return Boomerang.Create();
        }

    }

}
