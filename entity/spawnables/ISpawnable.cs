public interface ISpawnable
{

    /// <summary>
    /// The spawnable itself. Every spawnable is expected to be a Node2D.
    /// </summary>
    public Node2D Node => (Node2D)this;

    public void Remove();

    /// <summary>
    /// Called right before a spawnable is prepared and added to the tree. No more context modifications will occur
    /// after this call, but modifying context is still allowed at this point.
    /// </summary>
    public void Prepare(SpawnableContext context)
    {
    }

}