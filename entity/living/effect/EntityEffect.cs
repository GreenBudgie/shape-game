public abstract partial class EntityEffect(Entity owner, float duration) : Node2D
{
    
    [Signal]
    public delegate void RemovedEventHandler();
    
    public abstract EntityEffectType Type { get; }

    public float Duration { get; set; } = duration;
    public Entity OwnerEntity { get; } = owner;

    public sealed override void _Ready()
    {
        OnAttach();
    }
    
    protected virtual void OnAttach()
    {
    }

    public sealed override void _Process(double delta)
    {
        Duration -= (float)delta;
        if (Duration <= 0)
        {
            Remove();
            return;
        }
        
        OnUpdate(delta);
    }

    protected virtual void OnUpdate(double delta)
    {
    }

    public void Remove()
    {
        EmitSignalRemoved();
        OnRemove();
    }

    /// <summary>
    /// Override to add custom removal logic. By default, this method just uses QueueFree, but sometimes you might
    /// need to prolong the tree removal for different reasons. "Removed" signal is still emitted immediately
    /// if overriden.
    /// </summary>
    protected virtual void OnRemove()
    {
        QueueFree();
    }
}