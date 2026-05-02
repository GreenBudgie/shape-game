public interface ISpawnable<out T> where T : Node2D
{

    public T Node { get; }

    public void Remove();

    /// <summary>
    /// Called after all stats were prepared and applied, but before it is entered the tree (before _Ready is called)
    /// </summary>
    public void Prepare(SpawnableContext context)
    {
    }

}