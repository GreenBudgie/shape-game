using System.Collections.Generic;

public abstract class Module : IStatsAware
{

    public Module()
    {
        ModuleRegistry.Modules.Add(this);
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
    /// Whether this module as an intermediate connection and splits the chain of connections in two.
    /// Usually it's only possible when module has both incoming and outgoing connections
    /// </summary>
    public virtual bool BreaksConnectionCycle { get; } = false;

    public virtual List<SpawnableStat> Stats { get; } = [];
    
    IEnumerable<SpawnableStat> IStatsAware.Stats => Stats;

}