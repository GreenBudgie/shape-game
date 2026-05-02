public interface ISpawnableComponent
{

    /// <summary>
    /// Called BEFORE a spawnable is prepared and added to the tree. Modifying context is ALLOWED at this point.
    /// </summary>
    public void Prepare(SpawnableContext context)
    {
    }
    
    /// <summary>
    /// Called AFTER spawnable is prepared and added to the tree. Modifying context is NOT allowed at this point. 
    /// </summary>
    public void Apply(SpawnableContext context)
    {
    }

}