public partial class ProgressText : Label
{
    public override void _Ready()
    {
        LevelManager.Instance.DestroyProgressUpdated += OnProgressUpdated;
    }
    
    private void OnProgressUpdated(int prevProgress, int newProgress)
    {
        Text = $"{newProgress}";
    }
}
