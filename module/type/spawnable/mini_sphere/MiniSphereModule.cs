using System.Collections.Generic;

public partial class MiniSphereModule : SpawnableModule
{

    public override ModuleType Type => ModuleTypeRegistry.MiniSphere;

    public override List<SpawnableStat> GetStats() => [
        new DamageStat { Value = 3 },
        new SpeedStat { Value = 2000 },
        new ReloadStat { Value = 0.2f },
        new LifetimeStat { Value = 1, ValueDelta = 0.1f },
    ];

    public override ISpawnable CreateSpawnable()
    {
        return MiniSphereProjectile.Create();
    }

}
