public abstract partial class SpawnableModuleType : ModuleType
{

    /// <summary>
    /// Creates a spawnable, but does not add it to the tree
    /// </summary>
    public abstract ISpawnable<Node2D> CreateSpawnable();

    public override Color Color => ColorScheme.Orange;
}