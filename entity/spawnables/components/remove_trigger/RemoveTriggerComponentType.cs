public class RemoveTriggerComponentType : ComponentType
{
    public override RemoveTriggerComponent CreateComponent(ISpawnable spawnable)
    {
        return new RemoveTriggerComponent { Spawnable = spawnable };
    }
}