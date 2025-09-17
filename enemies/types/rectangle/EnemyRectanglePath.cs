public partial class EnemyRectanglePath : ClosedEnemyPath
{

    private static readonly PackedScene Scene = GD.Load<PackedScene>("uid://dipoelw83e5kl");

    protected override float MinYOffset => 300;
    protected override float MaxYOffset => 700;
    protected override float PathWidth => 2500;
    protected override float Speed => 0.15f;

    public static EnemyRectanglePath Create()
    {
        return Scene.Instantiate<EnemyRectanglePath>();
    }

}