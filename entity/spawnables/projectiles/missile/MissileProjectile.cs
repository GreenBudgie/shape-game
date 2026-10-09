public partial class MissileProjectile : BasicRigidBodyProjectile
{

    private static readonly PackedScene Scene = GD.Load<PackedScene>("uid://dbw8nhcxt7xux");

    private AudioStream _shotSound = null!;
    
    public static MissileProjectile Create()
    {
        return Scene.Instantiate<MissileProjectile>();
    }
    
    protected override void OnReady()
    {
        SoundManager.Instance.PlayPositionalSound(this, _shotSound).RandomizePitchOffset(0.1f);
    }

    private bool _isRemoving;
    
    public override void Remove()
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
        DissolveEffect.DissolveAndRemove(this, sprite, 0.25f);
        
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
        
        LaunchTriggers();
    }
}
