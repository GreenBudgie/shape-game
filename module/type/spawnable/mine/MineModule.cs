using System.Collections.Generic;

public partial class MineModule : SpawnableModule
{

    public override ModuleType Type => ModuleTypeRegistry.Mine;

    public override List<SpawnableStat> GetStats() => [
        new SpeedStat { Value = 5000 },
        new ReloadStat { Value = 3 },
        new ExplosionDamageStat { Value = 10 },
        new ExplosionRadiusStat { Value = 400 },
        new LifetimeStat { Value = 0.5f },
    ];

    public override ISpawnable<Node2D> CreateSpawnable()
    {
        return MineProjectile.Create();
    }

}
