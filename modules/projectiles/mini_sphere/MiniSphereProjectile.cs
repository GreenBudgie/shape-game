public partial class MiniSphereProjectile : BasicRigidBodyProjectile<MiniSphereProjectile>
{

    private static readonly PackedScene Scene = GD.Load<PackedScene>("uid://dbw8nhcxt7xux");

    [Export]
    private AudioStream _shotSound = null!;
    
    public override MiniSphereProjectile Node => this;

    public static MiniSphereProjectile Create()
    {
        return Scene.Instantiate<MiniSphereProjectile>();
    }
    
    public override void _Ready()
    {
        base._Ready();
        
        SoundManager.Instance.PlayPositionalSound(this, _shotSound).RandomizePitchOffset(0.1f);

        var lifetime = Context.CalculateStat<LifetimeStat>();
        var randomizedLifetime = Max(RandomUtils.DeltaRange(lifetime, 0.1f), 0.1f);
        GetTree().CreateTimer(randomizedLifetime).Timeout += Remove;
    }

    private bool _isRemoving;
    
    protected override void Remove()
    {
        if (_isRemoving)
        {
            return;
        }
        
        _isRemoving = true;
        CollisionLayer = 0;
        CollisionMask = 0;
        LinearDamp = 4;
        
        var sprite = GetNode<Sprite2D>("MiniSphereSprite");
        DissolveEffect.Dissolve(this, sprite, 0.25f);
        
        BurstParticleEffect.Create(GlobalPosition)
            .WithAmount(4, 1)
            .Color(ColorScheme.LightGreen)
            .WithTexture(ParticleTextures.Circle)
            .CircleShape(18)
            .WithLifetime(0.5f)
            .WithScale(0.3f, 0.2f)
            .InheritVelocity(this)
            .VelocitySpreadFactor(0.4f)
            .MinVelocity(100f)
            .VelocityDelta(50f)
            .MaxVelocity(500f)
            .Configure()
            .Spawn();
    }
}
