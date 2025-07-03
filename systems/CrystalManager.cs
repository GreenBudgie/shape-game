public partial class CrystalManager : Node
{

    [Signal]
    public delegate void CrystalCollectedEventHandler();

    public static CrystalManager Instance { get; private set; } = null!;

    public int Crystals { get; private set; }

    public override void _EnterTree()
    {
        Instance = this;
    }

    public void CollectCrystal()
    {
        Crystals++;
        EmitSignalCrystalCollected();
    }

}