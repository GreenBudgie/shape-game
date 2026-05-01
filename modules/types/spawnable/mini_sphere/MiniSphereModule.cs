[GlobalClass]
public partial class MiniSphereModule : SpawnableModule
{

    public override ISpawnable<Node2D> CreateSpawnable()
    {
        return MiniSphereProjectile.Create();
    }

}