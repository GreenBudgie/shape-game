using System.Collections.Generic;

public partial class YinYangModule : SpawnableModule
{

    public override ModuleType Type => ModuleTypeRegistry.YinYang;

    public override ISpawnableProducer CreateProducer()
    {
        return YinYangModuleProducer.Instance;
    }

    private class YinYangModuleProducer : ISpawnableProducer
    {

        public static readonly YinYangModuleProducer Instance = new();

        public List<SpawnableStat> GetStats() => [
            new DamageStat { Value = 2 },
            new LifetimeStat { Value = 10 },
            new ReloadStat { Value = 0.5f },
            new SpeedStat { Value = 800 },
        ];

        public ISpawnable CreateSpawnable()
        {
            return YinYang.Create();
        }

    }

}
