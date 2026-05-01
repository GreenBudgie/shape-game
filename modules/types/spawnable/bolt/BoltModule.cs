[GlobalClass]
public partial class BoltModule : SpawnableModule
{

    public override ISpawnable<Node2D> CreateSpawnable()
    {
        return BoltProjectile.Create();
    }

}