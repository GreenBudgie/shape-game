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

    private const float BeamAnimationDuration = 0.15f;
    private Tween? _beamAnimationTween;
    private Beam _beam = null!;

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

        _beam = Beam.Create()
            .SetFromTo(_yinSphere.GlobalPosition, _yangSphere.GlobalPosition)
            .SetEnergy(2)
            .SetProgress(0)
            .SetOutlineThickness(5)
            .SetThickness(20)
            .SetBeamCount(3)
            .SetOutlineColor(ColorScheme.Yellow)
            .SetBeamColor(ColorScheme.Yellow.Lightened(0.5f));
        
        ShapeGame.Instance.AddChild(_beam);
        
        _beamAnimationTween = _beam.CreateTween().SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Quad);
        _beamAnimationTween
            .TweenProperty(_beam.ShaderMaterial, Beam.ProgressShaderParam, 1, BeamAnimationDuration);
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
        _yinRotation = (_yinRotation + _rotationSpeed * (float)delta) % Tau;
        _yangRotation = (_yangRotation + _rotationSpeed * (float)delta) % Tau;

        _yinFollowTarget.Position = Vector2.FromAngle(_yinRotation) * PathRadius;
        _yangFollowTarget.Position = Vector2.FromAngle(_yangRotation) * PathRadius;

        GlobalPosition += (float)delta * _speed * _direction;

        if (IsInstanceValid(_yinSphere) && IsInstanceValid(_yangSphere))
        {
            _beam.SetFromTo(_yinSphere.GlobalPosition, _yangSphere.GlobalPosition);
        }
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
        
        _beamAnimationTween?.Kill();
        _beamAnimationTween = _beam.CreateTween().SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Quad);
        _beamAnimationTween
            .TweenProperty(_beam.ShaderMaterial, Beam.ProgressShaderParam, 0, BeamAnimationDuration);
     
        _beamAnimationTween.Finished += FullyRemove;
    }

    private void FullyRemove()
    {
        QueueFree();
        _beam.QueueFree();
    }
}