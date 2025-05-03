public partial class EnemySquare : Enemy
{

    private static readonly PackedScene BulletScene = GD.Load<PackedScene>("uid://bhj8dgeytmpxx");
    private static readonly PackedScene PathScene = GD.Load<PackedScene>("uid://b1ehfaspqd28s");

    private const double FireDelay = 1;

    private double _fireTimer = FireDelay;

    private EnemySquarePath _path = null!;

    public override void _Ready()
    {
        _path = PathScene.Instantiate<EnemySquarePath>();
        ShapeGame.Instance.CallDeferred(Node.MethodName.AddChild, _path);
    }

    public override void _Process(double delta)
    {
        var direction = GlobalPosition.DirectionTo(_path.PathPoint.GlobalPosition);
        var distance = GlobalPosition.DistanceTo(_path.PathPoint.GlobalPosition);
        var followSpeed = distance * 10;
        ApplyCentralForce(direction * followSpeed);
        
        if (_fireTimer <= 0)
        {
            Fire();
            _fireTimer = FireDelay;
        }
        else
        {
            _fireTimer -= delta;
        }
    }

    private void Fire()
    {
        var bullet = BulletScene.Instantiate<EnemySquareBullet>();
        bullet.Position = Position;
        GetParent().AddChild(bullet);
    }
}