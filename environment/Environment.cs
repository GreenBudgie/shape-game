public partial class Environment : Node2D
{

    public static Environment Instance { get; private set; } = null!;

    public LevelBoundary Walls { get; private set; } = null!;
    public LevelBoundary Floor { get; private set; } = null!;
    public LevelBoundary Ceiling { get; private set; } = null!;

    public Environment()
    {
        Instance = this;
    }

    public override void _Ready()
    {
        Walls = GetNode<LevelBoundary>("LevelWalls");
        Floor = GetNode<LevelBoundary>("LevelFloor");
        Ceiling = GetNode<LevelBoundary>("LevelCeiling");

        GamePhaseManager.Instance.PhaseChanged += OnPhaseChange;
    }

    private void OnPhaseChange(GamePhase phase)
    {
        if (phase == GamePhase.Shop)
        {
            Walls.EnableGlowWithColor(ColorScheme.Yellow);
            Ceiling.EnableGlowWithColor(ColorScheme.Red);
            Ceiling.DisableCollisions();
            return;
        }
        
        if (phase == GamePhase.Level)
        {
            Walls.EnableGlowWithColor(ColorScheme.LightBlue);
            Ceiling.DisableGlow();
            Ceiling.EnableCollisions();
            Floor.EnableCollisions();
        }

        if (phase == GamePhase.LevelPreparation)
        {
            Walls.EnableGlowWithColor(ColorScheme.LightBlue);
            Ceiling.DisableGlow();
            Ceiling.EnableCollisions();
            Floor.DisableCollisions();
        }
    }

    private void OnPlayerLeftFloor()
    {
        Floor.EnableCollisions();
    }
}
