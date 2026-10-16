using System.Collections.Generic;

public partial class RemoveTriggerComponent : Component
{
    public override ComponentType Type => ComponentTypeRegistry.RemoveTrigger;

    public readonly List<SpawnableContext> ContextsToSpawn = [];

    public override void Prepare(SpawnableContext context)
    {
        context.Spawnable.Removed += LaunchTriggers;
    }

    public void LaunchTriggers()
    {
        foreach (var contextsToSpawn in ContextsToSpawn)
        {
            contextsToSpawn.ForceSpawn();
        }
    }
    
}