public partial class NextLevelArea : Area2D
{
    public override void _Ready()
    {
        BodyEntered += PrepareNextLevel;
    }

    private void PrepareNextLevel(Node _)
    {
        if (GamePhaseManager.Instance.Phase == GamePhase.Shop)
        {
            LevelManager.Instance.PrepareNextLevel();
        }
    }
}
