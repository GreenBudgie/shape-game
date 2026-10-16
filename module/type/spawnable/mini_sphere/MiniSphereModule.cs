using System.Collections.Generic;

public partial class MiniSphereModule : SpawnableModule
{

    public override ModuleType Type => ModuleTypeRegistry.MiniSphere;

    public override ISpawnableProducer CreateProducer()
    {
        return MiniSphereModuleProducer.Instance;
    }

    private class MiniSphereModuleProducer : ISpawnableProducer
    {

        public static readonly MiniSphereModuleProducer Instance = new();

        public List<SpawnableStat> GetStats() => [
            new DamageStat { Value = 2 },
            new SpeedStat { Value = 2000 },
            new ReloadStat { Value = 0.2f },
            new LifetimeStat { Value = 1, ValueDelta = 0.1f },
        ];

        public ISpawnable CreateSpawnable()
        {
            return MiniSphereProjectile.Create();
        }

    }

}
