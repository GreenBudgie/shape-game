public partial class Pointer : Sprite2D
{
    
    private const float DistanceToEdge = 20;
    private const float MinScale = 0.35f;
    private const float SpawnDuration = 0.3f;
    private const float RemoveDuration = 0.3f;

    private static readonly PackedScene Scene = GD.Load<PackedScene>("uid://kmwq84squv6w");

    private CollisionObject2D _target = null!;
    private Color _color;

    public static Pointer Create(CollisionObject2D target, Color color)
    {
        var node = Scene.Instantiate<Pointer>();
        node._target = target;
        node._color = color;
        node.Modulate = color.AsTransparent();
        node.UpdatePositionAndScale();
        node.Offset = new Vector2(0, -DistanceToEdge);
        return node;
    }

    private Tween? _spawnTween;

    public override void _Ready()
    {
        _spawnTween = CreateTween().SetParallel();

        _spawnTween.TweenProperty(this, OffsetProperty, Vector2.Zero, SpawnDuration)
            .SetEase(Tween.EaseType.Out)
            .SetTrans(Tween.TransitionType.Quad);
        _spawnTween.TweenProperty(this, ModulateProperty, _color, SpawnDuration)
            .SetEase(Tween.EaseType.Out)
            .SetTrans(Tween.TransitionType.Quad);
    }

    public override void _Process(double delta)
    {
        if (_isRemoving)
        {
            return;
        }
        
        if (!IsInstanceValid(_target))
        {
            Remove();
            return;
        }
        
        UpdatePositionAndScale();
    }

    private bool _isRemoving;

    private void UpdatePositionAndScale()
    {
        float y;
        float distanceFromEdge;
        if (_target.GlobalPosition.Y < ShapeGame.Center.Y)
        {
            // Above
            Rotation = 0;
            y = ShapeGame.PlayableArea.Position.Y + DistanceToEdge;
            distanceFromEdge = Abs(_target.GlobalPosition.Y);
        }
        else
        {
            // Below
            Rotation = Pi;
            y = ShapeGame.PlayableArea.End.Y - DistanceToEdge;
            distanceFromEdge = _target.GlobalPosition.Y - ShapeGame.PlayableArea.End.Y;
        }

        GlobalPosition = new Vector2(_target.GlobalPosition.X, y);

        var distanceRatio = 1 - distanceFromEdge / ShapeGame.DistanceToOutsideBorder;
        var scale = Lerp(MinScale, 1, distanceRatio);
        Scale =  new Vector2(scale, scale);
    }

    public void Remove()
    {
        if (_isRemoving)
        {
            return;
        }

        _isRemoving = true;
        _spawnTween?.Kill();

        var tween = CreateTween().SetParallel();

        tween.TweenProperty(this, OffsetProperty, new Vector2(0, -DistanceToEdge), RemoveDuration)
            .SetEase(Tween.EaseType.In)
            .SetTrans(Tween.TransitionType.Quad);
        tween.TweenProperty(this, ModulateProperty, _color.AsTransparent(), RemoveDuration)
            .SetEase(Tween.EaseType.In)
            .SetTrans(Tween.TransitionType.Quad);
        tween.Finished += QueueFree;
    }

}