public partial class EnemySquarePath : EnemyPath
{

    private const float MinYOffset = 300;
    private const float MaxYOffset = 600;

    public override void _Ready()
    {
        base._Ready();
        
        PathPoint.ProgressRatio = (float)GD.RandRange(0f, 1f);

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