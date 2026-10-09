public abstract class ComponentType
{
    
    public ComponentType()
    {
        ComponentTypeRegistry.Types.Add(this);
    }

    public abstract Component CreateComponent(ISpawnable spawnable);

    public bool IsApplicableTo(ISpawnable spawnable)
    {
        if (!Supports(spawnable) || !spawnable.SupportsComponentType(this))
        {
            return false;
        }

        return AllowMultipleCopies(spawnable) || !spawnable.HasComponentType(this);
    }

    /// <summary>
    /// Whether to automatically add this component to the provided spawnable. Repeating logic from Supports and
    /// AllowMultipleCopies here is not required, it is checked separately.
    /// </summary>
    public virtual bool ShouldAutoAdd(ISpawnable spawnable)
    {
        return false;
    }
    
    /// <summary>
    /// Whether this component can work on a provided spawnable. Component should NEVER be added to a spawnable
    /// that it doesn't support. True by default.
    /// </summary>
    public virtual bool Supports(ISpawnable spawnable)
    {
        return true;
    }
    
    /// <summary>
    /// Whether it's possible to have multiple copies of the same component type on a single spawnable.
    /// Second component should NEVER be added to a spawnable that doesn't support multiple copies.
    /// False by default.
    /// </summary>
    public virtual bool AllowMultipleCopies(ISpawnable spawnable)
    {
        return false;
    }

}