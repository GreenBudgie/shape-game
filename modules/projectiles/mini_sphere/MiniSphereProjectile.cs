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

        GetTree().CreateTimer(Context.CalculateStat<LifetimeStat>()).Timeout += Remove;
    }

    protected override void Remove()
    {
        CollisionLayer = 0;
        CollisionMask = 0;

        LinearDamp = 4;
        var sprite = GetNode<Sprite2D>("MiniSphereSprite");
        DissolveEffect.Dissolve(this, sprite, 0.25f);
    }
}
