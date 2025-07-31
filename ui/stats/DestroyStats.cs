using System;

public partial class DestroyStats : Control
{

    private RichTextLabel _destroyRequirementLabel = null!;
    private RichTextLabel _destroyProgressLabel = null!;
    private TextureProgressBar _destroyProgress = null!;

    public override void _Ready()
    {
        _destroyRequirementLabel = GetNode<RichTextLabel>("DestroyRequirementLabel");
        _destroyProgressLabel = GetNode<RichTextLabel>("DestroyProgressLabel");
        _destroyProgress = GetNode<TextureProgressBar>("DestroyProgress");
        
        UpdateRequirementLabel(0);
        UpdateProgress(0);
        
        LevelManager.Instance.LevelStarted += OnLevelStarted;
        LevelManager.Instance.DestroyProgressUpdated += OnDestroyProgressUpdated;
    }

    private void OnLevelStarted()
    {
        UpdateRequirementLabel(LevelManager.Instance.DestroyRequirement);
        UpdateProgress(LevelManager.Instance.DestroyProgress);
    }

    private void OnDestroyProgressUpdated(int prevProgress, int newProgress)
    {
        UpdateProgress(newProgress);
    }

    private void UpdateRequirementLabel(int requirement)
    {
        _destroyRequirementLabel.Text = string.Empty;
        
        _destroyRequirementLabel.PushColor(ColorScheme.Red);
        _destroyRequirementLabel.AppendText(requirement.ToString());
        _destroyRequirementLabel.Pop();
        
        _destroyRequirementLabel.AppendText(" UNITS");

        _destroyProgress.MinValue = 0;
        _destroyProgress.MaxValue = requirement;
    }
    
    private void UpdateProgress(int progress)
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

}