public partial class EnemySquarePath : Path2D
{

    public PathFollow2D PathPoint { get; private set; } = null!;

    public override void _Ready()
    {
        PathPoint = GetNode<PathFollow2D>("PathPoint");
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