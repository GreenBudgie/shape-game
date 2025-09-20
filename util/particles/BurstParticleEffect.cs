public partial class BurstParticleEffect : ParticleBuilder<BurstParticleEffect>
{

    private static readonly PackedScene Scene = GD.Load<PackedScene>("uid://cdy1djaep1tof");

    private ParticleProcessMaterial _material = null!;
    
    public static BurstParticleEffect Create(Vector2 globalPosition)
    {
        var node = Scene.Instantiate<BurstParticleEffect>();
        node.GlobalPosition = globalPosition;
        return node;
    }
    
}