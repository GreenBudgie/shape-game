using System.Collections.Generic;
using System.Linq;

public abstract partial class BasicRigidBodyProjectile<T> : RigidBody2D,
    ISpawnable<T>,
    IPlayerCollisionDetector 
    where T : Node2D
{
    [Export] private AudioStream _wallHitSound = null!;

    public abstract T Node { get; }

    protected SpawnableContext Context = null!;
    protected int ObstaclesToPierce;

    private Area2D? _piercingDetectionArea;
 
    public virtual void Prepare(SpawnableContext context)
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

    public virtual void PlayerShapeEntered(Player player)
    {
        HandleBodyEntered(player);
    }

    public void PlayerShapeExited(Player player)
    {
        HandleBodyExited(player);
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

        if (HandleWallHit(collisionObject2D))
        {
            return;
        }

        HealthController.GetHealthControllerIfExists(collisionObject2D)?.Damage(Context.CalculateStat<DamageStat>());

        if (ObstaclesToPierce <= 0)
        {
            Remove();
        }
    }

    private bool HandleWallHit(CollisionObject2D collisionObject)
    {
        const float minVelocityToMakeSound = 100f;
        var isWall = collisionObject.HasCollisionLayer(CollisionLayers.LevelWalls, CollisionLayers.ProjectileBarrier);
        if (isWall && LinearVelocity.Length() > minVelocityToMakeSound)
        {
            SoundManager.Instance.PlayPositionalSound(this, _wallHitSound).RandomizePitchOffset(0.1f);
        }

        return isWall;
    }

    private List<CollisionLayers> _initialPierceableLayerMasks = [];

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
        
        if (this.HasCollisionLayer(CollisionLayers.EnemyProjectiles))
        {
            this.DisableCollisionLayer(CollisionLayers.EnemyProjectiles);
            this.EnableCollisionLayer(CollisionLayers.PiercingEnemyProjectiles);
        }

        foreach (var pierceableLayer in CollisionObjectUtils.PierceableLayers)
        {
            if (this.HasCollisionMask(pierceableLayer))
            {
                _initialPierceableLayerMasks.Add(pierceableLayer);
                this.DisableCollisionMaskLayer(pierceableLayer);
            }
        }
        
        _piercingDetectionArea = new Area2D();
        _piercingDetectionArea.CollisionLayer = 0;
        _piercingDetectionArea.CollisionMask = 0;
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

        _piercingDetectionArea.EnableCollisionMaskLayers(_initialPierceableLayerMasks);
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
        
        if (this.HasCollisionLayer(CollisionLayers.PiercingEnemyProjectiles))
        {
            this.DisableCollisionLayer(CollisionLayers.PiercingEnemyProjectiles);
            this.EnableCollisionLayer(CollisionLayers.EnemyProjectiles);
        }

        this.EnableCollisionMaskLayers(_initialPierceableLayerMasks);
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

        HealthController.GetHealthControllerIfExists(collisionObject2D)?.Damage(Context.CalculateStat<DamageStat>());

        HandleWallHit(collisionObject2D);
        AddCollisionExceptionWith(collisionObject2D);

        var callable = Callable.From(() => HandlePiercingDetectionAreaBodyExited(collisionObject2D));
        collisionObject2D.Connect(Godot.Node.SignalName.TreeExiting, callable);
        Connect(
            Godot.Node.SignalName.TreeExiting,
            Callable.From(() =>
            {
                if (!IsInstanceValid(collisionObject2D))
                {
                    return;
                }

                collisionObject2D.Disconnect(Godot.Node.SignalName.TreeExiting, callable);
            })
        );

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