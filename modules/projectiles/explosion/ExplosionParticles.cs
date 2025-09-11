public partial class ExplosionParticles : GpuParticles2D
{
    private const float MaxRadius = 2000;

    private const float MinAmount = 15;
    private const float MaxAmount = 70;
    private const float MinInitialVelocity = 1000;
    private const float MaxInitialVelocity = 3500;
    private const float InitialVelocityDelta = 500;
    private const float MinScale = 0.6f;
    private const float MaxScale = 1.4f;
    private const float ScaleDelta = 0.25f;
    private const float MinEmissionRadius = 20;
    private const float MaxEmissionRadius = 500;

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
        var weight = Clamp(_explosion.GetRadius(), 0, MaxRadius) / MaxRadius;
        var material = (ParticleProcessMaterial)ProcessMaterial;

        Amount = RoundToInt(Lerp(MinAmount, MaxAmount, weight));
        
        var maxInitialVelocity = Lerp(MinInitialVelocity, MaxInitialVelocity, weight);
        material.InitialVelocityMin = MinInitialVelocity - InitialVelocityDelta;
        material.InitialVelocityMax = maxInitialVelocity + InitialVelocityDelta;

        var scale = Lerp(MinScale, MaxScale, weight);
        material.ScaleMin = scale - ScaleDelta;
        material.ScaleMax = scale + ScaleDelta;

        material.EmissionSphereRadius = Lerp(MinEmissionRadius, MaxEmissionRadius, weight); 

        Finished += QueueFree;
        
        Restart();
    }
}