using System.Collections.Generic;

public abstract class ModuleType : IStatsAware
{

    public ModuleType()
    {
        ModuleTypeRegistry.Types.Add(this);
    }

    public abstract Texture2D Texture { get; }

    public abstract ModuleShape Shape { get; }
    
    public abstract string Name { get; }

    public abstract string Description { get; }
    
    public abstract int Price { get; }
    
    public abstract Color Color { get; }
    
    public virtual HashSet<HexCoordinates> OutgoingConnections { get; } = [];
    
    public virtual HashSet<HexCoordinates> IncomingConnections { get; } = [];

    /// <summary>
    /// Whether this module stops connections from propagating further. If a chain of connections
    /// meets this module, it will be treated as it has no further connections in that direction.
    ///
    /// Modules that interrupt connections may still form a cycle, which is prohibited.
    /// </summary>
    public virtual bool InterruptsConnections => false;

    public virtual List<SpawnableStat> Stats { get; } = [];
    
    IEnumerable<SpawnableStat> IStatsAware.Stats => Stats;

}