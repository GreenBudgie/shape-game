using System.Collections.Generic;
using System.Linq;

public partial class EnemyRectangle : Enemy
{
    public const float ProjectileChargeTime = 0.3f;

    [Export] private AudioStream _chargeSound = null!;
    [Export] private AudioStream _launchSound = null!;

    private const float FireDelay = 2.5f;
    private const float FireDelayDelta = 1f;
    private const float PitchPerProjectileSpawn = 0.1f;

    private double _fireTimer = RandomFireDelay();
    private EnemyPathFollowController _pathFollowController = null!;
    private List<ProjectileSpawn> _projectileSpawns = null!;
    private State _state = State.Idle;

    public override void _Ready()
    {
        base._Ready();

        RotationDegrees = (float)GD.RandRange(-180f, 180f);

        var path = EnemyRectanglePath.Create();
        Callable.From(() => ShapeGame.Instance.AddChild(path)).CallDeferred();
        _pathFollowController = EnemyPathFollowController.AttachEnemyToPath(this, path);

        var projectileSpawnPositionsNode = GetNode("ProjectileSpawnPositions");
        _projectileSpawns = projectileSpawnPositionsNode.GetChildren()
            .OfType<Marker2D>()
            .Select(node => new ProjectileSpawn(node.GetPosition()))
            .ToList();
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

        if (_state == State.Idle)
        {
            ProcessIdle(delta);
        }
        else if (_state == State.ChargingAttack)
        {
            ProcessChargeAttack(delta);
        } else if (_state == State.Attacking)
        {
            ProcessAttack(delta);
        }
    }

    public override float GetMaxHealth()
    {
        return 12;
    }

    public override float GetCrystalsToDrop()
    {
        return 1;
    }

    private void SetIdle()
    {
        _fireTimer = RandomFireDelay();
        _state = State.Idle;
    }

    private void ProcessIdle(double delta)
    {
        _fireTimer -= delta;

        if (_fireTimer <= 0)
        {
            ChargeAttack();
        }
    }

    private void ChargeAttack()
    {
        _nextProjectileChargeTime = 0;
        _state = State.ChargingAttack;
    }

    private const float ProjectileChargeDelay = 0.15f;

    private double _nextProjectileChargeTime;

    private void ProcessChargeAttack(double delta)
    {
        _nextProjectileChargeTime -= delta;

        if (_nextProjectileChargeTime <= 0)
        {
            ChargeProjectileOrAttack();
        }
    }

    private void ChargeProjectileOrAttack()
    {
        var availableProjectileSpawns = _projectileSpawns.Where(spawn => spawn.ProjectilePreview == null).ToList();

        if (availableProjectileSpawns.Count == 0)
        {
            Attack();
            return;
        }

        var randomSpawn = availableProjectileSpawns.GetRandom();
        var projectile = EnemyRectangleProjectilePreview.Create(this);
        projectile.Position = randomSpawn.Position;
        AddChild(projectile);
        randomSpawn.ProjectilePreview = projectile;

        var totalSpawns = _projectileSpawns.Count;
        var pitch = 1 + (totalSpawns - availableProjectileSpawns.Count) * PitchPerProjectileSpawn;
        var sound = SoundManager.Instance.PlayPositionalSound(projectile, _chargeSound);
        sound.PitchScale = RandomUtils.DeltaRange(pitch, 0.05f);

        _nextProjectileChargeTime = ProjectileChargeDelay;
    }
    
    private const float ProjectileLaunchDelay = 0.2f;
    private const float ProjectileLaunchDelayDelta = 0.05f;

    private double _nextProjectileLaunchTime;

    private void Attack()
    {
        _nextProjectileLaunchTime = ProjectileChargeTime;
        _state = State.Attacking;
    }

    private void ProcessAttack(double delta)
    {
        _nextProjectileLaunchTime -= delta;

        if (_nextProjectileLaunchTime <= 0)
        {
            LaunchProjectile();
        }
    }

    private void LaunchProjectile()
    {
        var availableProjectileSpawns = _projectileSpawns.Where(spawn => spawn.ProjectilePreview != null).ToList();

        if (availableProjectileSpawns.Count == 0)
        {
            SetIdle();
            return;
        }

        var randomSpawn = availableProjectileSpawns.GetRandom();
        var projectile = randomSpawn.ProjectilePreview!;
        projectile.Launch();
        randomSpawn.ProjectilePreview = null;

        const float midImpulseStrength = 300f;
        const float impulseStrengthDelta = 100f;
        var impulseStrength = RandomUtils.DeltaRange(midImpulseStrength, impulseStrengthDelta);
        var impulse = Vector2.FromAngle(Rotation - Pi / 2) * impulseStrength;
        ApplyImpulse(impulse, randomSpawn.Position);

        var pitch = 1 + availableProjectileSpawns.Count * PitchPerProjectileSpawn;
        var sound = SoundManager.Instance.PlayPositionalSound(projectile, _launchSound);
        sound.PitchScale = RandomUtils.DeltaRange(pitch, 0.05f);
        
        _nextProjectileLaunchTime = RandomUtils.DeltaRange(ProjectileLaunchDelay, ProjectileLaunchDelayDelta);
    }

    private static float RandomFireDelay() => RandomUtils.DeltaRange(FireDelay, FireDelayDelta);

    private class ProjectileSpawn(Vector2 position)
    {
        public Vector2 Position { get; } = position;
        public EnemyRectangleProjectilePreview? ProjectilePreview { get; set; }
    }

    private enum State
    {
        Idle,
        ChargingAttack,
        Attacking
    }
}