[GlobalClass]
public partial class YinYangModule : SpawnableModule
{

    public override ISpawnable<Node2D> CreateSpawnable()
    {
        return YinYang.Create();
    }

}