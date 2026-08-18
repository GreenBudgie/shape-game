public partial class Environment : Node2D
{

    private LevelBoundary _walls = null!;
    private LevelBoundary _floor = null!;
    private LevelBoundary _ceiling = null!;

    public override void _Ready()
    {
        _walls = GetNode<LevelBoundary>("LevelWalls");
        _floor = GetNode<LevelBoundary>("LevelFloor");
        _ceiling = GetNode<LevelBoundary>("LevelCeiling");

        GamePhaseManager.Instance.PhaseChanged += OnPhaseChange;
    }

    private void OnPhaseChange(GamePhase phase)
    {
        if (phase == GamePhase.Shop)
        {
            _walls.EnableGlowWithColor(ColorScheme.Yellow);
            _ceiling.EnableGlowWithColor(ColorScheme.Red);
            return;
        }
        
        if (phase == GamePhase.Level)
        {
            _walls.EnableGlowWithColor(ColorScheme.LightBlue);
            _ceiling.DisableGlow();
        }
    }
}
