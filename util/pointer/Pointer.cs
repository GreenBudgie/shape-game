public partial class Pointer : Sprite2D
{
    
    private const float DistanceToEdge = 40;

    private static readonly PackedScene Scene = GD.Load<PackedScene>("uid://kmwq84squv6w");

    private CollisionObject2D _target = null!;
    private Color _color;

    public static Pointer Create(CollisionObject2D target, Color color)
    {
        var node = Scene.Instantiate<Pointer>();
        node._target = target;
        node._color = color;
        node.Scale = new Vector2(0.5f, 0.5f);
        node.Modulate = color;
        node.UpdatePosition();
        node.Offset = new Vector2(0, -DistanceToEdge);
        return node;
    }

    private Tween? _spawnTween;

    public override void _Ready()
    {
        const float spawnDuration = 0.25f;

        _spawnTween = CreateTween()
            .SetParallel()
            .SetEase(Tween.EaseType.Out)
            .SetTrans(Tween.TransitionType.Quad);

        _spawnTween.TweenProperty(this, OffsetProperty, Vector2.Zero, spawnDuration);
        _spawnTween.TweenProperty(this, ScaleProperty, Vector2.One, spawnDuration);
        _spawnTween.TweenProperty(this, ModulateProperty, _color, spawnDuration);
    }

    public override void _Process(double delta)
    {
        UpdatePosition();
    }

    private bool _isRemoving;

    private void UpdatePosition()
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

        float y;
        if (_target.GlobalPosition.Y < ShapeGame.Center.Y)
        {
            Rotation = 0;
            y = ShapeGame.PlayableArea.Position.Y + DistanceToEdge;
        }
        else
        {
            Rotation = Pi;
            y = ShapeGame.PlayableArea.End.Y - DistanceToEdge;
        }

        GlobalPosition = new Vector2(_target.GlobalPosition.X, y);
    }

    public void Remove()
    {
        if (_isRemoving)
        {
            return;
        }

        _isRemoving = true;
        _spawnTween?.Kill();

        const float removeDuration = 0.25f;

        var tween = CreateTween()
            .SetParallel()
            .SetEase(Tween.EaseType.In)
            .SetTrans(Tween.TransitionType.Quad);

        tween.TweenProperty(this, OffsetProperty, new Vector2(0, -DistanceToEdge), removeDuration);
        tween.TweenProperty(this, ScaleProperty, new Vector2(0.5f, 0.5f), removeDuration);
        tween.TweenProperty(this, ModulateProperty, _color.AsTransparent(), removeDuration);
        tween.Finished += QueueFree;
    }

}