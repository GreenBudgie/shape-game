using System.Collections.Generic;

public abstract partial class ModifierModule : Module
{

    public override List<SpawnableStat> GetStats()
    {
        return CreateModifier().GetStats();
    }
    
    public abstract ISpawnableModifier CreateModifier();

}
