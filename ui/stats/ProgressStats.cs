public partial class ProgressStats : Control
{

    private RichTextLabel _destroyRequirementLabel = null!;
    private RichTextLabel _destroyProgressLabel = null!;
    private TextureProgressBar _destroyProgress = null!;
    
    private RichTextLabel _surviveRequirementLabel = null!;
    private RichTextLabel _surviveProgressLabel = null!;
    private TextureProgressBar _surviveProgress = null!;

    public override void _Ready()
    {
        _destroyRequirementLabel = GetNode<RichTextLabel>("DestroyRequirementLabel");
        _destroyProgressLabel = GetNode<RichTextLabel>("DestroyProgressLabel");
        _destroyProgress = GetNode<TextureProgressBar>("DestroyProgress");
        
        _surviveRequirementLabel = GetNode<RichTextLabel>("SurviveRequirementLabel");
        _surviveProgressLabel = GetNode<RichTextLabel>("SurviveProgressLabel");
        _surviveProgress = GetNode<TextureProgressBar>("SurviveProgress");
        
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
        UpdateDestroyProgress(newProgress);
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
    
    private void UpdateDestroyProgress(int progress)
    {
        _destroyProgressLabel.Text = string.Empty;
        
        _destroyProgressLabel.PushColor(ColorScheme.Red);
        _destroyProgressLabel.AppendText(progress.ToString());
        _destroyProgressLabel.Pop();
        
        _destroyProgressLabel.AppendText(" / ");
        
        _destroyProgressLabel.PushColor(ColorScheme.Red);
        _destroyProgressLabel.AppendText(LevelManager.Instance.DestroyRequirement.ToString());
        _destroyProgressLabel.Pop();

        _destroyProgress.Value = progress;
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
        
        _surviveProgressLabel.AppendText(":");
        
        _surviveProgressLabel.PushColor(ColorScheme.LightGreen);
        _surviveProgressLabel.AppendText(remainingSeconds.ToString());
        _surviveProgressLabel.Pop();

        _surviveProgress.Value = progressSeconds;
    }

}