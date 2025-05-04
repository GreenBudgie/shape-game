public partial class EnemySquarePath : Path2D
{

    private static readonly float MinYOffset = 100;
    private static readonly float MaxYOffset = 400;

    public PathFollow2D PathPoint { get; private set; } = null!;

    public override void _Ready()
    {
        PathPoint = GetNode<PathFollow2D>("PathPoint");

        var pointCount = Curve.PointCount;
        var leftmostPoint = Curve.GetPointPosition(0);
        var rightmostPoint = Curve.GetPointPosition(pointCount - 1);
        var pathWidth = rightmostPoint.X - leftmostPoint.X;
        var xOffset = ShapeGame.PlayableArea.GetCenter().X - pathWidth / 2f;
        var yOffset = (float)GD.RandRange(MinYOffset, MaxYOffset);
        GlobalPosition = new Vector2(xOffset, yOffset);
    }

    private int _direction = 1; // 1 = forward, -1 = backward

    public override void _Process(double delta)
    {
        const float speed = 0.1f;
        PathPoint.ProgressRatio = Clamp(PathPoint.ProgressRatio + (float)(speed * _direction * delta), 0f, 1f);

        if (PathPoint.ProgressRatio is >= 1.0f or <= 0.0f)
        {
            _direction *= -1;
        }
    }

}