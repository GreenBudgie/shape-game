public partial class RequirementLabel : Label
{
    public override void _Ready()
    {
        LevelManager.Instance.LevelStarted += OnLevelStarted;
    }

    private void OnLevelStarted()
    {
        Text = $"{LevelManager.Instance.RequireLevel().DestroyRequirement}";
    }
}