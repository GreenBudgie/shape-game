public partial class PlayerInventory : ModuleInventory
{

    public static PlayerInventory Instance { get; private set; } = null!;

    public PlayerInventory()
    {
        Instance = this;
    }
    
}