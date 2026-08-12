using System.Collections.Generic;

public abstract class Level
{

    public Level()
    {
        LevelRegistry.Levels.Add(this);
    }
    
    public abstract int Number { get; }

    public abstract int DestroyRequirement { get; }

    public virtual float PolysteroidMinTimeToSpawn => 2f;
    
    public virtual float PolysteroidMaxTimeToSpawn => 4f;

    public abstract float MaxEnemies { get; }
    
    public abstract List<LevelPhase> Phases { get; }

}