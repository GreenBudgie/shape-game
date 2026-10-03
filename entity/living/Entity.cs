using System;

public abstract partial class Entity : RigidBody2D, IAreaAware
{

    public HealthController HealthController { get; }
    public EntityEffectController EffectController { get; }
    public CollisionAreaSampler Area { get; }

    public Sprite2D Sprite { get; protected set; } = null!;
    public GlowWrapper Glow { get; protected set; } = null!;

    public Entity()
    {
        HealthController = new HealthController(this);
        EffectController = new EntityEffectController(this);
        Area = new CollisionAreaSampler(this);
    }

    public sealed override void _Ready()
    {
        Setup();
        
        AddChild(HealthController);
        AddChild(EffectController);
        
        Validate();
    }

    protected virtual void Setup()
    {
    }

    private void Validate()
    {
        if (Sprite == null)
        {
            throw new Exception($"Sprite reference is not set for an entity {GetType().Name}");
        }
        if (Glow == null)
        {
            throw new Exception($"Glow reference is not set for an entity {GetType().Name}");
        }
    }
    
}