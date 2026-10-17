using System.Collections.Generic;

public partial class RemoveTriggerComponent : Component
{
    public override ComponentType Type => ComponentTypeRegistry.RemoveTrigger;

    public SpawnableContext? ContextToTrigger { get; set; }

    public override void Prepare(SpawnableContext context)
    {
        if (ContextToTrigger == null)
        {
            QueueFree();
            return;
        }
        
        context.Spawnable.Removed += LaunchTriggers;
    }

    public void LaunchTriggers()
    {
        if (ContextToTrigger == null)
        {
            return;
        }
        
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
        

        ContextToTrigger.Position = position;
        ContextToTrigger.Direction = direction;
        ContextToTrigger.Source = source;

        ContextToTrigger.ForceSpawn();
    }
    
}