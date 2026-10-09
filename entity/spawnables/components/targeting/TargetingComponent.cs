using System;

public partial class TargetingComponent : Component
{
    public override ComponentType Type => ComponentTypeRegistry.Targeting;

    public override void Prepare(SpawnableContext context)
    {
        if (Spawnable.Node is not RigidBody2D rigidBody)
        {
            throw new Exception("TargetingComponent is only applicable to RigidBodies");
        }
        
        TrailParticles.Create(rigidBody)
            .WithTexture(ParticleTextures.Square)
            .WithScale(0.4f, 0.1f)
            .Color(ColorScheme.Orange)
            .Spawn();
        
        // TODO
    }
    
}