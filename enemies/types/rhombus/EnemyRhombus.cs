public partial class EnemyRhombus : Enemy
{
    [Export] private AudioStream _shotSound = null!;
    [Export] private Marker2D _leftProjectileSpawnPosition = null!;
    [Export] private Marker2D _rightProjectileSpawnPosition = null!;

    private EnemyRhombusPath _path = null!;
    private EnemyPathFollowController _pathFollowController = null!;

    protected override void OnActivate()
    {
        _path = EnemyRhombusPath.Create();
        ShapeGame.Instance.CallDeferred(Node.MethodName.AddChild, _path);
        _pathFollowController = EnemyPathFollowController.AttachEnemyToPath(this, _path);
        
        ResetAttack();
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        ApplyTorque(-_path.Direction * 10000f);
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

        if (IsDestroyed)
        {
            return;
        }

        HandleAttack(delta);
    }

    public override float GetMaxHealth()
    {
        return 5;
    }

    public override float GetCrystalsToDrop()
    {
        return 1;
    }

    private const double MinAttackStartDelaySeconds = 2;
    private const double MaxAttackStartDelaySeconds = 3;
    private const double MinAttackDurationSeconds = 3;
    private const double MaxAttackDurationSeconds = 5;
    private const float AttackTorqueAcceleration = 25000;
    private const float StartAttackTorqueAcceleration = 100000;
    private const double StartDelayPerShotSeconds = 0.6f;
    private const double MinDelayPerShotSeconds = 0.2f;
    private const double DelayPerShotDecreaseSeconds = 0.05f;

    private double _attackStartDelay;
    private double _attackDuration;
    private bool _isAttacking;
    private float _attackTorque;
    private double _timeToFire;
    private double _prevTimeToFire;

    private void ResetAttack()
    {
        _attackStartDelay = GD.RandRange(MinAttackStartDelaySeconds, MaxAttackStartDelaySeconds);
        _isAttacking = false;
        _attackTorque = 0;
    }

    private void HandleAttack(double delta)
    {
        if (_isAttacking)
        {
            ProcessAttack(delta);
            return;
        }

        if (_attackStartDelay <= 0)
        {
            StartAttack();
            return;
        }

        _attackStartDelay -= delta;
    }

    private void StartAttack()
    {
        _attackDuration = GD.RandRange(MinAttackDurationSeconds, MaxAttackDurationSeconds);
        _isAttacking = true;
        _timeToFire = StartDelayPerShotSeconds;
        _prevTimeToFire = _timeToFire;
    }

    private void ProcessAttack(double delta)
    {
        if (_attackDuration <= 0)
        {
            ResetAttack();
            return;
        }

        if (_timeToFire <= 0)
        {
            _timeToFire = Max(_prevTimeToFire - DelayPerShotDecreaseSeconds, MinDelayPerShotSeconds);
            _prevTimeToFire = _timeToFire;

            FireProjectiles();
        }

        ApplyTorque(-_path.Direction * (StartAttackTorqueAcceleration + _attackTorque));

        _attackTorque += (float)(delta * AttackTorqueAcceleration);
        _attackDuration -= delta;
        _timeToFire -= delta;
    }

    private void FireProjectiles()
    {
        FireProjectile(_leftProjectileSpawnPosition.GlobalPosition);
        FireProjectile(_rightProjectileSpawnPosition.GlobalPosition);

        var delayPercent = (_timeToFire - MinDelayPerShotSeconds) / (StartDelayPerShotSeconds - MinDelayPerShotSeconds);
        var sound = SoundManager.Instance.PlayPositionalSound(this, _shotSound);
        sound.PitchScale = Lerp(0.8f, 1.2f, 1f - (float)delayPercent);
    }

    private void FireProjectile(Vector2 globalPosition)
    {
        var projectile = EnemyRhombusProjectile.Create();
        projectile.GlobalPosition = globalPosition;

        const float initialSpeed = 2000f;
        var projectileDirection = GlobalPosition.DirectionTo(globalPosition);
        projectile.ApplyCentralImpulse(projectileDirection * initialSpeed);

        ShapeGame.Instance.AddChild(projectile);
    }
}