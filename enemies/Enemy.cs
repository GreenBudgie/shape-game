using System;

public abstract partial class Enemy : RigidBody2D
{

    [Export] protected Color EnemyColor { get; private set; }

    [Export] protected CollisionShape2D CrystalSpawnArea = null!;

    [Export] protected AudioStream DamageSound = null!;
    [Export] protected AudioStream DestroySound = null!;

    private float _health;

    private Glow _glow = null!;
    private AnimationPlayer _enemyAnimations = null!;

    public bool IsDestroyed { get; private set; }

    public override void _Ready()
    {
        if (CrystalSpawnArea.Shape is not RectangleShape2D)
        {
            throw new ArgumentException(
                $"Crystal spawn area should be a RectangleShape2D, but {Name} uses different shape"
            );
        }
        if (!CrystalSpawnArea.Disabled)
        {
            CrystalSpawnArea.Disabled = true;
        }

        _enemyAnimations = GetNode<AnimationPlayer>("EnemyAnimations");
        _health = GetMaxHealth();

        var sprite = GetNode<Sprite2D>("Sprite");
        _glow = Glow.AddGlow(sprite)
            .SetColor(EnemyColor)
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

        SoundManager.Instance.PlayPositionalSound(this, DestroySound);

        var crystalSpawnAreaShape = (RectangleShape2D)CrystalSpawnArea.Shape;
        var halfShapeSize = crystalSpawnAreaShape.Size * 0.5f;
        var areaX = CrystalSpawnArea.Position.X;
        var areaY = CrystalSpawnArea.Position.Y;
        var minCrystalSpawnX = areaX - halfShapeSize.X;
        var maxCrystalSpawnX = areaX + halfShapeSize.X;
        var minCrystalSpawnY = areaY + halfShapeSize.Y;
        var maxCrystalSpawnY = areaY + halfShapeSize.Y;
        for (var i = 0; i < GetCrystalsToDrop(); i++)
        {
            var crystal = FallingCrystal.Create();
            ShapeGame.Instance.CallDeferred(Node.MethodName.AddChild, crystal);
            
            var randomXOffset = (float)GD.RandRange(minCrystalSpawnX, maxCrystalSpawnX);
            var randomYOffset = (float)GD.RandRange(minCrystalSpawnY, maxCrystalSpawnY);
            var randomOffset = new Vector2(randomXOffset, randomYOffset);
            crystal.GlobalPosition = GlobalPosition + randomOffset;
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

        EnemyManager.Instance.EmitSignal(EnemyManager.SignalName.EnemyDestroyed, this);
        
        _enemyAnimations.Play("destroy");
        _enemyAnimations.AnimationFinished += _ => QueueFree();
    }

}