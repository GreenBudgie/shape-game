public partial class HealthController : Node2D
{
    [Signal]
    public delegate void HealthChangedEventHandler(float delta);

    [Signal]
    public delegate void DestroyedEventHandler();

    [Signal]
    public delegate void DestroyAnimationFinishedEventHandler();

    [ExportGroup("Health")] [Export] public float MaxHealth { get; set; }

    [ExportGroup("Relations")] [Export] private Sprite2D? _sprite;
    [Export] private GlowWrapper? _glowWrapper;
    [Export] private CollisionShape2D _damageLabelSpawnArea = null!;

    [ExportGroup("Sounds")] [Export] protected AudioStream DamageSound = null!;
    [Export] protected AudioStream DestroySound = null!;

    /// <summary>
    /// Health, always between 0 and MaxHealth
    /// </summary>
    public float Health { get; private set; }

    /// <summary>
    /// When true, damage will never be received. However, healing will still work.
    /// Any damage will be treated as 0.
    /// </summary>
    public bool IsInvulnerable { get; set; }

    private Tween? _healthTween;

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

    public void FullHeal()
    {
        ChangeHealth(MaxHealth - Health);
    }

    public void ChangeHealth(float delta)
    {
        if (IsDestroyed())
        {
            return;
        }

        HealthLabel.Create(delta, ToGlobal(_damageLabelSpawnArea.Shape.GetRect().RandomPoint()));

        if (!IsInvulnerable || delta >= 0)
        {
            Health = Clamp(Health + delta, 0, MaxHealth);
        }

        EmitSignalHealthChanged(delta);

        if (IsDestroyed())
        {
            ForceDestroy();
            return;
        }

        var dangerLevel = 1f - GetHealthRatio();

        _glowWrapper?
            .SetRadius(40f * dangerLevel)
            .SetStrength(2f * dangerLevel)
            .SetPulseRadiusDelta(20f * dangerLevel)
            .SetPulseStrengthDelta(dangerLevel)
            .SetPulsesPerSecond(1f + dangerLevel * (3f - 1f));

        if (delta <= 0)
        {
            PlayDamageEffect(dangerLevel);
        }
        else
        {
            PlayHealEffect();
        }
    }

    private void PlayHealEffect()
    {
        // TODO heal sound
        // var sound = SoundManager.Instance.PlayPositionalSound(this, DamageSound);
        // sound.PitchScale = Lerp(0.75f, 1.25f, dangerLevel);

        if (_sprite == null)
        {
            return;
        }

        const float duration = 0.25f;
        const float inDuration = duration / 4f;

        _healthTween?.Kill();
        _healthTween = _sprite.CreateTween().SetTrans(Tween.TransitionType.Cubic);

        _healthTween.TweenScale(_sprite, 1.2f, inDuration)
            .SetEase(Tween.EaseType.Out);
        _healthTween.Parallel()
            .TweenRotationDegrees(_sprite, 30f, inDuration)
            .SetEase(Tween.EaseType.Out);
        _healthTween.Parallel()
            .TweenModulate(_sprite, Colors.LightGreen * 5f, inDuration)
            .SetEase(Tween.EaseType.Out);

        _healthTween.TweenScale(_sprite, 1f, duration - inDuration)
            .SetEase(Tween.EaseType.In);
        _healthTween.Parallel()
            .TweenRotationReset(_sprite, duration - inDuration)
            .SetEase(Tween.EaseType.In);
        _healthTween.Parallel()
            .TweenModulateReset(_sprite, duration - inDuration)
            .SetEase(Tween.EaseType.In);
    }

    private void PlayDamageEffect(float dangerLevel)
    {
        var sound = SoundManager.Instance.PlayPositionalSound(this, DamageSound);
        sound.PitchScale = Lerp(0.75f, 1.25f, dangerLevel);

        if (_sprite == null)
        {
            return;
        }

        const float duration = 0.25f;
        const float inDuration = duration / 4f;

        _healthTween?.Kill();
        _healthTween = _sprite.CreateTween().SetTrans(Tween.TransitionType.Cubic);

        _healthTween.TweenScale(_sprite, 1.2f, inDuration)
            .SetEase(Tween.EaseType.Out);
        _healthTween.Parallel()
            .TweenModulate(_sprite, 5f, inDuration)
            .SetEase(Tween.EaseType.Out);

        _healthTween.TweenScaleReset(_sprite, duration - inDuration)
            .SetEase(Tween.EaseType.In);
        _healthTween.Parallel()
            .TweenModulateReset(_sprite, duration - inDuration)
            .SetEase(Tween.EaseType.In);
    }

    public void Destroy()
    {
        if (IsInvulnerable)
        {
            return;
        }
        
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