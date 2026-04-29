public partial class ExplosionParticles : GpuParticles2D
{

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
        var radiusRatio = _explosion.GetEffectRadiusRatio();
        var material = (ParticleProcessMaterial)ProcessMaterial;

        Amount = RoundToInt(Lerp(MinAmount, MaxAmount, radiusRatio));
        
        var maxInitialVelocity = Lerp(MinInitialVelocity, MaxInitialVelocity, radiusRatio);
        material.InitialVelocityMin = MinInitialVelocity - InitialVelocityDelta;
        material.InitialVelocityMax = maxInitialVelocity + InitialVelocityDelta;

        var scale = Lerp(MinScale, MaxScale, radiusRatio);
        material.ScaleMin = scale - ScaleDelta;
        material.ScaleMax = scale + ScaleDelta;

        material.EmissionSphereRadius = Lerp(MinEmissionRadius, MaxEmissionRadius, radiusRatio); 

        Finished += QueueFree;
        
        Restart();
    }
}