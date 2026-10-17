public partial class TetherComponent : Component
{

    private const float TetherDistanceSq = 200f * 200f;
    
    public override ComponentType Type => ComponentTypeRegistry.Tether;

    public SpawnableContext? ContextToConnect { get; set; }

    private bool _isTethered;
    
    public override void Prepare(SpawnableContext context)
    {
        if (ContextToConnect == null)
        {
            QueueFree();
            return;
        }
        
        base.Prepare(context);
    }

    public override void _Process(double delta)
    {
        if (ContextToConnect == null)
        {
            QueueFree();
            return;
        }

        var distanceSq = ContextToConnect.Position.DistanceSquaredTo(Spawnable.Node.GlobalPosition);
        if (distanceSq < TetherDistanceSq)
        {
            return;
        }

        ContextToConnect.Source = Spawnable.Node;
        ContextToConnect.ForceSpawn();
        var tetheredSpawnable = ContextToConnect.Spawnable.Node;
        if (tetheredSpawnable is not RigidBody2D tetheredRigidBody)
        {
            QueueFree();
            return;
        }

        var tether = new BodyTether((RigidBody2D)Spawnable.Node, tetheredRigidBody);
        Spawnable.Node.AddChild(tether);
        
        QueueFree();
    }
}