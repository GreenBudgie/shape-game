[GlobalClass]
public partial class MineModule : SpawnableModule
{

    public override ISpawnable<Node2D> CreateSpawnable()
    {
        return MineProjectile.Create();
    }

}