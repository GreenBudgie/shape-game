public partial class DestroyProgressBar : TextureProgressBar
{

    private readonly ShakeTween _shakeTween = new ShakeTween()
        .TiltDelta(5f)
        .MaxTilt(5f)
        .SizeDelta(0.1f)
        .MaxSize(1.1f)
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

    private Glow _glow = null!;
    private Tween? _destroyProgressTween;
    private GpuParticles2D _particles = null!;

    public override void _Ready()
    {
        _particles = GetNode<GpuParticles2D>("Particles");

        _glow = Glow.AddGlow(this)
            .SetColor(ColorScheme.Red)
            .SetStrength(0.5f)
            .SetRadius(0);

        UpdateDestroyRequirement(0);
        UpdateDestroyProgress(0);

        LevelManager.Instance.LevelStarted += OnLevelStarted;
        LevelManager.Instance.DestroyProgressUpdated += OnDestroyProgressUpdated;
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

        var isCompleted = progress == LevelManager.Instance.DestroyRequirement;
        if (isCompleted)
        {
            return;
        }

        _shakeTween.Play(this);
        _glowTween.Play(_glow);
    }

}