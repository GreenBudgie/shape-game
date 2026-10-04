public partial class BoltProjectile : BasicRigidBodyProjectile
{

    private static readonly PackedScene Scene = GD.Load<PackedScene>("uid://bnh56fabyfl1o");

    [Export]
    private AudioStream _shotSound = null!;
    
    public static BoltProjectile Create()
    {
        return Scene.Instantiate<BoltProjectile>();
    }

    protected override void OnReady()
    {
        SoundManager.Instance.PlayPositionalSound(this, _shotSound).RandomizePitchOffset(0.1f);
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        var direction = LinearVelocity.Normalized();
        var angle = direction.Angle() + Pi / 2;
        Rotation = angle;
    }

}