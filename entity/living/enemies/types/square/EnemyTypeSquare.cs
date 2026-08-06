public class EnemyTypeSquare : EnemyType
{
    public override PackedScene Scene => GD.Load<PackedScene>("uid://csm3f5807x7i8");
    public override string Name => "Square";
}