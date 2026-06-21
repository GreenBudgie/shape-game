[GlobalClass]
public partial class BarrierModule : SpawnableModule
{
    
    public override ISpawnable<Node2D> CreateSpawnable()
    {
        return Barrier.Create();
    }
    
}
