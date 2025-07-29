public partial class EnemyRhombusProjectile : RigidBody2D, IPlayerCollisionDetector
{
    private const double MaxLifetimeSeconds = 6;
    
    [Export] private AudioStream _hitWallSound = null!;

    private double _lifetimeSeconds = MaxLifetimeSeconds; 
    
    public override void _Ready()
    {
        BodyEntered += HandleBodyEntered;
    }

    public override void _Process(double delta)
    {
        if (this.IsOutsidePlayableArea() || _lifetimeSeconds <= 0)
        {
            QueueFree();
        }

        _lifetimeSeconds -= delta;
    }
    
    public override void _PhysicsProcess(double delta)
    {
        var direction = LinearVelocity.Normalized();
        var angle = direction.Angle() + Pi;
        Rotation = angle;
    }
    
    public void CollideWithPlayer(Player player)
    {
        QueueFree();
    }
    
    private void HandleBodyEntered(Node body)
    {
        if (!body.IsInGroup("level_walls"))
        {
            return;
        }
        
        var speed = LinearVelocity.Length();
        if (speed <= 100)
        {
            return;
        }

        var sound = SoundManager.Instance.PlayPositionalSound(this, _hitWallSound);
        sound.RandomizePitch(0.8f, 1.2f);
    }
    
}
