public class TargetingComponentType : ComponentType
{
    public override Component CreateComponent(ISpawnable spawnable)
    {
        return new TargetingComponent { Spawnable = spawnable };
    }

    public override bool Supports(ISpawnable spawnable)
    {
        return spawnable.Node is RigidBody2D;
    }
}