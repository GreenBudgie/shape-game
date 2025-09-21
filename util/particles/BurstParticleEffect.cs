public partial class BurstParticleEffect : ParticleBuilder<BurstParticleEffect>
{

    private static readonly PackedScene Scene = GD.Load<PackedScene>("uid://cdy1djaep1tof");

    public static BurstParticleEffect Create(Vector2 globalPosition)
    {
        var node = Scene.Instantiate<BurstParticleEffect>();
        node.GlobalPosition = globalPosition;
        return node;
    }
    
    public override void _Ready()
    {
        Finished += QueueFree;
        Restart();
    }

    public void Spawn()
    {
        ShapeGame.Instance.AddChild(this);
    }
    
}