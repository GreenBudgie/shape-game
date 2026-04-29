public abstract partial class EnemyPath : Path2D
{
    
    public PathFollow2D PathPoint { get; private set; } = null!;
    
    public override void _Ready()
    {
        PathPoint = GetNode<PathFollow2D>("PathPoint");
    }
    
}