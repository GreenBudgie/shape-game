using System.Collections.Generic;

public abstract partial class SpawnableModule : Module
{
    public override List<SpawnableStat> GetStats()
    {
        return CreateProducer().GetStats();
    }

    public abstract ISpawnableProducer CreateProducer();
    
}