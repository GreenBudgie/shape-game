public partial class SurviveProgressBar : StatsProgressBar
{

    protected override Color Color => ColorScheme.LightGreen;
    protected override int Requirement => LevelManager.Instance.SurviveRequirementSeconds;
    protected override int Progress => LevelManager.Instance.SurviveProgressSeconds;
    protected override bool PlayProgressUpdateEffect => false;

    public override void _Ready()
    {
        base._Ready();
        LevelManager.Instance.SurviveProgressUpdated += OnProgressUpdated;
    }

}