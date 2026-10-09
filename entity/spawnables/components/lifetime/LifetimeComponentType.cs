public class LifetimeComponentType : ComponentType
{
    public override Component CreateComponent(ISpawnable spawnable)
    {
        return new LifetimeComponent { Spawnable = spawnable };
    }

    public override bool Supports(ISpawnable spawnable)
    {
        return spawnable.Node is RigidBody2D;
    }

    public override bool ShouldAutoAdd(ISpawnable spawnable)
    {
        return spawnable.Context.CalculateStat<LifetimeStat>() > 0;
    }
}