public partial class ProgressStats : Control
{

    private RichTextLabel _destroyRequirementLabel = null!;
    private RichTextLabel _destroyProgressLabel = null!;
    private TextureProgressBar _destroyProgress = null!;
    
    private RichTextLabel _surviveRequirementLabel = null!;
    private RichTextLabel _surviveProgressLabel = null!;
    private TextureProgressBar _surviveProgress = null!;

    private Glow _destroyProgressGlow = null!;
    
    public override void _Ready()
    {
        _destroyRequirementLabel = GetNode<RichTextLabel>("DestroyRequirementLabel");
        _destroyProgressLabel = GetNode<RichTextLabel>("DestroyProgressLabel");
        _destroyProgress = GetNode<TextureProgressBar>("DestroyProgress");
        
        _surviveRequirementLabel = GetNode<RichTextLabel>("SurviveRequirementLabel");
        _surviveProgressLabel = GetNode<RichTextLabel>("SurviveProgressLabel");
        _surviveProgress = GetNode<TextureProgressBar>("SurviveProgress");

        _destroyProgressGlow = Glow.AddGlow(_destroyProgress)
            .SetColor(ColorScheme.Red)
            .SetStrength(0.5f)
            .SetRadius(0);
        
        UpdateDestroyRequirementLabel(0);
        UpdateDestroyProgress(0);
        UpdateSurviveRequirementLabel(0);
        UpdateSurviveProgress(0);
        
        LevelManager.Instance.LevelStarted += OnLevelStarted;
        LevelManager.Instance.DestroyProgressUpdated += OnDestroyProgressUpdated;
        LevelManager.Instance.SurviveProgressUpdated += OnSurviveProgressUpdated;
    }

    private void OnLevelStarted()
    {
        UpdateDestroyRequirementLabel(LevelManager.Instance.DestroyRequirement);
        UpdateDestroyProgress(LevelManager.Instance.DestroyProgress);
        UpdateSurviveRequirementLabel(LevelManager.Instance.SurviveRequirementSeconds);
        UpdateSurviveProgress(LevelManager.Instance.SurviveProgressSeconds);
    }

    private void OnDestroyProgressUpdated(int prevProgress, int newProgress)
    {
        UpdateDestroyProgress(newProgress, prevProgress < newProgress);
    }
    
    private void OnSurviveProgressUpdated(float prevProgress, float newProgress)
    {
        UpdateSurviveProgress(newProgress);
    }

    private void UpdateDestroyRequirementLabel(int requirement)
    {
        _destroyRequirementLabel.Text = string.Empty;
        
        _destroyRequirementLabel.PushColor(ColorScheme.Red);
        _destroyRequirementLabel.AppendText(requirement.ToString());
        _destroyRequirementLabel.Pop();
        
        _destroyRequirementLabel.AppendText(" SEC");

        _destroyProgress.MinValue = 0;
        _destroyProgress.MaxValue = requirement;
    }

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

    private Tween? _destroyProgressTween;
    
    private void UpdateDestroyProgress(int progress, bool playEffect = false)
    {
        _destroyProgressLabel.Text = string.Empty;
        
        _destroyProgressLabel.PushColor(ColorScheme.Red);
        _destroyProgressLabel.AppendText(progress.ToString());
        _destroyProgressLabel.Pop();
        
        _destroyProgressLabel.AppendText(" / ");
        
        _destroyProgressLabel.PushColor(ColorScheme.Red);
        _destroyProgressLabel.AppendText(LevelManager.Instance.DestroyRequirement.ToString());
        _destroyProgressLabel.Pop();

        _destroyProgressTween?.Kill();

        _destroyProgressTween = _destroyProgress.CreateTween().SetTrans(Tween.TransitionType.Sine);
        _destroyProgressTween.TweenProperty(
            @object: _destroyProgress,
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

        _shakeTween.Play(_destroyProgress);
        _glowTween.Play(_destroyProgressGlow);
    }
    
    private void UpdateSurviveRequirementLabel(int requirementSeconds)
    {
        _surviveRequirementLabel.Text = string.Empty;
        
        _surviveRequirementLabel.PushColor(ColorScheme.LightGreen);
        _surviveRequirementLabel.AppendText(requirementSeconds.ToString());
        _surviveRequirementLabel.Pop();
        
        _surviveRequirementLabel.AppendText(" SEC");

        _surviveProgress.MinValue = 0;
        _surviveProgress.MaxValue = requirementSeconds;
    }
    
    private void UpdateSurviveProgress(float progressSeconds)
    {
        _surviveProgressLabel.Text = string.Empty;

        var remainingSecondsProgress = LevelManager.Instance.SurviveRequirementSeconds - FloorToInt(progressSeconds);
        var remainingMinutes = remainingSecondsProgress / 60;
        var remainingSeconds = remainingSecondsProgress % 60;
        
        _surviveProgressLabel.PushColor(ColorScheme.LightGreen);
        _surviveProgressLabel.AppendText(remainingMinutes.ToString());
        _surviveProgressLabel.Pop();
        
        _surviveProgressLabel.AppendText(" : ");

        var formattedSeconds = remainingSeconds < 10 ? $"0{remainingSeconds}" : remainingSeconds.ToString();
        _surviveProgressLabel.PushColor(ColorScheme.LightGreen);
        _surviveProgressLabel.AppendText(formattedSeconds);
        _surviveProgressLabel.Pop();

        _surviveProgress.Value = progressSeconds;
    }

}