public interface ISpawnableComponent
{

    /// <summary>
    /// Called before a spawnable is added to the tree
    /// </summary>
    public void Prepare(SpawnableContext context)
    {
    }

}