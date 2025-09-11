public partial class MineProjectile : RigidBody2D, IProjectile<MineProjectile>
{

    private static readonly PackedScene Scene = GD.Load<PackedScene>("uid://coo3tnuma02fs");

    [Export]
    private AudioStream _shotSound = null!;
    
    [Export]
    private AudioStream _wallHitSound = null!;
    
    public MineProjectile Node => this;
    
    public static MineProjectile Create()
    {
        return Scene.Instantiate<MineProjectile>();
    }

    private ShotContext _context = null!;

    public void Prepare(ShotContext context)
    {
        _context = context;
    }

    public override void _Ready()
    {
        SoundManager.Instance.PlayPositionalSound(this, _shotSound).RandomizePitchOffset(0.1f);
        GetTree().CreateTimer(0.5f).Timeout += Fuse;
        BodyEntered += HandleBodyEntered;
    }

    public override void _Process(double delta)
    {
        if (this.IsOutsidePlayableArea())
        {
            QueueFree();
        }
    }

    private void Fuse()
    {
        var radius = (float)GD.RandRange(200, 1500);
        var explosion = Explosion.Create(this)
            .SetRadius(radius)
            //.SetRadius(_context.CalculateStat<ExplosionRadiusStat>())
            .SetFuseTimeSeconds(1);

        explosion.Detonated += QueueFree;
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
    }

}