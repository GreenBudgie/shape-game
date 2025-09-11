public partial class BoltProjectile : RigidBody2D, IProjectile<BoltProjectile>
{

    private static readonly PackedScene BoltProjectileScene = GD.Load<PackedScene>("uid://bnh56fabyfl1o");

    [Export]
    private AudioStream _shotSound = null!;
    
    [Export]
    private AudioStream _wallHitSound = null!;
    
    public BoltProjectile Node => this;

    private ShotContext _context = null!; 
    
    public static BoltProjectile Create()
    {
        return BoltProjectileScene.Instantiate<BoltProjectile>();
    }

    public void Prepare(ShotContext context)
    {
        _context = context;
    }

    public override void _Ready()
    {
        SoundManager.Instance.PlayPositionalSound(this, _shotSound).RandomizePitchOffset(0.1f);
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
        if (body is not CollisionObject2D collisionObject2D)
        {
            return;
        }

        if (collisionObject2D.GetCollisionLayerValue(CollisionLayers.LevelWalls))
        {
            SoundManager.Instance.PlayPositionalSound(this, _wallHitSound).RandomizePitchOffset(0.1f);
        }
        
        if (collisionObject2D is not Enemy enemy)
        {
            return;
        }
        
        enemy.Damage(_context.CalculateStat<DamageStat>());
        QueueFree();
    }

}