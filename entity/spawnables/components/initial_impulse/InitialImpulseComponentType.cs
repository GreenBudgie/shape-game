public class InitialImpulseComponentType : ComponentType
{
    public override Component CreateComponent(ISpawnable spawnable)
    {
        return new InitialImpulseComponent { Spawnable = spawnable };
    }

    public override bool Supports(ISpawnable spawnable)
    {
        return spawnable.Node is RigidBody2D;
    }

    public override bool ShouldAutoAdd(ISpawnable spawnable)
    {
        return spawnable.Context.CalculateStat<SpeedStat>() > 0;
    }
}