using System.Collections.Generic;

public partial class MissileModule : SpawnableModule
{
    public override ModuleType Type => ModuleTypeRegistry.Missile;

    public override List<SpawnableStat> GetStats() => [
        new DamageStat { Value = 3 },
        new ExplosionDamageStat { Value = 5 },
        new ExplosionRadiusStat { Value = 200 },
        new SpeedStat { Value = 2000 },
        new ReloadStat { Value = 1f },
        new LifetimeStat { Value = 5 },
    ];

    public override ISpawnable CreateSpawnable()
    {
        return MiniSphereProjectile.Create();
    }
}