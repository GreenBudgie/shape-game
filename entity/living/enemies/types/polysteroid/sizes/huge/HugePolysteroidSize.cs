public class HugePolysteroidSize : PolysteroidSize
{

    public override Texture2D Texture => GD.Load<Texture2D>("uid://ds2374q7vmkm3");
    public override PackedScene CollisionPolygonScene => GD.Load<PackedScene>("uid://b02hlip5urj08");
    public override PackedScene AreaScene => GD.Load<PackedScene>("uid://c7nwwkcvaimyw");
    public override float Health => 8;
    public override float Gravity => 0.5f;
    public override float Mass => 1.25f;

}