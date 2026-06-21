public abstract partial class SpawnableModule : Module
{

    /// <summary>
    /// Creates a spawnable, but does not add it to the tree
    /// </summary>
    public abstract ISpawnable<Node2D> CreateSpawnable();

}