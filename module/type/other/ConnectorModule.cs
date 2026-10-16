public abstract partial class ConnectorModule : Module
{

    public abstract ISpawnableContextLifecycleDelegator CreateConnector(SpawnableContext context);

}