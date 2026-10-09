public abstract partial class Component : Node2D
{

    /// <summary>
    /// Should be filled on creation in ComponentType, thus not available in Component's constructor
    /// </summary>
    public ISpawnable Spawnable { get; set; } = null!;
    
    public abstract ComponentType Type { get; }

    /// <summary>
    /// Called BEFORE a spawnable is prepared and added to the tree, but AFTER modifiers are applied.
    /// Modifying context is NOT allowed at this point.
    /// </summary>
    public virtual void Prepare(SpawnableContext context)
    {
    }
    
    /// <summary>
    /// Called AFTER spawnable is prepared and added to the tree  (after its _Ready() is called).
    /// Modifying context is NOT allowed at this point. 
    /// </summary>
    public virtual void Apply(SpawnableContext context)
    {
    }

}