using System.Collections.Generic;

public class TriggerDelegator : ISpawnableContextLifecycleDelegator
{
    public List<SpawnableStat> GetStats() => [];

    public void DelegateLifecycle(SpawnableContext context, SpawnableContext targetContext)
    {
        if (context.Spawnable.IsProxy)
        {
            return;
        }
        
        var component = context.Spawnable.GetOrAddSingleComponentIfPossible(ComponentTypeRegistry.RemoveTrigger);
        if (component is not RemoveTriggerComponent removeTriggerComponent)
        {
            return;
        }
        
        removeTriggerComponent.ContextsToSpawn.Add(targetContext);
    }
}