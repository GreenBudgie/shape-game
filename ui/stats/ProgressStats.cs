public partial class ProgressStats : Control
{
    private RichTextLabel _destroyRequirementLabel = null!;
    private RichTextLabel _destroyProgressLabel = null!;

    private RichTextLabel _surviveRequirementLabel = null!;
    private RichTextLabel _surviveProgressLabel = null!;

    public override void _Ready()
    {
        _destroyRequirementLabel = GetNode<RichTextLabel>("DestroyRequirementLabel");
        _destroyProgressLabel = GetNode<RichTextLabel>("DestroyProgressLabel");

        _surviveRequirementLabel = GetNode<RichTextLabel>("SurviveRequirementLabel");
        _surviveProgressLabel = GetNode<RichTextLabel>("SurviveProgressLabel");

        UpdateDestroyRequirementLabel(0);
        UpdateSurviveRequirementLabel(0);
        UpdateSurviveProgress(0);

        LevelManager.Instance.LevelStarted += OnLevelStarted;
        LevelManager.Instance.DestroyProgressUpdated += OnDestroyProgressUpdated;
        LevelManager.Instance.SurviveProgressUpdated += OnSurviveProgressUpdated;
    }

    private void OnLevelStarted(Level level)
    {
        UpdateDestroyRequirementLabel(level.DestroyRequirement);
        UpdateSurviveRequirementLabel(level.SurviveRequirement);
        UpdateSurviveProgress(0);
    }

    private void OnDestroyProgressUpdated(int prevProgress, int newProgress)
    {
        UpdateDestroyProgress(newProgress);
    }

    private void UpdateDestroyProgress(int progress)
    {
        _destroyProgressLabel.Text = string.Empty;
        
        var level = LevelManager.Instance.Level;
        if (level == null)
        {
            return;
        }

        _destroyProgressLabel.PushColor(ColorScheme.Red);
        _destroyProgressLabel.AppendText(progress.ToString());
        _destroyProgressLabel.Pop();

        _destroyProgressLabel.AppendText(" / ");

        _destroyProgressLabel.PushColor(ColorScheme.Red);
        _destroyProgressLabel.AppendText(level.DestroyRequirement.ToString());
        _destroyProgressLabel.Pop();
    }

    private void OnSurviveProgressUpdated(int prevProgress, int newProgress)
    {
        UpdateSurviveProgress(newProgress);
    }

    private void UpdateDestroyRequirementLabel(int requirement)
    {
        _destroyRequirementLabel.Text = string.Empty;

        _destroyRequirementLabel.PushColor(ColorScheme.Red);
        _destroyRequirementLabel.AppendText(requirement.ToString());
        _destroyRequirementLabel.Pop();

        _destroyRequirementLabel.AppendText(" UNITS");
    }

    private void UpdateSurviveRequirementLabel(int requirementSeconds)
    {
        _surviveRequirementLabel.Text = string.Empty;

        _surviveRequirementLabel.PushColor(ColorScheme.LightGreen);
        _surviveRequirementLabel.AppendText(requirementSeconds.ToString());
        _surviveRequirementLabel.Pop();

        _surviveRequirementLabel.AppendText(" SEC");
    }

    private void UpdateSurviveProgress(int progressSeconds)
    {
        _surviveProgressLabel.Text = string.Empty;
        
        var level = LevelManager.Instance.Level;
        if (level == null)
        {
            return;
        }

        var remainingSecondsProgress = level.SurviveRequirement - progressSeconds;
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
    }
}