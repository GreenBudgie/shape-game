using System.Collections.Generic;

public abstract class Level
{

    public Level()
    {
        LevelRegistry.Levels.Add(this);
    }
    
    public abstract int Number { get; }
    
    public virtual FloatRange PolysteroidTimeToSpawn => new(2f, 4f);
    
    public abstract List<LevelPhase> Phases { get; }

}