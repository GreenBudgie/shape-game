public partial class BubbleProjectile : BasicRigidBodyProjectile
{

    private static readonly PackedScene Scene = GD.Load<PackedScene>("uid://cdbndpmqwdcwv");
    private static readonly AudioStream LaunchSound = GD.Load<AudioStream>("uid://csf0vbsjkv377");
    private static readonly AudioStream PopSound = GD.Load<AudioStream>("uid://dptd5skvk2xkc");

    public static BubbleProjectile Create()
    {
        return Scene.Instantiate<BubbleProjectile>();
    }

    protected override void OnReady()
    {
        Rotation = RandomUtils.RandomAngle();
        AngularVelocity = RandomUtils.Range(-10, 10);
        
        var initialImpulseComponent = this.GetSingleComponent<InitialImpulseComponent>();
        initialImpulseComponent.SpeedDelta = 500;
        initialImpulseComponent.Spread = 40;

        SoundManager.Instance.PlayPositionalSound(this, LaunchSound).RandomizePitchOffset(0.2f);
    }

    public override void OnRemove()
    {
        CollisionLayer = 0;
        CollisionMask = 0;
        LinearDamp = 4;
        
        SoundManager.Instance.PlayPositionalSound(this, PopSound).RandomizePitchOffset(0.2f);

        var sprite = GetNode<Sprite2D>("BubbleSprite");
        DissolveAndRemove(sprite, 0.25f);

        BurstParticleEffect.Create(GlobalPosition)
            .WithAmount(4, 1)
            .Color(ColorScheme.LightBlue)
            .WithTexture(ParticleTextures.Circle)
            .CircleShape(14)
            .WithLifetime(0.5f)
            .WithScale(0.3f, 0.2f)
            .Spawn();
    }
}
