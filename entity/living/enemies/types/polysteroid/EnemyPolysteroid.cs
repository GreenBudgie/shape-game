public partial class EnemyPolysteroid : Enemy
{

    private const float Damage = 3;

    protected override bool IsEnvironmental => true;

    private PolysteroidSize _size = null!;

    public override void _Ready()
    {
        base._Ready();

        _size = PolysteroidSizeRegistry.Sizes.GetRandom();
        GetNode<Sprite2D>("%Sprite").Texture = _size.Texture;
        Mass = _size.Mass;
        HealthController.ChangeMaxHealthImmediately(_size.Health);

        var area = _size.AreaScene.Instantiate<CollisionShape2D>();
        AddChild(area);
        Area = area;
        
        var collisionPolygon = _size.CollisionPolygonScene.Instantiate<CollisionPolygon2D>();
        AddChild(collisionPolygon);

        GravityScale = 0;
        TrailParticles.Create(this)
            .WithTexture(ParticleTextures.Circle)
            .WithScale(0.3f, 0.1f)
            .Color(ColorScheme.Red)
            .RectangleShape(area.Shape.GetRect())
            .Spawn();

        BodyEntered += OnBodyEntered;
    }

    public override float GetTimeToActivate()
    {
        return 0;
    }

    private bool _isFirstPhysicsFrame = true;

    public override void _IntegrateForces(PhysicsDirectBodyState2D state)
    {
        if (!_isFirstPhysicsFrame)
        {
            return;
        }

        _isFirstPhysicsFrame = false;
        
        GravityScale = _size.Gravity;
        
        var randomDirection = RandomUtils.Range(0, Tau);
        var randomSpeed = RandomUtils.Range(500, 1000);
        var randomImpulse = Vector2.FromAngle(randomDirection) * randomSpeed;
        ApplyCentralImpulse(randomImpulse);
        
        var randomTorque = RandomUtils.RandomSignedDeltaRange(5000, 3000);
        ApplyTorqueImpulse(randomTorque);

        ConstantTorque = RandomUtils.RandomSignedDeltaRange(7500, 2500);
    }

    public override float GetCrystalsToDrop()
    {
        return 0;
    }

    private void OnBodyEntered(Node body)
    {
        if (body is not CollisionObject2D collisionObject)
        {
            return;
        }

        if (collisionObject.HasCollisionLayer(CollisionLayers.LevelOutsideBoundary) 
            && GlobalPosition.Y > ShapeGame.PlayableArea.End.Y)
        {
            QueueFree();
            return;
        }

        if (collisionObject is Player player)
        {
            player.HealthController.Damage(Damage);
            HealthController.Destroy();
        }
    }
    
}
