using System;

public partial class SourceCollisionControlComponent : Node, ISpawnableComponent
{
    public void Prepare(SpawnableContext context)
    {
        var node = context.Spawnable.Node;
        if (node is not CollisionObject2D spawnableCollisionObject)
        {
            throw new ArgumentException("SourceCollisionControlComponent is compatible only with CollisionObject2D");
        }

        var originalSource = context.OriginalSource;
        if (originalSource is not CollisionObject2D sourceCollisionObject)
        {
            return;
        }
        
        spawnableCollisionObject.CollisionMask &= ~sourceCollisionObject.CollisionLayer;
    }
}
