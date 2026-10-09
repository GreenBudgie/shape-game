using System;

public interface ISpawnable
{

    /// <summary>
    /// Called after entity was damaged by this spawnable
    /// </summary>
    public event Action<EntityDamagedEvent> EntityDamaged;

    /// <summary>
    /// Filled automatically on context creation. Do not assign a new value to it!
    ///
    /// Will be null in constructor, but always not-null on preparation. Not marked as nullable for convenience.
    /// </summary>
    public SpawnableContext Context { get; set; }

    /// <summary>
    /// The spawnable itself, cast to Node2D. Every spawnable is expected to be a Node2D.
    /// </summary>
    public Node2D Node => (Node2D)this;

    public void Remove();
    
    /// <summary>
    /// Called right after an instance is created, before modifiers and components are added to the context.
    /// Context can still be modified after this point.
    /// </summary>
    public void Setup(SpawnableContext context)
    {
    }

    /// <summary>
    /// Called right before a spawnable is prepared and added to the tree. Context modification is NOT allowed at
    /// this point.
    /// </summary>
    public void Prepare(SpawnableContext context)
    {
    }

    /// <summary>
    /// Whether this spawnable supports provided component type. If false, this component can never be added to
    /// this spawnable, even through auto-add. True by default.
    /// </summary>
    public bool SupportsComponentType(ComponentType type)
    {
        return true;
    }

}