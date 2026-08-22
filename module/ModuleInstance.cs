using System.Collections.Generic;
using System.Linq;

public class ModuleInstance(ModuleType type) : IStatsAware
{

    public ModuleType Type { get; } = type;
    public ActiveModule? ActiveModule { get; } = type.CreateActiveModule(null);

    public IEnumerable<SpawnableStat> Stats => Type.Stats.Concat(ActiveModule?.Stats ?? []);
}