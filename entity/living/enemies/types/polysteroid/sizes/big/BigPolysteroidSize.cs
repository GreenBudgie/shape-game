public class BigPolysteroidSize : PolysteroidSize
{

    public override Texture2D Texture => GD.Load<Texture2D>("uid://1dpobmb33rix");
    public override PackedScene CollisionPolygonScene => GD.Load<PackedScene>("uid://bv080kd1u54xa");
    public override PackedScene AreaScene => GD.Load<PackedScene>("uid://cvptb1mnqp5ry");
    public override float Health => 5;
    public override float Gravity => 0.5f;
    public override float Mass => 1f;

}