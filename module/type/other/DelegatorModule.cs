using System.Collections.Generic;

public abstract partial class DelegatorModule : Module
{
    public override List<SpawnableStat> GetStats() => [];

    public abstract ISpawnableContextLifecycleDelegator CreateDelegator();

}