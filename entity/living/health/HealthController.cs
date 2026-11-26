using System.Collections.Generic;
using System.Globalization;

public partial class HealthController(Entity owner) : Node2D
{
    private const float HitScale = 1.2f;
    private const float DeathScale = 1.75f;

    [Signal]
    public delegate void HealthChangedEventHandler(float delta);

    [Signal]
    public delegate void DestroyedEventHandler();

    [Signal]
    public delegate void DestroyAnimationFinishedEventHandler();
    
    public Entity OwnerEntity { get; } = owner;

    public float MaxHealth { get; set; }

    /// <summary>
    /// Health, always between 0 and MaxHealth
    /// </summary>
    public float Health { get; private set; }

    /// <summary>
    /// When true, damage will never be received. However, healing will still work.
    /// Any damage will be treated as 0.
    /// </summary>
    public bool IsInvulnerable { get; set; }

    public float InvulnerabilityPeriod { get; set; } = 0f;

    private float _glowExtent;
    private Tween? _healthTween;
    private readonly Dictionary<DamageSource, double> _invulnerableUntil = [];
    private double _elapsedTime;

    public override void _Ready()
    {
        Health = MaxHealth;
        UpdateGlow(0f);
        GetViewport().SizeChanged += UpdateGlowMargin;
    }

    public override void _Process(double delta)
    {
        _elapsedTime += delta;
    }

    private void StopInvulnerabilityPeriod()
    {
        _invulnerableUntil.Clear();
    }

    /// <summary>
    /// Changes max health and immediately changes Health to the new value. Does not count as damaging/healing.
    /// </summary>
    public void ChangeMaxHealthImmediately(float maxHealth)
    {
        MaxHealth = maxHealth;
        Health = maxHealth;
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
        ChangeHealth(null, MaxHealth - Health);
    }

    /**
     * Inflicts a specified amount of damage. Negative damage means healing.
     * The same as ChangeHealth(-damage), but more convenient to use since negation is not needed.
     */
    public void Damage(DamageSource? source, float damage)
    {
        ChangeHealth(source, -damage);
    }

    /**
     * Heals by specified amount. Negative healing means damage.
     * The same as ChangeHealth(heal), but more readable in some situations.
     */
    public void Heal(float heal)
    {
        ChangeHealth(null, heal);
    }

    /**
     * Changes health by delta, plays effects and sound. Negative delta is considered damage, positive - healing.
     */
    private void ChangeHealth(DamageSource? source, float delta)
    {
        if (IsDestroyed())
        {
            return;
        }

        if (delta < 0 && IsInvulnerableTo(source))
        {
            // Invulnerability period is active, do nothing on damage
            return;
        }

        CreateHealthLabel(delta);

        if (!IsInvulnerable || delta >= 0)
        {
            Health = Clamp(Health + delta, 0, MaxHealth);
        }
        
        if (delta < 0 && IsZeroApprox(InvulnerabilityPeriod) && source.HasValue)
        {
            _invulnerableUntil[source.Value] = _elapsedTime + InvulnerabilityPeriod;
        }

        EmitSignalHealthChanged(delta);

        if (IsDestroyed())
        {
            ForceDestroy();
            return;
        }

        var dangerLevel = 1f - GetHealthRatio();

        UpdateGlow(dangerLevel);
        PlayHealthEffect(dangerLevel, delta > 0f);
    }

    private void UpdateGlow(float dangerLevel)
    {
        var effects = OwnerEntity.Effects;
        var glowing = dangerLevel > 0f;
        var radius = 40f * dangerLevel;
        var pulseRadius = glowing ? 10f + 20f * dangerLevel : 0f;
        effects.GlowEnabled = glowing;
        effects.GlowRadius = radius;
        effects.GlowStrength = 2f * dangerLevel;
        effects.PulseRadiusDelta = pulseRadius;
        effects.PulseStrengthDelta = glowing ? 0.65f + dangerLevel : 0f;
        effects.PulsesPerSecond = 1f + 2f * dangerLevel;
        _glowExtent = (radius + pulseRadius) * HitScale;
        UpdateGlowMargin();
    }

    private void UpdateGlowMargin()
    {
        var transform = OwnerEntity.GetViewportTransform() * OwnerEntity.GlobalTransform;
        OwnerEntity.VisualEffectsGroup.FitMargin = Ceil(_glowExtent * Max(transform.X.Length(), transform.Y.Length())) + 4f;
    }

    private void CreateHealthLabel(float healthDelta)
    {
        var sign = healthDelta > 0 ? "+" : "";
        var color = Sign(healthDelta) switch
        {
            -1 => ColorScheme.Red,
            0 => ColorScheme.Yellow,
            _ => ColorScheme.LightGreen
        };

        string healthToDisplay; 
        if (Abs(healthDelta) < 1)
        {
            if (IsEqualApprox(healthDelta, 0f))
            {
                healthToDisplay = "0";
            }
            else
            {
                healthToDisplay = healthDelta.ToString("0.#", CultureInfo.InvariantCulture);
            }
        }
        else
        {
            healthToDisplay = RoundToInt(healthDelta).ToString();
        }

        var position = OwnerEntity.Area.GetRandomGlobalPoint();
        var text = sign + healthToDisplay;
        
        PopupLabel.Create(position, text).SetColor(color);
    }

    private void PlayHealthEffect(float dangerLevel, bool healing)
    {
        var sound = SoundManager.Instance.PlayPositionalSound(this, healing ? HealthSounds.HealSound : HealthSounds.DamageSound);
        sound.PitchScale = Lerp(0.75f, 1.25f, dangerLevel);

        const float duration = 0.25f;
        const float inDuration = duration / 4f;
        var group = OwnerEntity.VisualEffectsGroup;
        var material = OwnerEntity.Effects.Material;
        var tint = healing ? Colors.LightGreen * 5f : new Color(5f, 5f, 5f);

        _healthTween?.Kill();
        _healthTween = group.CreateTween().SetTrans(Tween.TransitionType.Cubic);
        _healthTween.TweenScale(group, HitScale, inDuration).SetEase(Tween.EaseType.Out);
        _healthTween.Parallel().TweenProperty(material, VisualEffects.BaseTintProperty, tint, inDuration)
            .SetEase(Tween.EaseType.Out);
        if (healing)
        {
            _healthTween.Parallel().TweenRotationDegrees(group, 20f, inDuration).SetEase(Tween.EaseType.Out);
        }

        const float outDuration = duration - inDuration;
        _healthTween.TweenScaleReset(group, outDuration).SetEase(Tween.EaseType.In);
        _healthTween.Parallel().TweenProperty(material, VisualEffects.BaseTintProperty, Colors.White, outDuration)
            .SetEase(Tween.EaseType.In);
        if (healing)
        {
            _healthTween.Parallel().TweenRotationReset(group, outDuration).SetEase(Tween.EaseType.In);
        }
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

    private bool IsInvulnerableTo(DamageSource? source)
    {
        if (IsInvulnerable)
        {
            return true;
        }
        
        return source.HasValue
               && _invulnerableUntil.TryGetValue(source.Value, out var until)
               && until > _elapsedTime;
    }

    private void ForceDestroy()
    {
        Health = 0;
        EmitSignalDestroyed();

        SoundManager.Instance.PlayPositionalSound(this, HealthSounds.DestroySound);
        const float duration = 0.4f;
        
        _healthTween?.Kill();
        var group = OwnerEntity.VisualEffectsGroup;
        var effects = OwnerEntity.Effects;
        effects.PulseEnabled = false;
        effects.DissolveEnabled = true;
        _glowExtent = effects.GlowRadius * DeathScale;
        UpdateGlowMargin();

        var destroyTween = group.CreateTween().SetParallel();
        destroyTween.TweenProperty(effects.Material, VisualEffects.GlowColorAlphaProperty, 0f, duration);
        destroyTween.TweenProperty(effects.Material, VisualEffects.DissolveValueProperty, 0f, duration);
        destroyTween.TweenProperty(effects.Material, VisualEffects.BaseTintProperty, new Color(5f, 5f, 5f, 0f), duration)
            .SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.Out);
        destroyTween.TweenScale(group, DeathScale, duration)
            .SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.Out);
        destroyTween.Finished += EmitSignalDestroyAnimationFinished;
    }
}
