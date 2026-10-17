using System;

public partial class TetherModule : DelegatorModule
{
    public override ModuleType Type => ModuleTypeRegistry.Tether;

    public override ISpawnableContextLifecycleDelegator CreateDelegator()
    {
        return new TetherDelegator();
    }
}
