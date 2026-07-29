using System.Collections.Generic;

public interface IStatsAware
{
    
    public IEnumerable<SpawnableStat> Stats { get; }
    
}