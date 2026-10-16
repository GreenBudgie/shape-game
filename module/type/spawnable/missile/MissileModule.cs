using System.Collections.Generic;

public partial class MissileModule : SpawnableModule
{

    public override ModuleType Type => ModuleTypeRegistry.Missile;

    public override ISpawnableProducer CreateProducer()
    {
        return MissileModuleProducer.Instance;
    }

    private class MissileModuleProducer : ISpawnableProducer
    {

        public static readonly MissileModuleProducer Instance = new();

        public List<SpawnableStat> GetStats() => [
            new DamageStat { Value = 3 },
            new ExplosionDamageStat { Value = 5 },
            new ExplosionRadiusStat { Value = 200 },
            new SpeedStat { Value = 500 },
            new ReloadStat { Value = 1f },
            new LifetimeStat { Value = 4 },
        ];

        public ISpawnable CreateSpawnable()
        {
            return MissileProjectile.Create();
        }

    }

}
