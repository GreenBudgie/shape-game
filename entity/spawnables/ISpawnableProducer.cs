public interface ISpawnableProducer : IStatsAware
{

    /// <summary>
    /// Creates a raw spawnable, but does not add it to the tree
    /// </summary>
    public ISpawnable CreateSpawnable();

}