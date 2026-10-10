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

}
