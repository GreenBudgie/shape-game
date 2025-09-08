public interface IProjectile<out T> where T : Node2D
{

    public T Node { get; }

    /// <summary>
    /// Called at last, after all stats and modifiers were prepared and applied, but before it is entered the tree
    /// (before _Ready is called)
    /// </summary>
    public void Prepare(ShotContext context)
    {
    }

}