using System.Collections.Generic;

public abstract class Level
{

    public Level()
    {
        LevelRegistry.Levels.Add(this);
    }
    
    public abstract int Number { get; }

    public abstract int DestroyRequirement { get; }
    
    public abstract List<LevelPhase> Phases { get; }

}