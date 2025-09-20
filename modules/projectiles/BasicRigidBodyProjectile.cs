public abstract partial class BasicRigidBodyProjectile<T> : RigidBody2D, IProjectile<T> where T : Node2D
{
    [Export] private AudioStream _wallHitSound = null!;

    public abstract T Node { get; }

    protected ShotContext Context = null!;

    public void Prepare(ShotContext context)
    {
        Context = context;
    }

    public override void _Ready()
    {
        BodyEntered += HandleBodyEntered;
    }

    public override void _Process(double delta)
    {
        if (this.IsOutsidePlayableArea())
        {
            Remove();
        }
    }

    protected virtual void Remove()
    {
        QueueFree();
    }

    protected virtual void OnCollideWithEnemy(Enemy enemy)
    {
        enemy.Damage(Context.CalculateStat<DamageStat>(), this);
        Remove();
    }

    protected virtual void HandleBodyEntered(Node body)
    {
        if (body is not CollisionObject2D collisionObject2D)
        {
            return;
        }

        if (collisionObject2D.GetCollisionLayerValue(CollisionLayers.LevelOutsideBoundary))
        {
            QueueFree();
            return;
        }

        if (collisionObject2D.GetCollisionLayerValue(CollisionLayers.LevelWalls))
        {
            SoundManager.Instance.PlayPositionalSound(this, _wallHitSound).RandomizePitchOffset(0.1f);
        }

        if (collisionObject2D is Enemy enemy)
        {
            OnCollideWithEnemy(enemy);
        }
    }
}