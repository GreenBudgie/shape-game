public partial class YinYang : Node2D, ISpawnable<YinYang>
{
    private const float PathRadius = 128f;

    public YinYang Node => this;

    private Node2D _yinFollowTarget = null!;
    private Node2D _yangFollowTarget = null!;
    private Sprite2D _pathSprite = null!;

    private float _yinRotation = 0;
    private float _yangRotation = Pi;

    private SpawnableContext _context = null!;
    private Vector2 _direction;
    private float _speed;

    private static readonly PackedScene Scene = GD.Load<PackedScene>("uid://cfienje1goost");

    public static YinYang Create()
    {
        var node = Scene.Instantiate<YinYang>();
        return node;
    }

    public void Prepare(SpawnableContext context)
    {
        _context = context;
    }

    public override void _Ready()
    {
        _direction = _context.Direction;
        _speed = _context.CalculateStat<SpeedStat>();

        _pathSprite = GetNode<Sprite2D>("Path");
        _yinFollowTarget = GetNode<Node2D>("YinFollowTarget");
        _yangFollowTarget = GetNode<Node2D>("YangFollowTarget");

        var yinSphere = YinYangSphere.Create(YinYangType.Yin, _yinFollowTarget);
        var yangSphere = YinYangSphere.Create(YinYangType.Yang, _yangFollowTarget);

        yinSphere.OtherSphere = yangSphere;
        yangSphere.OtherSphere = yinSphere;

        SpawnSphere(yinSphere, _yinFollowTarget);
        SpawnSphere(yangSphere, _yangFollowTarget);
    }

    private void SpawnSphere(YinYangSphere sphere, Node2D followTarget)
    {
        var context = new SpawnableContext(sphere)
        {
            Position = followTarget.GlobalPosition,
            Source = this,
            OriginalSource = _context.OriginalSource
        };

        context.Stats.AddRange(_context.Stats);

        context.Spawn();
    }

    public override void _Process(double delta)
    {
        const float rotationSpeed = 3f;

        _yinRotation = (_yinRotation + rotationSpeed * (float)delta) % Tau;
        _yangRotation = (_yangRotation + rotationSpeed * (float)delta) % Tau;

        _yinFollowTarget.Position = Vector2.FromAngle(_yinRotation) * PathRadius;
        _yangFollowTarget.Position = Vector2.FromAngle(_yangRotation) * PathRadius;

        GlobalPosition += (float)delta * _speed * _direction;

        _pathSprite.Rotation -= rotationSpeed * (float)delta / 2f;
    }

    public void Remove()
    {
        QueueFree();
    }
}