using System;

public abstract partial class Enemy : Entity
{
    
    [Export] public Color Color { get; private set; }
    
    protected bool IsActive { get; private set; }

    /// <summary>
    /// Whether this enemy is environmental. Environmental enemies do not count as standard enemies.
    /// </summary>
    public virtual bool IsEnvironmental { get; } = false;
    
    private uint _initialCollisionLayer;
    private uint _initialCollisionMask;

    protected override void Setup()
    {
        AddToGroup(EnemyManager.AliveEnemiesGroup);
        
        Glow = GetNode<GlowWrapper>("Glow")
            .SetColor(Color)
            .SetStrength(0)
            .SetRadius(0)
            .EnablePulsing();
        Sprite = GetNode<Sprite2D>("Glow/Sprite");

        var pointer = new PointerArea(this)
        {
            PointerColor = Color,
            ShowOnlyWhenMovingIn = false,
        };
        AddChild(pointer);
        
        if (GetTimeToActivate() > 0)
        {
            Deactivate();
            GetTree().CreateTimer(GetTimeToActivate()).Timeout += Activate;
        }
        
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

    /// <summary>
    /// A callback when entity is activated. Not called if GetTimeToActivate is 0
    /// </summary>
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
    public virtual float GetTimeToActivate()
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
            FallingCrystal.Spawn(Area.GetRandomGlobalPoint());
        }
    }

    private void SpawnParticles()
    {
        BurstParticleEffect.Create(GlobalPosition)
            .AreaShape(Area)
            .WithTexture(ParticleTextures.Square)
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