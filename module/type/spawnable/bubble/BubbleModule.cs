using System.Collections.Generic;

public partial class BubbleModule : SpawnableModule
{

    public override ModuleType Type => ModuleTypeRegistry.Bubble;

    public override ISpawnableProducer CreateProducer()
    {
        return BubbleModuleProducer.Instance;
    }

    private class BubbleModuleProducer : ISpawnableProducer
    {

        public static readonly BubbleModuleProducer Instance = new();

        public List<SpawnableStat> GetStats() => [
            new DamageStat { Value = 0.5f },
            new SpeedStat { Value = 1000 },
            new ReloadStat { Value = 0.08f },
            new LifetimeStat { Value = 0.8f, ValueDelta = 0.15f },
        ];

        public ISpawnable CreateSpawnable()
        {
            return BubbleProjectile.Create();
        }

    }

}
