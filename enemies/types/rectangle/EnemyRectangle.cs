public partial class EnemyRectangle : Enemy
{

    [Export] private AudioStream _shotSound = null!;

    private const double FireDelay = 1;

    private double _fireTimer = FireDelay;
    private EnemyPathFollowController _pathFollowController = null!;

    public override void _Ready()
    {
        base._Ready();

        var path = EnemyRectanglePath.Create();
        Callable.From(() => ShapeGame.Instance.AddChild(path)).CallDeferred();
        _pathFollowController = EnemyPathFollowController.AttachEnemyToPath(this, path);
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

        if (IsDestroyed)
        {
            return;
        }

        if (!_pathFollowController.IsPathReached)
        {
            return;
        }

        if (_fireTimer <= 0)
        {
            Fire();
            _fireTimer = FireDelay;
        }
        else
        {
            _fireTimer -= delta;
        }
    }

    public override float GetMaxHealth()
    {
        return 12;
    }

    public override float GetCrystalsToDrop()
    {
        return 25;
    }

    private void Fire()
    {
        // TODO
    }
}