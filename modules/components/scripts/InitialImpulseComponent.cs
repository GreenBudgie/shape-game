public partial class InitialImpulseComponent : Node, IProjectileComponent
{

    [Export]
    public Vector2 Direction { get; private set; } = Vector2.Up;
    
    [ExportCategory("Torque")]
    [Export] public float Torque { get; private set; }
    [Export] public float TorqueDelta { get; private set; }

    private float _speed;

    public Node Node => this;

    public void Prepare(ShotContext context)
    {
        _speed = context.CalculateStat<SpeedStat>();
        Direction = Direction.Normalized();
    }

    public void Apply(ShotContext context)
    {
        var projectile = context.Projectile.Node;
        if (projectile is not RigidBody2D rigidBodyProjectile)
        {
            return;
        }
        
        var playerTilt = Player.FindPlayer()?.GetTilt() ?? 0;
        var vector = Direction * _speed;
        var moveVector = vector.Rotated(playerTilt);
        rigidBodyProjectile.ApplyCentralImpulse(moveVector);
        
        if (Torque == 0 && TorqueDelta == 0)
        {
            return;
        }
        var actualTorque = (float)GD.RandRange(Torque - TorqueDelta, Torque + TorqueDelta);
        rigidBodyProjectile.ApplyTorqueImpulse(actualTorque);
    }

}