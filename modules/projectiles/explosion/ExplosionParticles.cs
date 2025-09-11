public partial class ExplosionParticles : GpuParticles2D
{

    private static readonly PackedScene Scene = GD.Load<PackedScene>("uid://bhga26pudvab6");

    private Explosion _explosion = null!;

    public static ExplosionParticles Create(Explosion explosion)
    {
        var node = Scene.Instantiate<ExplosionParticles>();
        node._explosion = explosion;
        node.GlobalPosition = explosion.GlobalPosition;
        ShapeGame.Instance.AddChild(node);
        return node;
    }

    public override void _Ready()
    {
        Finished += QueueFree;
    }
}
