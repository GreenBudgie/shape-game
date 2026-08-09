public partial class HealthBar : TextureProgressBar
{
    
    private Glow _glow = null!;
    
    public override void _Ready()
    {
        _glow = Glow.AddGlow(this)
            .SetColor(ColorScheme.Red)
            .SetStrength(2)
            .SetRadius(0)
            .EnablePulsing()
            .SetPulseStrengthDelta(1f);
        
        ShapeGame.Instance.PostSetup += PostSetup;

        PlayerManager.Instance.HealthChanged += OnHealthChanged;
        PlayerManager.Instance.Destroyed += UpdateHealth;
        PlayerManager.Instance.Respawned += UpdateHealth;
    }

    private void PostSetup()
    {
        UpdateHealth();   
    }
    
    private void OnHealthChanged(float health)
    {
        UpdateHealth();
    }

    private Tween? _animationTween;

    private void UpdateHealth()
    {
        var player = Player.FindPlayer();
        float health;
        if (player != null)
        {
            MaxValue = player.HealthController.MaxHealth;
            health = player.HealthController.Health;
        }
        else
        {
            MaxValue = 1;
            health = 0;
        }

        const float dangerThreshold = 0.5f;
        var rawRatio = (float)Clamp(MaxValue > 0 ? health / MaxValue : 0, 0, 1);
        var ratio = 1f - Clamp(rawRatio * (1 / dangerThreshold), 0, 1);
        
        _glow?
            .SetRadius(40f * ratio)
            .SetStrength(2f * ratio)
            .SetPulseRadiusDelta(20f * ratio)
            .SetPulseStrengthDelta(ratio)
            .SetPulsesPerSecond(1f + ratio);
        
        const float startDuration = 0.1f;
        const float endDuration = 0.2f;

        _animationTween?.Kill();
        _animationTween = CreateTween().SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Quad);
        _animationTween.TweenRangeValue(this, health, 0.25f);
        _animationTween.Parallel()
            .TweenOffsetScale(this, (float)GD.RandRange(1.04f, 1.08f), startDuration);
        _animationTween.Parallel()
            .TweenOffsetRotation(this, RandomUtils.RandomSignedDeltaRange(0.1f, 0.05f), startDuration);
        
        _animationTween.TweenOffsetScaleReset(this, endDuration);
        _animationTween.Parallel().TweenOffsetRotationReset(this, endDuration);
    }
}
