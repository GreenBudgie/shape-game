using System.Collections.Generic;

public partial class RemoveTriggerComponent : Component
{
    public override ComponentType Type => ComponentTypeRegistry.RemoveTrigger;

    public readonly List<SpawnableContext> ContextsToSpawn = [];

    public override void Prepare(SpawnableContext context)
    {
        context.Spawnable.Removed += LaunchTriggers;
    }

    public void LaunchTriggers()
    {
        var position = Spawnable.Node.GlobalPosition;
        
        Vector2 direction;
        if (Spawnable.Node is RigidBody2D rigidBody)
        {
            direction = rigidBody.LinearVelocity.Normalized();
        }
        else
        {
            direction = Vector2.Up;
        }

        var source = Spawnable.Node;
        
        foreach (var contextToSpawn in ContextsToSpawn)
        {
            contextToSpawn.Position = position;
            contextToSpawn.Direction = direction;
            contextToSpawn.Source = source;

            contextToSpawn.ForceSpawn();
        }
    }
    
}