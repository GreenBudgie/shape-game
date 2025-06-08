public partial class EnemyRhombusPath : Path2D
{

    private const float MinYOffset = 300;
    private const float MaxYOffset = 600;

    private const float PathWidth = 2800;

    public PathFollow2D PathPoint { get; private set; } = null!;
    
    private int _direction = 1; // 1 = clockwise, -1 = counterclockwise
    
    public override void _Ready()
    {
        PathPoint = GetNode<PathFollow2D>("PathPoint");

        var xOffset = ShapeGame.PlayableArea.GetCenter().X - PathWidth / 2f;
        var yOffset = (float)GD.RandRange(MinYOffset, MaxYOffset);
        GlobalPosition = new Vector2(xOffset, yOffset);

        _direction = GD.Randf() > 0.5f ? -1 : 1;
    }

    public override void _Process(double delta)
    {
        const float speed = 0.1f;
        PathPoint.ProgressRatio = Clamp(PathPoint.ProgressRatio + (float)(speed * _direction * delta), 0f, 1f);
    }

}