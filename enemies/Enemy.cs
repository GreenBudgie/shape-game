using System;

public abstract partial class Enemy : RigidBody2D
{

    [Signal]
    public delegate void DestroyedEventHandler();
    
    [Export] public Color Color { get; private set; }

    [Export] protected CollisionShape2D Area = null!;
    [Export] protected AudioStream DamageSound = null!;
    [Export] protected AudioStream DestroySound = null!;

    private float _health;

    private Glow _glow = null!;
    private AnimationPlayer _enemyAnimations = null!;

    public bool IsDestroyed { get; private set; }
    /// <summary>
    /// A rectangle (in local coordinates) to use for spawning effects inside the enemy
    /// </summary>
    public Rect2 AreaRect { get; private set; }

    public override void _Ready()
    {
        if (Area.Shape is not RectangleShape2D areaShape)
        {
            throw new ArgumentException(
                $"Crystal spawn area should be a RectangleShape2D, but {Name} uses different shape"
            );
        }
        if (!Area.Disabled)
        {
            Area.Disabled = true;
        }
        AreaRect = areaShape.GetRect();

        _enemyAnimations = GetNode<AnimationPlayer>("EnemyAnimations");
        _health = GetMaxHealth();

        var sprite = GetNode<Sprite2D>("Sprite");
        _glow = Glow.AddGlow(sprite)
            .SetColor(Color)
            .SetStrength(0)
            .SetRadius(0)
            .EnablePulsing();
    }

    public abstract float GetMaxHealth();
    
    public abstract float GetCrystalsToDrop();

    public void Damage(float damage)
    {
        if (IsDestroyed)
        {
            return;
        }
        
        DamageEffect.Create(damage, this);
        _health -= damage;
        if (_health <= 0)
        {
            Destroy();
            return;
        }
        
        var hpRatio = Clamp(_health / GetMaxHealth(), 0f, 1f);
        var dangerLevel = 1f - hpRatio;

        var sound = SoundManager.Instance.PlayPositionalSound(this, DamageSound);
        sound.PitchScale = Lerp(0.75f, 1.25f, dangerLevel);
        
        _glow
            .SetRadius(40f * dangerLevel)
            .SetStrength(2f * dangerLevel)
            .SetPulseRadiusDelta(20f * dangerLevel)
            .SetPulseStrengthDelta(dangerLevel)
            .SetPulsesPerSecond(1f + dangerLevel * (3f - 1f));

        _enemyAnimations.Play("damage");
    }

    private void Destroy()
    {
        IsDestroyed = true;
        CollisionLayer = 0;
        CollisionMask = 0;

        EnemyDestroyParticles.Create(this);
        SoundManager.Instance.PlayPositionalSound(this, DestroySound);

        for (var i = 0; i < GetCrystalsToDrop(); i++)
        {
            var crystal = FallingCrystal.Create();
            Callable.From(() => ShapeGame.Instance.AddChild(crystal)).CallDeferred();

            crystal.GlobalPosition = GlobalPosition + AreaRect.RandomPoint();
            var randomStrength = (float)GD.RandRange(750f, 1500f);
            var randomAngle = GD.RandRange(5 * Pi / 4, 7 * Pi / 4);
            var randomDirection = Vector2.FromAngle((float)randomAngle);
            var impulse = randomDirection * randomStrength;
            crystal.ApplyCentralImpulse(impulse);
        }

        _glow.DisablePulsing();
        _glow.SetCullOccluded(false);
        var fadeOutTween = _glow.CreateTween();
        var setColorAction = _glow.SetColor;
        var finalGlowColor = _glow.GetColor();
        finalGlowColor.A = 0;
        fadeOutTween.TweenMethod(Callable.From(setColorAction), _glow.GetColor(), finalGlowColor, 0.25);

        EmitSignalDestroyed();
        EnemyManager.Instance.EmitSignal(EnemyManager.SignalName.EnemyDestroyed, this);
        
        _enemyAnimations.Play("destroy");
        _enemyAnimations.AnimationFinished += _ => QueueFree();
    }

}