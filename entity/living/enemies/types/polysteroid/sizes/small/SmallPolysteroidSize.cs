public class SmallPolysteroidSize : PolysteroidSize
{

    public override Texture2D Texture => GD.Load<Texture2D>("uid://c4ga3ws0xv1tu");
    public override PackedScene CollisionPolygonScene => GD.Load<PackedScene>("uid://812qiyrwy8te");
    public override PackedScene AreaScene => GD.Load<PackedScene>("uid://dmqo324pfopkr");
    public override float Health => 1;
    public override float Gravity => 0.5f;
    public override float Mass => 0.5f;

}