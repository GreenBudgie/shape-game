public partial class DestroyProgressBar : StatsProgressBar
{

    protected override Color Color => ColorScheme.Red;
    protected override int Requirement => LevelManager.Instance.Level?.DestroyRequirement ?? 0;
    protected override int Progress => LevelManager.Instance.DestroyProgress;
    protected override bool PlayProgressUpdateEffect => true;

    public override void _Ready()
    {
        base._Ready();
        LevelManager.Instance.DestroyProgressUpdated += OnProgressUpdated;
    }
    
}