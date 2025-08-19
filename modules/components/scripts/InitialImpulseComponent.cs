public partial class InitialImpulseComponent : Node, IProjectileComponent
{

    [Export] public RigidBody2D Projectile { get; private set; } = null!;
    [Export] public Vector2 BaseImpulse { get; private set; }

    public Node Node => this;

    public override void _Ready()
    {
        var playerTilt = Player.FindPlayer()?.GetTilt() ?? 0;
        var moveVector = BaseImpulse.Rotated(playerTilt);
        Projectile.ApplyCentralImpulse(moveVector);
    }

}