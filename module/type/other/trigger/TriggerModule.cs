using System;

public partial class TriggerModule : DelegatorModule
{

    public override ModuleType Type => ModuleTypeRegistry.Trigger;

    public override ISpawnableContextLifecycleDelegator CreateDelegator()
    {
        return new TriggerDelegator();
    }

}
