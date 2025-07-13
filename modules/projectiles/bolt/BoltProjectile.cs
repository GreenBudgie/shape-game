public partial class BoltProjectile : RigidBody2D, IProjectile
{

    private static readonly PackedScene BoltProjectileScene = GD.Load<PackedScene>("uid://bnh56fabyfl1o");

    public static BoltProjectile Create()
    {
        return BoltProjectileScene.Instantiate<BoltProjectile>();
    }

    public override void _Ready()
    {
        BodyEntered += HandleBodyEntered;
    }

    public override void _Process(double delta)
    {
        if (this.IsOutsidePlayableArea())
        {
            QueueFree();
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        var direction = LinearVelocity.Normalized();
        var angle = direction.Angle() + Pi / 2;
        Rotation = angle;
    }

    private void HandleBodyEntered(Node body)
    {
        if (body is not Enemy enemy)
        {
            return;
        }
        
        enemy.Damage();
        QueueFree();
    }
    
}