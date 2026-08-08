public partial class PlayerManager : Node
{

    public static PlayerManager Instance { get; private set; } = null!;
    
    [Signal]
    public delegate void RespawnedEventHandler();
    
    [Signal]
    public delegate void DestroyedEventHandler();
    
    [Signal]
    public delegate void HealthChangedEventHandler(float health);
    
    public PlayerManager()
    {
        Instance = this;
    }
    
}