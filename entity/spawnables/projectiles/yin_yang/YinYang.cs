public partial class YinYang : Node2D, ISpawnable<YinYang>
{
    private const float PathRadius = 128f;
    const float RotationSpeed = 6f;

    public YinYang Node => this;

    private Node2D _yinFollowTarget = null!;
    private Node2D _yangFollowTarget = null!;
    private YinYangSphere _yinSphere = null!;
    private YinYangSphere _yangSphere = null!;
    private Sprite2D _pathSprite = null!;

    private float _yinRotation;
    private float _yangRotation = Pi;

    private SpawnableContext _context = null!;
    private Vector2 _direction;
    private float _speed;

    private Tween? _pathAnimationTween;

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

    private Beam _beam = null!;

    public override void _Ready()
    {
        _direction = _context.Direction;
        _speed = _context.CalculateStat<SpeedStat>();

        _pathSprite = GetNode<Sprite2D>("Path");
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

        _pathSprite.Modulate = Colors.Transparent;

        _pathAnimationTween = CreateTween().SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.Out);
        _pathAnimationTween.TweenModulate(_pathSprite, new Color(1, 1, 1, 0.75f), 0.1f);

        _beam = Beam.Create()
            .SetFromTo(_yinSphere.GlobalPosition, _yangSphere.GlobalPosition)
            .SetEnergy(2)
            .SetProgress(1)
            .SetOutlineThickness(5)
            .SetBeamCount(2)
            .SetOutlineColor(ColorScheme.Yellow)
            .SetBeamColor(ColorScheme.Yellow.Lightened(0.5f));
        
        ShapeGame.Instance.AddChild(_beam);
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
        _yinRotation = (_yinRotation + RotationSpeed * (float)delta) % Tau;
        _yangRotation = (_yangRotation + RotationSpeed * (float)delta) % Tau;

        _yinFollowTarget.Position = Vector2.FromAngle(_yinRotation) * PathRadius;
        _yangFollowTarget.Position = Vector2.FromAngle(_yangRotation) * PathRadius;

        GlobalPosition += (float)delta * _speed * _direction;

        _pathSprite.Rotation -= RotationSpeed * (float)delta / 2f;

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
        
        _pathAnimationTween?.Kill();
        _pathAnimationTween = CreateTween().SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.Out);
        _pathAnimationTween.FadeOut(_pathSprite, 0.1f);
        _pathAnimationTween.Parallel().TweenScale(_pathSprite, 1.3f, 0.1f);

        _pathAnimationTween.Finished += QueueFree;
        
        _beam.QueueFree();
    }
}