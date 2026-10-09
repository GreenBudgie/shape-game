public class IgnitionComponentType : ComponentType
{
    public override Component CreateComponent(ISpawnable spawnable)
    {
        return new IgnitionComponent { Spawnable = spawnable };
    }

    public override bool Supports(ISpawnable spawnable)
    {
        return spawnable.Node is RigidBody2D and IAreaAware;
    }

    public override bool ShouldAutoAdd(ISpawnable spawnable)
    {
        return spawnable.Context.CalculateStat<BurningStat>() > 0;
    }
}