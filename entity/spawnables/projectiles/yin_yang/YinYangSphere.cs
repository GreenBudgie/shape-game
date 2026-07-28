public partial class YinYangSphere : BasicRigidBodyProjectile<YinYangSphere>
{

    private const float Radius = 32f;

    private static readonly Texture2D YinTexture = GD.Load<Texture2D>("uid://dndyopr6137m3"); // Blue
    private static readonly Texture2D YangTexture = GD.Load<Texture2D>("uid://bye8t1ytl68gx"); // Red
    
    public override YinYangSphere Node => this;

    public YinYangSphere? OtherSphere { get; set; }

    private YinYangType _type;
    private Node2D? _followTarget;

    private static readonly PackedScene Scene = GD.Load<PackedScene>("uid://3bwof5vbs2i3");

    public static YinYangSphere Create(YinYangType type, Node2D followTarget)
    {
        var node = Scene.Instantiate<YinYangSphere>();
        node._type = type;
        node._followTarget = followTarget;
        return node;
    }
    
    private Tween? _pathAnimationTween;

    public override void _Ready()
    {
        base._Ready();
        
        var texture = _type == YinYangType.Yin ? YinTexture : YangTexture;
        GetNode<Sprite2D>("Sprite2D").Texture = texture;
        
        Modulate = Colors.Transparent;
        
        _pathAnimationTween = CreateTween().SetTrans(Tween.TransitionType.Quad);
        _pathAnimationTween.FadeIn(this, 0.1f);
        
        TrailParticles.Create(this)
            .WithTexture(ParticleTextures.Circle)
            .WithScale(0.4f, 0.1f)
            .Color(_type == YinYangType.Yin ? ColorScheme.LightBlue : ColorScheme.Red)
            .Spawn();

        if (OtherSphere != null)
        {
            OtherSphere.TreeExiting += Detach;
        }
        
        if (_followTarget != null)
        {
            _followTarget.TreeExiting += Detach;
        }
    }

    protected override bool ShouldRemoveWhenOutsidePlayableArea()
    {
        var isDetached = _followTarget == null;
        return isDetached;
    }

    public override void Remove()
    {
        BurstParticleEffect.Create(GlobalPosition)
            .WithAmount(4, 1)
            .Color(_type == YinYangType.Yin ? ColorScheme.LightBlue : ColorScheme.Red)
            .WithTexture(ParticleTextures.Circle)
            .CircleShape(Radius)
            .WithLifetime(0.5f)
            .WithScale(0.5f, 0.2f)
            .Spawn();
        
        base.Remove();
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        
        if (_followTarget == null)
        {
            return;
        }
        
        var direction = GlobalPosition.DirectionTo(_followTarget.GlobalPosition);
        var distance = GlobalPosition.DistanceTo(_followTarget.GlobalPosition);

        const float followSpeed = 100f;
        ApplyCentralForce(direction * distance * followSpeed);
    }

    protected override void OnWallHit(CollisionObject2D collisionObject)
    {
        base.OnWallHit(collisionObject);

        if (_followTarget != null)
        {
            Remove();
        }
    }

    public void Detach()
    {
        if (_followTarget == null)
        {
            return;
        }
        
        OtherSphere = null;
        _followTarget = null;

        const float minSpeed = 1500;
        const float maxSpeed = 2200;
        var speed = (float)GD.RandRange(minSpeed, maxSpeed);
        LinearVelocity = Vector2.FromAngle(GD.Randf() * Pi * 2) * speed;

        LinearDamp = 0;
    }
}
