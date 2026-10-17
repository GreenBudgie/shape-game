using System.Collections.Generic;

public class TetherDelegator : ISpawnableContextLifecycleDelegator
{
    public List<SpawnableStat> GetStats() => [];

    public void DelegateLifecycle(SpawnableContext context, SpawnableContext targetContext)
    {
        var component = context.Spawnable.AddComponentIfPossible(ComponentTypeRegistry.Tether);
        if (component is not TetherComponent tetherComponent)
        {
            return;
        }
        
        tetherComponent.ContextToConnect = targetContext;
    }
}