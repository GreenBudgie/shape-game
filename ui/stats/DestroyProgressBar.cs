public partial class DestroyProgressBar : StatsProgressBar
{

    protected override Color Color => ColorScheme.Red;
    protected override int Requirement => LevelManager.Instance.DestroyRequirement;
    protected override int Progress => LevelManager.Instance.DestroyProgress;
    protected override bool PlayProgressUpdateEffect => true;

    public override void _Ready()
    {
        base._Ready();
        LevelManager.Instance.DestroyProgressUpdated += OnProgressUpdated;
    }
    
}