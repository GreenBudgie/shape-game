public class MediumPolysteroidSize : PolysteroidSize
{

    public override Texture2D Texture => GD.Load<Texture2D>("uid://cyctlcsqycfd");
    public override PackedScene CollisionPolygonScene => GD.Load<PackedScene>("uid://d4mx1roxlpqnb");
    public override PackedScene AreaScene => GD.Load<PackedScene>("uid://bs351yavc0pmt");
    public override float Health => 3;
    public override float Gravity => 0.5f;
    public override float Mass => 0.75f;

}