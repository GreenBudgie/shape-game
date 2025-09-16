using System.Collections.Generic;

public partial class EnemyRectangle : Enemy
{
    [Export] private AudioStream _shotSound = null!;

    private const double FireDelay = 1;

    private double _fireTimer = FireDelay;
    private EnemyPathFollowController _pathFollowController = null!;
    private List<Vector2> _projectileSpawnPositions = null!;

    public override void _Ready()
    {
        base._Ready();

        RotationDegrees = (float)GD.RandRange(-180f, 180f);

        var path = EnemyRectanglePath.Create();
        Callable.From(() => ShapeGame.Instance.AddChild(path)).CallDeferred();
        _pathFollowController = EnemyPathFollowController.AttachEnemyToPath(this, path);
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        // Resist the tilt, trying to return to 0 degrees of rotation
        const float noTorqueThresholdDegrees = 2f;
        var rotationDegrees = RotationDegrees;
        if (Abs(rotationDegrees) < noTorqueThresholdDegrees)
        {
            return;
        }
        
        const float torqueByDegree = 1000f;
        var direction = rotationDegrees < 0 ? 1 : -1;
        var torque = Abs(rotationDegrees) * torqueByDegree;
        ApplyTorque(torque * direction);
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