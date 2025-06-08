public partial class EnemyRhombusPath : Path2D
{

    private static readonly PackedScene Scene = GD.Load<PackedScene>("uid://dofw2c36kh88s");
    
    private const float MinYOffset = 300;
    private const float MaxYOffset = 600;

    private const float PathWidth = 2800;

    public PathFollow2D PathPoint { get; private set; } = null!;
    
    private int _direction = 1; // 1 = clockwise, -1 = counterclockwise

    public static EnemyRhombusPath CreatePath(int direction)
    {
        var path = Scene.Instantiate<EnemyRhombusPath>();
        path._direction = direction;
        return path;
    }
    
    public override void _Ready()
    {
        PathPoint = GetNode<PathFollow2D>("PathPoint");

        var xOffset = ShapeGame.PlayableArea.GetCenter().X - PathWidth / 2f;
        var yOffset = (float)GD.RandRange(MinYOffset, MaxYOffset);
        GlobalPosition = new Vector2(xOffset, yOffset);
    }

    public override void _Process(double delta)
    {
        const float speed = 0.1f;
        PathPoint.ProgressRatio += (float)(speed * _direction * delta);
    }

}