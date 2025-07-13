public partial class InitialImpulseComponent : Node, IModuleComponent
{
    
    [Export] public RigidBody2D Projectile { get; private set; } = null!;
    [Export] public Vector2 BaseImpulse { get; private set; }

    public override void _Ready()
    {
        var playerRotation = Player.FindPlayer()?.Rotation ?? 0;
        var moveVector = BaseImpulse.Rotated(playerRotation);
        Projectile.ApplyCentralImpulse(moveVector);
    }

}