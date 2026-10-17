public class RemoveTriggerComponentType : ComponentType
{
    public override RemoveTriggerComponent CreateComponent(ISpawnable spawnable)
    {
        return new RemoveTriggerComponent { Spawnable = spawnable };
    }

    public override bool Supports(ISpawnable spawnable)
    {
        return !spawnable.IsProxy;
    }

    public override bool AllowMultipleCopies(ISpawnable spawnable)
    {
        return true;
    }
}