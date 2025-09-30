using System.Linq;

public abstract partial class BasicRigidBodyProjectile<T> : RigidBody2D, IProjectile<T> where T : Node2D
{
    [Export] private AudioStream _wallHitSound = null!;

    public abstract T Node { get; }

    protected ShotContext Context = null!;
    protected int EnemiesToPierce;
    
    private Area2D? _piercingEnemyDetectionArea;

    public void Prepare(ShotContext context)
    {
        Context = context;
        EnemiesToPierce = RoundToInt(context.CalculateStat<PiercingStat>());
    }

    public override void _Ready()
    {
        SetupPiercing();
        BodyEntered += HandleBodyEntered;
        BodyExited += HandleBodyExited;
    }

    public override void _Process(double delta)
    {
        if (this.IsOutsidePlayableArea())
        {
            Remove();
        }
    }

    public virtual void Remove()
    {
        QueueFree();
    }

    private void OnCollideWithEnemy(Enemy enemy)
    {
        enemy.Damage(Context.CalculateStat<DamageStat>(), this);
        if (EnemiesToPierce <= 0)
        {
            Remove();
        }
    }

    private void OnLeaveEnemy(Enemy enemy)
    {
        RemoveCollisionExceptionWith(enemy);
    }

    private void HandleBodyExited(Node body)
    {
        if (body is Enemy enemy)
        {
            OnLeaveEnemy(enemy);
        }
    }

    private void HandleBodyEntered(Node body)
    {
        if (body is not CollisionObject2D collisionObject2D)
        {
            return;
        }

        if (collisionObject2D.HasCollisionLayer(CollisionLayers.LevelOutsideBoundary))
        {
            QueueFree();
            return;
        }

        if (collisionObject2D.HasCollisionLayer(CollisionLayers.LevelWalls))
        {
            SoundManager.Instance.PlayPositionalSound(this, _wallHitSound).RandomizePitchOffset(0.1f);
        }

        if (collisionObject2D is Enemy enemy)
        {
            OnCollideWithEnemy(enemy);
        }
    }

    private void SetupPiercing()
    {
        if (EnemiesToPierce <= 0)
        {
            return;
        }

        if (this.HasCollisionLayer(CollisionLayers.PlayerProjectiles))
        {
            this.DisableCollisionLayer(CollisionLayers.PlayerProjectiles);
            this.EnableCollisionLayer(CollisionLayers.PiercingPlayerProjectiles);
        }
        
        this.DisableCollisionMaskLayer(CollisionLayers.Enemies);
        _piercingEnemyDetectionArea = new Area2D();
        var collisionPolygons = GetChildren().OfType<CollisionPolygon2D>();
        var collisionShapes = GetChildren().OfType<CollisionShape2D>();
        foreach (var collisionPolygon in collisionPolygons)
        {
            _piercingEnemyDetectionArea.AddChild(collisionPolygon.Duplicate());
        }
        foreach (var collisionShape in collisionShapes)
        {
            _piercingEnemyDetectionArea.AddChild(collisionShape.Duplicate());
        }
        
        _piercingEnemyDetectionArea.EnableCollisionMaskLayer(CollisionLayers.Enemies);
        _piercingEnemyDetectionArea.BodyEntered += HandlePiercingEnemyDetectionAreaCollision;
        _piercingEnemyDetectionArea.BodyExited += HandlePiercingEnemyDetectionAreaBodyExited;
        
        AddChild(_piercingEnemyDetectionArea);
    }

    private void StopPiercing()
    {
        if (this.HasCollisionLayer(CollisionLayers.PiercingPlayerProjectiles))
        {
            this.DisableCollisionLayer(CollisionLayers.PiercingPlayerProjectiles);
            this.EnableCollisionLayer(CollisionLayers.PlayerProjectiles);
        }
        this.EnableCollisionMaskLayer(CollisionLayers.Enemies);
    }

    private void HandlePiercingEnemyDetectionAreaCollision(Node body)
    {
        if (EnemiesToPierce <= 0)
        {
            return;
        }

        if (body is not Enemy enemy)
        {
            return;
        }
        
        OnCollideWithEnemy(enemy);
        AddCollisionExceptionWith(enemy);
        EnemiesToPierce--;
        if (EnemiesToPierce == 0)
        {
            StopPiercing();
        }
    }
    
    private void HandlePiercingEnemyDetectionAreaBodyExited(Node body)
    {
        if (body is not Enemy enemy)
        {
            return;
        }

        if (!GetCollisionExceptions().Contains(enemy))
        {
            return;
        }
        
        RemoveCollisionExceptionWith(enemy);
        if (GetCollisionExceptions().Count != 0 || EnemiesToPierce > 0)
        {
            return;
        }
        
        _piercingEnemyDetectionArea?.QueueFree();
        _piercingEnemyDetectionArea = null;
    }
    
}