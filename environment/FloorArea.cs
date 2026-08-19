public partial class FloorArea : Area2D
{

    private uint _initialCollisionMask;
    
    public override void _Ready()
    {
        _initialCollisionMask = CollisionMask;
        CollisionMask = 0;
        
        GamePhaseManager.Instance.PhaseChanged += OnPhaseChanged;
        BodyExited += OnPlayerLeft;
    }

    private void OnPhaseChanged(GamePhase phase)
    {
        if (phase == GamePhase.LevelPreparation)
        {
            CollisionMask = _initialCollisionMask;
            return;
        }

        CollisionMask = 0;
    }

    private void OnPlayerLeft(Node _)
    {
        if (GamePhaseManager.Instance.Phase == GamePhase.LevelPreparation)
        {
            LevelManager.Instance.StartNextLevel();
        }
    }
}
