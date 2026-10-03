using System.Collections.Generic;

public partial class BoltModule : SpawnableModule
{

    public override ModuleType Type => ModuleTypeRegistry.Bolt;

    public override List<SpawnableStat> GetStats() => [
        new DamageStat { Value = 2 },
        new SpeedStat { Value = 3000 },
        new ReloadStat { Value = 0.8f },
        new LifetimeStat { Value = 4 },
    ];

    public override ISpawnable CreateSpawnable()
    {
        return BoltProjectile.Create();
    }

}
