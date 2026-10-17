using System.Collections.Generic;

public class TriggerDelegator : ISpawnableContextLifecycleDelegator
{
    public List<SpawnableStat> GetStats() => [];

    public void DelegateLifecycle(SpawnableContext context, SpawnableContext targetContext)
    {
        var component = context.Spawnable.AddComponentIfPossible(ComponentTypeRegistry.RemoveTrigger);
        if (component is not RemoveTriggerComponent removeTriggerComponent)
        {
            return;
        }
        
        removeTriggerComponent.ContextToTrigger = targetContext;
    }
}