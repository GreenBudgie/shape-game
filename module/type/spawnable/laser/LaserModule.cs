using System.Collections.Generic;

public partial class LaserModule : SpawnableModule
{

    public override ModuleType Type => ModuleTypeRegistry.Laser;

    public override ISpawnableProducer CreateProducer()
    {
        return LaserModuleProducer.Instance;
    }

    private class LaserModuleProducer : ISpawnableProducer
    {

        public static readonly LaserModuleProducer Instance = new();

        public List<SpawnableStat> GetStats() => [
            new ReloadStat { Value = 1f },
            new LifetimeStat { Value = 1.4f },
        ];

        public ISpawnable CreateSpawnable()
        {
            return new Laser();
        }

    }

}
