public partial class EnemyPathFollowController : Node2D
{

    private const double DefaultTimeToReachPath = 1.5;
    private const float DefaultVelocityCapBeforePathReached = 1500;
    private const float DefaultPathFollowSpeed = 10;

    private Enemy _enemy = null!;

    public EnemyPath Path { get; private set; } = null!;

    public bool IsPathReached { get; private set; }

    public double TimeToReachPath { get; set; } = DefaultTimeToReachPath;

    public float VelocityCapBeforePathReached { get; set; } = DefaultVelocityCapBeforePathReached;
    
    public float PathFollowSpeed { get; set; } = DefaultPathFollowSpeed;

    public override void _Ready()
    {
        GetTree().CreateTimer(TimeToReachPath, processAlways: false).Timeout += SetReachedPath;
    }

    public override void _PhysicsProcess(double delta)
    {
        if (_enemy.IsDestroyed)
        {
            return;
        }

        var direction = GlobalPosition.DirectionTo(Path.PathPoint.GlobalPosition);
        var distance = GlobalPosition.DistanceTo(Path.PathPoint.GlobalPosition);
        if (ShouldCapVelocity())
        {
            return;
        }

        _enemy.ApplyCentralForce(direction * distance * PathFollowSpeed);
    }

    public override void _ExitTree()
    {
        Path.QueueFree();
    }

    private bool ShouldCapVelocity()
    {
        if (IsPathReached)
        {
            return false;
        }

        var velocityLengthSq = _enemy.LinearVelocity.LengthSquared();
        var isVelocityAboveThreshold = velocityLengthSq >= VelocityCapBeforePathReached * VelocityCapBeforePathReached;
        return isVelocityAboveThreshold;
    }

    private void SetReachedPath()
    {
        IsPathReached = true;
    }

    public static EnemyPathFollowController AttachEnemyToPath(Enemy enemy, EnemyPath enemyPath)
    {
        var controller = new EnemyPathFollowController();
        controller._enemy = enemy;
        controller.Path = enemyPath;
        enemy.AddChild(controller);
        return controller;
    }

}