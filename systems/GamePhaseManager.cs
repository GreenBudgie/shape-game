public partial class GamePhaseManager : Node
{
    public static GamePhaseManager Instance { get; private set; } = null!;

    [Signal]
    public delegate void PhaseChangedEventHandler(GamePhase phase);
    
    public GamePhase Phase { get; private set; } = GamePhase.Level;
    
    public GamePhaseManager()
    {
        Instance = this;
    }
    
    public void ChangePhase(GamePhase newPhase)
    {
        Phase = newPhase;
        EmitSignalPhaseChanged(newPhase);
    }
    
}