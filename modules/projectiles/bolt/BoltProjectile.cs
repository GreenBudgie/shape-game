public partial class BoltProjectile : BasicRigidBodyProjectile<BoltProjectile>
{

    private static readonly PackedScene Scene = GD.Load<PackedScene>("uid://bnh56fabyfl1o");

    [Export]
    private AudioStream _shotSound = null!;
    
    public override BoltProjectile Node => this;

    public static BoltProjectile Create()
    {
        return Scene.Instantiate<BoltProjectile>();
    }

    public override void _Ready()
    {
        base._Ready();
        
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