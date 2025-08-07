public partial class DestroyProgressBar : TextureProgressBar
{

    private static readonly Vector2 Center = new(210, 42);

    private readonly ShakeTween _shakeTween = new ShakeTween()
        .FixedTilt(5f)
        .FixedSize(0.1f)
        .InTime(0.15f)
        .OutTime(0.4f);

    private readonly GlowTween _glowTween = new GlowTween()
        .MinStrength(1f)
        .StrengthDelta(1f)
        .MaxStrength(2f)
        .RadiusDelta(20f)
        .MaxRadius(20f)
        .InTime(0.15f)
        .OutTime(0.5f);
    
    private readonly ShakeTween _finalShakeTween = new ShakeTween()
        .FixedTilt(8f)
        .FixedSize(0.25f)
        .InTime(0.15f)
        .OutTime(0.6f);

    private readonly GlowTween _finalGlowTween = new GlowTween()
        .MinStrength(1f)
        .StrengthDelta(2f)
        .MaxStrength(2f)
        .MinRadius(40f)
        .RadiusDelta(50f)
        .MaxRadius(50f)
        .InTime(0.15f)
        .OutTime(0.6f);
    
    private readonly ShakeTween _completeShakeTween = new ShakeTween()
        .FixedTilt(3f)
        .FixedSize(0.05f)
        .InTime(0.1f)
        .OutTime(0.2f);

    private Glow _glow = null!;
    private Tween? _destroyProgressTween;
    private GpuParticles2D _particles = null!;
    private GpuParticles2D _finalParticles = null!;
    private double _completeShakeTimer;
    private bool _isCompleted;

    public override void _Ready()
    {
        _particles = GetNode<GpuParticles2D>("Particles");
        _finalParticles = GetNode<GpuParticles2D>("FinalParticles");

        _glow = Glow.AddGlow(this)
            .SetColor(ColorScheme.Red)
            .SetStrength(0.5f)
            .SetRadius(0);

        UpdateDestroyRequirement(0);
        UpdateDestroyProgress(0);

        LevelManager.Instance.LevelStarted += OnLevelStarted;
        LevelManager.Instance.DestroyProgressUpdated += OnDestroyProgressUpdated;
    }

    public override void _Process(double delta)
    {
        if (!_isCompleted)
        {
            return;
        }

        _completeShakeTimer -= delta;
        if (_completeShakeTimer <= 0)
        {
            PlayOccasionalCompleteEffect();
            ResetCompleteTimer();
        }
    }

    private void OnLevelStarted()
    {
        UpdateDestroyRequirement(LevelManager.Instance.DestroyRequirement);
        UpdateDestroyProgress(LevelManager.Instance.DestroyProgress);
    }

    private void OnDestroyProgressUpdated(int prevProgress, int newProgress)
    {
        UpdateDestroyProgress(newProgress, prevProgress < newProgress);
    }

    private void UpdateDestroyRequirement(int requirement)
    {
        MinValue = 0;
        MaxValue = requirement;
    }

    private void UpdateDestroyProgress(int progress, bool playEffect = false)
    {
        _isCompleted = progress == LevelManager.Instance.DestroyRequirement;
        _destroyProgressTween?.Kill();

        _destroyProgressTween = CreateTween().SetTrans(Tween.TransitionType.Sine);
        _destroyProgressTween.TweenProperty(
            @object: this,
            property: Range.PropertyName.Value.ToString(),
            finalVal: progress,
            duration: 0.5f
        ).SetEase(Tween.EaseType.Out);

        if (!playEffect)
        {
            return;
        }

        // Particles
        
        _particles.Emitting = true;
        
        const float maxEmissionWidth = 200;
        
        var realRatio = (float)(progress / MaxValue);
        _particles.AmountRatio = realRatio;
        
        var particlesPositionX = Center.X - (1 - realRatio) * maxEmissionWidth;
        _particles.Position = new Vector2(particlesPositionX, Center.Y);

        var particlesEmissionExtentsX = realRatio * maxEmissionWidth;
        var material = (ParticleProcessMaterial)_particles.ProcessMaterial;
        material.EmissionBoxExtents = new Vector3(
            particlesEmissionExtentsX,
            material.EmissionBoxExtents.Y,
            material.EmissionBoxExtents.Z
        );

        if (!_isCompleted)
        {
            _shakeTween.Play(this);
            _glowTween.Play(_glow);
            return;
        }
        
        _finalParticles.Amount = 30;
        _finalParticles.Emitting = true;
            
        _finalShakeTween.Play(this);
        _finalGlowTween.Play(_glow);

        ResetCompleteTimer();
    }

    private void ResetCompleteTimer()
    {
        const float minTime = 1f;
        const float maxTime = 5f;
        _completeShakeTimer = GD.RandRange(minTime, maxTime);
    }

    private void PlayOccasionalCompleteEffect()
    {
        _finalParticles.Amount = 5;
        _finalParticles.Emitting = true;
        _completeShakeTween.Play(this);
    }

}