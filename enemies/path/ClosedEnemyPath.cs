public abstract partial class ClosedEnemyPath : EnemyPath
{
    
    // 1 = clockwise, -1 = counterclockwise
    public int Direction { get; protected set; } = RandomUtils.RandomSign();
    
    protected abstract float MinYOffset { get; }
    protected abstract float MaxYOffset { get; }
    protected abstract float PathWidth { get; }
    protected abstract float Speed { get; }
    
    
    public override void _Ready()
    {
        base._Ready();

        PathPoint.ProgressRatio = 0;

        var xOffset = ShapeGame.PlayableArea.GetCenter().X - PathWidth / 2f;
        var yOffset = (float)GD.RandRange(MinYOffset, MaxYOffset);
        GlobalPosition = new Vector2(xOffset, yOffset);
    }

    public override void _Process(double delta)
    {
        PathPoint.ProgressRatio += (float)(Speed * Direction * delta);
    }
}