using System.Collections.Generic;

public partial class BarrierModule : SpawnableModule
{

    public override ModuleType Type => ModuleTypeRegistry.Barrier;

    public override ISpawnableProducer CreateProducer()
    {
        return BarrierModuleProducer.Instance;
    }

    private class BarrierModuleProducer : ISpawnableProducer
    {

        public static readonly BarrierModuleProducer Instance = new();

        public List<SpawnableStat> GetStats() => [
            new LifetimeStat { Value = 10 },
            new ReloadStat { Value = 2 },
        ];

        public ISpawnable CreateSpawnable()
        {
            return Barrier.Create();
        }

    }

}
