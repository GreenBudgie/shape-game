using System.Collections.Generic;

public partial class TetherModule : Module
{
    public override List<SpawnableStat> GetStats()
    {
        throw new System.NotImplementedException();
    }

    public override ModuleType Type => ModuleTypeRegistry.Tether;

}
