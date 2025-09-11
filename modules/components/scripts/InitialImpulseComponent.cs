public partial class InitialImpulseComponent : Node, IProjectileComponent
{

    [Export]
    public Vector2 Direction { get; private set; } = Vector2.Up;

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
    }

}