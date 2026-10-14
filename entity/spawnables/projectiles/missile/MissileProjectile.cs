public partial class MissileProjectile : BasicRigidBodyProjectile
{

    private static readonly PackedScene Scene = GD.Load<PackedScene>("uid://h41qap5l8leg");
    private static readonly AudioStream ShotSound = GD.Load<AudioStream>("uid://l16gijqwh2r");
    
    public static MissileProjectile Create()
    {
        return Scene.Instantiate<MissileProjectile>();
    }

    public override void Setup(SpawnableContext context)
    {
        base.Setup(context);
        this.AddComponentIfPossible(ComponentTypeRegistry.Targeting);
        this.AddComponentIfPossible(ComponentTypeRegistry.RemoveExplosion);
    }

    protected override void OnReady()
    {
        SoundManager.Instance.PlayPositionalSound(this, ShotSound).RandomizePitchOffset(0.1f);
    }
    
    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        var direction = LinearVelocity.Normalized();
        var angle = direction.Angle() + Pi / 2;
        Rotation = angle;
        
        ApplyCentralForce(LinearVelocity.Normalized() * 3000f);
    }

}
