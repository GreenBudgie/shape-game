public partial class YinYang : Node2D, ISpawnable<YinYang>
{
    private const float PathRadius = 128f;
    const float RotationSpeedMin = 7f;
    const float RotationSpeedMax = 9f;

    public YinYang Node => this;

    private Node2D _yinFollowTarget = null!;
    private Node2D _yangFollowTarget = null!;
    private YinYangSphere _yinSphere = null!;
    private YinYangSphere _yangSphere = null!;

    private float _yinRotation;
    private float _yangRotation = Pi;

    private SpawnableContext _context = null!;
    private Vector2 _direction;
    private float _speed;
    private float _rotationSpeed;

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
        _rotationSpeed = (float)GD.RandRange(RotationSpeedMin, RotationSpeedMax) * RandomUtils.RandomSign();

        _yinFollowTarget = GetNode<Node2D>("YinFollowTarget");
        _yangFollowTarget = GetNode<Node2D>("YangFollowTarget");

        _yinSphere= YinYangSphere.Create(YinYangType.Yin, _yinFollowTarget);
        _yangSphere = YinYangSphere.Create(YinYangType.Yang, _yangFollowTarget);

        _yinSphere.OtherSphere = _yangSphere;
        _yangSphere.OtherSphere = _yinSphere;

        _yinSphere.TreeExiting += Remove;
        _yangSphere.TreeExiting += Remove;

        SpawnSphere(_yinSphere, _yinFollowTarget);
        SpawnSphere(_yangSphere, _yangFollowTarget);
    }

    private void SpawnSphere(YinYangSphere sphere, Node2D followTarget)
    {
        var context = new SpawnableContext(sphere);
        
        context.InheritFrom(_context);
        context.Position = followTarget.GlobalPosition;

        context.Spawn();
    }

    public override void _Process(double delta)
    {
        _yinRotation = (_yinRotation + _rotationSpeed * (float)delta) % Tau;
        _yangRotation = (_yangRotation + _rotationSpeed * (float)delta) % Tau;

        _yinFollowTarget.Position = Vector2.FromAngle(_yinRotation) * PathRadius;
        _yangFollowTarget.Position = Vector2.FromAngle(_yangRotation) * PathRadius;

        GlobalPosition += (float)delta * _speed * _direction;
    }

    private bool _isRemoving;

    public void Remove()
    {
        if (_isRemoving)
        {
            return;
        }
        
        _isRemoving = true;

        _yinSphere.Detach();
        _yangSphere.Detach();
        
        QueueFree();
    }
}