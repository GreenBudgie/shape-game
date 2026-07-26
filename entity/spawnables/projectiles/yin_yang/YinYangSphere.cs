public partial class YinYangSphere : BasicRigidBodyProjectile<YinYangSphere>
{

    private static readonly Texture2D YinTexture = GD.Load<Texture2D>("uid://dndyopr6137m3");
    private static readonly Texture2D YangTexture = GD.Load<Texture2D>("uid://bye8t1ytl68gx");
    
    public override YinYangSphere Node => this;

    public YinYangSphere OtherSphere { get; set; } = null!;

    private YinYangType _type;
    private Node2D _followTarget = null!;

    private static readonly PackedScene Scene = GD.Load<PackedScene>("uid://3bwof5vbs2i3");

    public static YinYangSphere Create(YinYangType type, Node2D followTarget)
    {
        var node = Scene.Instantiate<YinYangSphere>();
        node._type = type;
        node._followTarget = followTarget;
        return node;
    }

    public override void _Ready()
    {
        var texture = _type == YinYangType.Yin ? YinTexture : YangTexture;
        GetNode<Sprite2D>("Sprite2D").Texture = texture;
    }

    public override void _PhysicsProcess(double delta)
    {
        var direction = GlobalPosition.DirectionTo(_followTarget.GlobalPosition);
        var distance = GlobalPosition.DistanceTo(_followTarget.GlobalPosition);

        var followSpeed = 100f;

        ApplyCentralForce(direction * distance * followSpeed);
    }
}
