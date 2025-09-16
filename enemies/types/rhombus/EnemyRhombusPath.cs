public partial class EnemyRhombusPath : ClosedEnemyPath
{

    private static readonly PackedScene Scene = GD.Load<PackedScene>("uid://dofw2c36kh88s");

    protected override float MinYOffset => 300;
    protected override float MaxYOffset => 600;
    protected override float PathWidth => 2800;
    protected override float Speed => 0.1f;

    public static EnemyRhombusPath Create()
    {
        return Scene.Instantiate<EnemyRhombusPath>();
    }

}