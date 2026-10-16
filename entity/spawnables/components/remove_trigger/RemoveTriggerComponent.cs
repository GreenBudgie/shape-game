public partial class RemoveTriggerComponent : Component
{
    public override ComponentType Type => ComponentTypeRegistry.RemoveTrigger;

    public override void Prepare(SpawnableContext context)
    {
        context.Spawnable.Removed += LaunchTriggers;
    }

    public void LaunchTriggers()
    {
        
    }
    
}