using System.Collections.Generic;

public partial class ExplosionModule : SpawnableModule
{

    public override ModuleType Type => ModuleTypeRegistry.Explosion;

    public override List<SpawnableStat> GetStats() => [
        new ReloadStat { Value = 3 },
        new ExplosionDamageStat { Value = 10 },
        new ExplosionRadiusStat { Value = 400 },
    ];

    public override ISpawnable CreateSpawnable()
    {
        return Explosion.Create();
    }

}
