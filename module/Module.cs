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
    
    public virtual HashSet<HexCoordinates> Connections { get; } = [];

    public virtual List<SpawnableStat> Stats { get; } = [];
    
    IEnumerable<SpawnableStat> IStatsAware.Stats => Stats;

}