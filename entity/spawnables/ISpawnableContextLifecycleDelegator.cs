public interface ISpawnableContextLifecycleDelegator : IStatsAware
{

    public void DelegateLifecycle(SpawnableContext context, SpawnableContext targetContext);
    
}