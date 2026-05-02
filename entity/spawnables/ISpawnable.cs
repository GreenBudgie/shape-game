public interface ISpawnable<out T> where T : Node2D
{

    public T Node { get; }

    public void Remove();

    /// <summary>
    /// Called right before a spawnable is prepared and added to the tree. No more context modifications will occur
    /// after this call, but modifying context is still allowed at this point.
    /// </summary>
    public void Prepare(SpawnableContext context)
    {
    }

}