public partial class ParticleCache : Node
{

    public override void _Ready()
    {
        var particleMaterials = ResourceSearcher.FindResourcesInDirectory<ParticleProcessMaterial>(
            "res://cache/particles"
        );
        foreach (var particleMaterial in particleMaterials)
        {
            var particlesNode = new GpuParticles2D();
            particlesNode.SetProcessMaterial(particleMaterial);
            particlesNode.SetOneShot(true);
            particlesNode.SetModulate(new Color(1, 1, 1, 0));
            particlesNode.SetEmitting(true);
            AddChild(particlesNode);
            
            particlesNode.QueueFree();
        }
    }

}