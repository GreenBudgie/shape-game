public partial class TriggerModule : ConnectorModule
{

    public override ModuleType Type => ModuleTypeRegistry.Trigger;

    public override ISpawnableContextLifecycleDelegator CreateConnector(SpawnableContext context)
    {
        throw new System.NotImplementedException();
    }
}
