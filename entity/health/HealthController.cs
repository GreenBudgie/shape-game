public partial class HealthController : Node2D
{
    [Signal]
    public delegate void DamagedEventHandler(float damage);

    [Signal]
    public delegate void DestroyedEventHandler();

    [Signal]
    public delegate void DestroyAnimationFinishedEventHandler();

    [ExportGroup("Health")] 
    [Export] public float MaxHealth { get; set; }

    [ExportGroup("Relations")] 
    [Export] private Sprite2D? _sprite;
    [Export] private GlowWrapper? _glowWrapper;
    [Export] private CollisionShape2D _damageLabelSpawnArea = null!;

    [ExportGroup("Sounds")] 
    [Export] protected AudioStream DamageSound = null!;
    [Export] protected AudioStream DestroySound = null!;

    public float Health { get; private set; }

    private Tween? _damageTween;

    public static HealthController? GetHealthControllerIfExists(Node2D owner)
    {
        return owner.GetNodeOrNull<HealthController>("HealthController");
    }

    public static HealthController GetHealthController(Node2D owner)
    {
        return owner.GetNode<HealthController>("HealthController");
    }

    public override void _Ready()
    {
        Health = MaxHealth;
    }

    public bool IsDestroyed()
    {
        return Health <= 0f;
    }

    public float GetHealthRatio()
    {
        return Clamp(Health / MaxHealth, 0f, 1f);
    }

    public void Damage(float damage)
    {
        if (IsDestroyed())
        {
            return;
        }

        DamageLabel.Create(damage, ToGlobal(_damageLabelSpawnArea.Shape.GetRect().RandomPoint()));

        Health = Max(Health - damage, 0);
        EmitSignalDamaged(damage);

        if (IsDestroyed())
        {
            ForceDestroy();
            return;
        }

        var dangerLevel = 1f - GetHealthRatio();

        var sound = SoundManager.Instance.PlayPositionalSound(this, DamageSound);
        sound.PitchScale = Lerp(0.75f, 1.25f, dangerLevel);

        _glowWrapper?
            .SetRadius(40f * dangerLevel)
            .SetStrength(2f * dangerLevel)
            .SetPulseRadiusDelta(20f * dangerLevel)
            .SetPulseStrengthDelta(dangerLevel)
            .SetPulsesPerSecond(1f + dangerLevel * (3f - 1f));

        if (_sprite == null)
        {
            return;
        }

        const float duration = 0.25f;
        const float inDuration = duration / 4f;

        _damageTween?.Kill();
        _damageTween = _sprite.CreateTween().SetTrans(Tween.TransitionType.Cubic);

        _damageTween.TweenProperty(_sprite, ScaleProperty, new Vector2(1.2f, 1.2f), inDuration)
            .SetEase(Tween.EaseType.Out);
        _damageTween.Parallel()
            .TweenProperty(_sprite, ModulateProperty, new Color(5f, 5f, 5f), inDuration)
            .SetEase(Tween.EaseType.Out);
        
        _damageTween.TweenProperty(_sprite, ScaleProperty, Vector2.One, duration - inDuration)
            .SetEase(Tween.EaseType.In);
        _damageTween.Parallel()
            .TweenProperty(_sprite, ModulateProperty, Colors.White, duration - inDuration)
            .SetEase(Tween.EaseType.In);
    }

    public void Destroy()
    {
        if (!IsDestroyed())
        {
            ForceDestroy();
        }
    }

    private void ForceDestroy()
    {
        Health = 0;
        EmitSignalDestroyed();

        SoundManager.Instance.PlayPositionalSound(this, DestroySound);
        const float duration = 0.4f;

        if (_glowWrapper != null)
        {
            _glowWrapper.DisablePulsing();
            var fadeOutTween = _glowWrapper.CreateTween();
            fadeOutTween.TweenProperty(_glowWrapper, "Color:a", 0, duration);
        }

        if (_sprite == null)
        {
            EmitSignalDestroyAnimationFinished();
            return;
        }

        var destroyTween = _sprite.CreateTween()
            .SetTrans(Tween.TransitionType.Cubic)
            .SetEase(Tween.EaseType.Out);
        destroyTween.TweenProperty(_sprite, ModulateProperty, new Color(5f, 5f, 5f, 0f), duration);
        destroyTween.Parallel()
            .TweenProperty(_sprite, ScaleProperty, new Vector2(1.75f, 1.75f), duration);
        DissolveEffect.DissolveSprite(_sprite, duration);

        destroyTween.Finished += EmitSignalDestroyAnimationFinished;
    }
}