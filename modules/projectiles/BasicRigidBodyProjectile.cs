using System.Collections.Generic;
using System.Linq;

public abstract partial class BasicRigidBodyProjectile<T> : RigidBody2D, IProjectile<T> where T : Node2D
{
    [Export] private AudioStream _wallHitSound = null!;

    public abstract T Node { get; }

    protected ShotContext Context = null!;
    protected int ObstaclesToPierce;

    private Area2D? _piercingDetectionArea;

    public void Prepare(ShotContext context)
    {
        Context = context;
        ObstaclesToPierce = RoundToInt(context.CalculateStat<PiercingStat>());
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
    }

    private void OnLeavePierceableObject(CollisionObject2D collisionObject)
    {
        RemoveCollisionExceptionWith(collisionObject);
    }

    private void HandleBodyExited(Node body)
    {
        if (body is not CollisionObject2D collisionObject2D)
        {
            return;
        }

        if (!collisionObject2D.HasCollisionLayer(CollisionObjectUtils.PierceableLayers))
        {
            return;
        }
        
        OnLeavePierceableObject(collisionObject2D);
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

        HandleWallHit(collisionObject2D);

        if (collisionObject2D is Enemy enemy)
        {
            OnCollideWithEnemy(enemy);
        }
    }

    private void HandleWallHit(CollisionObject2D collisionObject)
    {
        const float minVelocityToMakeSound = 100f;
        var isWall = collisionObject.HasCollisionLayer(CollisionLayers.LevelWalls, CollisionLayers.ProjectileBarrier);
        if (isWall && LinearVelocity.Length() > minVelocityToMakeSound)
        {
            SoundManager.Instance.PlayPositionalSound(this, _wallHitSound).RandomizePitchOffset(0.1f);
        }
    }

    private void SetupPiercing()
    {
        if (ObstaclesToPierce <= 0)
        {
            return;
        }

        if (this.HasCollisionLayer(CollisionLayers.PlayerProjectiles))
        {
            this.DisableCollisionLayer(CollisionLayers.PlayerProjectiles);
            this.EnableCollisionLayer(CollisionLayers.PiercingPlayerProjectiles);
        }

        this.DisableCollisionMaskLayers(CollisionObjectUtils.PierceableLayers);
        _piercingDetectionArea = new Area2D();
        var collisionPolygons = GetChildren().OfType<CollisionPolygon2D>();
        var collisionShapes = GetChildren().OfType<CollisionShape2D>();
        foreach (var collisionPolygon in collisionPolygons)
        {
            _piercingDetectionArea.AddChild(collisionPolygon.Duplicate());
        }

        foreach (var collisionShape in collisionShapes)
        {
            _piercingDetectionArea.AddChild(collisionShape.Duplicate());
        }

        _piercingDetectionArea.EnableCollisionMaskLayers(CollisionObjectUtils.PierceableLayers);
        _piercingDetectionArea.BodyEntered += HandlePiercingDetectionAreaCollision;
        _piercingDetectionArea.BodyExited += HandlePiercingDetectionAreaBodyExited;

        AddChild(_piercingDetectionArea);
    }

    private void StopPiercing()
    {
        if (this.HasCollisionLayer(CollisionLayers.PiercingPlayerProjectiles))
        {
            this.DisableCollisionLayer(CollisionLayers.PiercingPlayerProjectiles);
            this.EnableCollisionLayer(CollisionLayers.PlayerProjectiles);
        }

        this.EnableCollisionMaskLayers(CollisionObjectUtils.PierceableLayers);
    }

    private void HandlePiercingDetectionAreaCollision(Node body)
    {
        if (ObstaclesToPierce <= 0)
        {
            return;
        }

        if (body is not CollisionObject2D collisionObject2D)
        {
            return;
        }

        if (!collisionObject2D.HasCollisionLayer(CollisionObjectUtils.PierceableLayers))
        {
            return;
        }

        if (collisionObject2D is Enemy enemy)
        {
            OnCollideWithEnemy(enemy);
        }
        
        HandleWallHit(collisionObject2D);
        AddCollisionExceptionWith(collisionObject2D);
        ObstaclesToPierce--;
        if (ObstaclesToPierce == 0)
        {
            StopPiercing();
        }
    }

    private void HandlePiercingDetectionAreaBodyExited(Node body)
    {
        if (body is not CollisionObject2D collisionObject2D)
        {
            return;
        }

        if (!collisionObject2D.HasCollisionLayer(CollisionObjectUtils.PierceableLayers))
        {
            return;
        }

        if (!GetCollisionExceptions().Contains(collisionObject2D))
        {
            return;
        }

        RemoveCollisionExceptionWith(collisionObject2D);
        if (GetCollisionExceptions().Count != 0 || ObstaclesToPierce > 0)
        {
            return;
        }

        _piercingDetectionArea?.QueueFree();
        _piercingDetectionArea = null;
    }
}