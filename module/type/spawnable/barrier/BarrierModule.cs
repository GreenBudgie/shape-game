using System.Collections.Generic;

public partial class BarrierModule : SpawnableModule
{
    
    public override ModuleType Type => ModuleTypeRegistry.Barrier;

    public override List<SpawnableStat> GetStats() => [
        new LifetimeStat { Value = 10 },
        new ReloadStat { Value = 2 },
    ];

    public override ISpawnable CreateSpawnable()
    {
        return Barrier.Create();
    }
    
}