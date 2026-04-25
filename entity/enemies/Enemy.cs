using System;

public abstract partial class Enemy : RigidBody2D
{
    
    [Export] public Color Color { get; private set; }

    [Export] protected CollisionShape2D Area = null!;

    public HealthController HealthController { get; private set; } = null!;
    
    private GlowWrapper _glowWrapper = null!;

    /// <summary>
    /// A rectangle (in local coordinates) to use for spawning effects inside the enemy
    /// </summary>
    public Rect2 AreaRect { get; private set; }
    
    protected bool IsActive { get; private set; }
    
    private uint _initialCollisionLayer;
    private uint _initialCollisionMask;

    public override void _Ready()
    {
        if (!Area.Disabled)
        {
            Area.Disabled = true;
        }
        AreaRect = Area.Shape.GetRect();
        
        AddToGroup(EnemyManager.AliveEnemiesGroup);

        HealthController = HealthController.GetHealthController(this);

        _glowWrapper = GetNode<GlowWrapper>("Glow")
            .SetColor(Color)
            .SetStrength(0)
            .SetRadius(0)
            .EnablePulsing();

        Deactivate();
        GetTree().CreateTimer(GetTimeToActivate()).Timeout += Activate;
        HealthController.Destroyed += OnDestroy;
        HealthController.DestroyAnimationFinished += QueueFree;
    }
    
    private void Deactivate()
    {
        _initialCollisionLayer = CollisionLayer;
        _initialCollisionMask = CollisionMask;
        CollisionLayer = 0;
        CollisionMask = 0;
        
        SetPhysicsProcess(false);
        SetProcess(false);
    }

    private void Activate()
    {
        IsActive = true;
        CollisionLayer = _initialCollisionLayer;
        CollisionMask = _initialCollisionMask;
        SetPhysicsProcess(true);
        SetProcess(true);
        OnActivate();
    }

    protected virtual void OnActivate()
    {
    }
    
    public abstract float GetCrystalsToDrop();

    /// <summary>
    /// The time it takes for entity to start any actions (firing, moving to path e.t.c.), in seconds.
    ///
    /// Before this time, entity is also invulnerable (its collision layer/mask are disabled).
    ///
    /// 1 second by default.
    /// </summary>
    public float GetTimeToActivate()
    {
        return 1;
    }

    private void OnDestroy()
    {
        RemoveFromGroup(EnemyManager.AliveEnemiesGroup);
        CollisionLayer = 0;
        CollisionMask = 0;

        Callable.From(SpawnParticles).CallNextPhysicsFrame(GetTree());

        if (EnemyManager.Instance.EnemiesDropCrystals)
        {
            DropCrystals();
        }

        EnemyManager.Instance.EmitSignal(EnemyManager.SignalName.EnemyDestroyed, this);
    }

    private void DropCrystals()
    {
        for (var i = 0; i < GetCrystalsToDrop(); i++)
        {
            var position = GlobalPosition + AreaRect.RandomPoint();
            FallingCrystal.Spawn(position);
        }
    }


    private void SpawnParticles()
    {
        BurstParticleEffect.Create(GlobalPosition)
            .WithTexture(ParticleTextures.Square)
            .RectangleShape(AreaRect)
            .WithAmountPerPixel(0.15f)
            .Color(Color)
            .InheritVelocity(this)
            .VelocitySpreadFactor(0.08f)
            .MinVelocity(300f)
            .VelocityDelta(150f)
            .MaxVelocity(2000f)
            .Configure()
            .Spawn();
    }

}