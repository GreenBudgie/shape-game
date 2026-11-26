public abstract partial class Entity : RigidBody2D, IAreaAware
{
    private static readonly Texture2D DissolveNoise = GD.Load<Texture2D>("uid://dv7focnbevck5");

    public HealthController HealthController { get; }
    public EntityEffectController EffectController { get; }
    public CollisionAreaSampler Area { get; }
    public CanvasGroup VisualEffectsGroup { get; private set; } = null!;
    public VisualEffects Effects { get; private set; } = null!;
    public Sprite2D Sprite { get; private set; } = null!;

    public Entity()
    {
        HealthController = new HealthController(this);
        EffectController = new EntityEffectController(this);
        Area = new CollisionAreaSampler(this);
    }

    public sealed override void _Ready()
    {
        VisualEffectsGroup = GetNode<CanvasGroup>("VisualEffectsGroup");
        Sprite = VisualEffectsGroup.GetNode<Sprite2D>("Sprite");
        Effects = VisualEffects.Attach(VisualEffectsGroup);
        Effects.DissolveTexture = DissolveNoise;
        Effects.PulseEnabled = true;
        Setup();

        HealthController.DestroyAnimationFinished += QueueFree;
        AddChild(HealthController);
        AddChild(EffectController);
    }

    protected abstract void Setup();
}